using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class ArenaPlayerAgent : Agent
{
    public int hitsToDeath = 3;
    public float movementForce = 8;
    public float rotationSpeed = 10;
    public int shotCooldown = 100;
    public int maxStepsAlive = 10000;

    public ArenaEffects effectManager;


    [HideInInspector]
    [NonSerialized]
    public int teamIndex;
    [HideInInspector]
    [NonSerialized]
    public SimpleMultiAgentGroup team;
    [HideInInspector]
    [NonSerialized]
    public ArenaSpawner spawner;

    private Rigidbody body;
    private int remainingHits = 3;
    private bool alive = true;

    private bool lastShoot = false;
    private int remainingCooldown = 0;
    private int remainingTime = 0;

    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        remainingHits = hitsToDeath;
        alive = true;
        remainingCooldown = 0;
        remainingTime = maxStepsAlive;
    }

    public override void OnEpisodeBegin()
    {
        remainingHits = hitsToDeath;
        alive = true;
        remainingCooldown = 0;
        remainingTime = maxStepsAlive;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var closestTeammatePos = Vector3.zero;
        var closestDistSqr = 2f;

        if (team != null)
            foreach (var agent in team.GetRegisteredAgents())
            {
                var obj = agent.gameObject;
                var relativePos = transform.InverseTransformPoint(obj.transform.position);

                if (relativePos.sqrMagnitude < closestDistSqr)
                {
                    closestTeammatePos = relativePos;
                    closestDistSqr = relativePos.sqrMagnitude;
                }
            }

        sensor.AddObservation(body?.velocity ?? Vector3.zero);
        sensor.AddObservation(closestTeammatePos);
        sensor.AddOneHotObservation(teamIndex, 4);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (!alive)
            return;

        if ((team == null) || (body == null))
            return;

        var moveForward = actionBuffers.ContinuousActions[0];
        var strafeRight = actionBuffers.ContinuousActions[1];
        var turn = actionBuffers.ContinuousActions[2];
        var shoot = actionBuffers.DiscreteActions[0] > 0;

        if (remainingCooldown > 0)
            remainingCooldown--;
        else if (shoot && !lastShoot && remainingCooldown <= 0)
        {
            Shoot();
            remainingCooldown = shotCooldown;
        }
        lastShoot = shoot;

        var move = Vector3.forward * moveForward + Vector3.right * strafeRight;
        move.Normalize();
        body.AddRelativeForce(movementForce * move);

        turn = Mathf.Clamp(turn, -1, 1) * rotationSpeed;
        transform.Rotate(Vector3.up, turn);

        if (transform.position.y < -5)
            Die();

        remainingTime--;
        if (remainingTime <= 0)
            Die(false);
    }

    public bool OnHit()
    {
        if (!alive)
            return false;

        remainingHits--;
        if (remainingHits > 0)
            return false;

        Die();
        return true;
    }

    private void Shoot()
    {
        // Debug.Log($"{gameObject.name} took a shot", this);

        var maxLength = 15f;

        var didHit = Physics.Raycast(
            transform.position, transform.forward,
            out RaycastHit hit, maxLength,
            LayerMask.GetMask("Default"),
            QueryTriggerInteraction.Ignore
        );

        if (!didHit)
        {
            effectManager?.ShotLine(transform.position, transform.position + transform.forward * maxLength);
            return;
        }

        effectManager?.ShotLine(transform.position, hit.point);

        var hitPlayer = hit.collider?.gameObject?.GetComponent<ArenaPlayerAgent>();
        if (hitPlayer == null)
        {
            effectManager?.HitWall(hit.point);
            return;
        }

        effectManager?.HitPlayer(hit.point);
        if (hitPlayer.OnHit())
            team?.AddGroupReward(50);
    }

    private void Die(bool shouldPunish=true)
    {
        Debug.Log($"{gameObject.name} died", this);

        alive = false;
        if (shouldPunish)
            team?.AddGroupReward(-1);
        effectManager?.PlayerDeath(transform.position);

        gameObject.SetActive(false);


        if (team == null)
            return;

        // If all agents are dead, end the episode
        var everyoneDead =
            team.GetRegisteredAgents()
                .All(agent => !((ArenaPlayerAgent)agent).alive);
        
        if (everyoneDead)
            spawner.TeamEliminated(teamIndex);
    }
}
