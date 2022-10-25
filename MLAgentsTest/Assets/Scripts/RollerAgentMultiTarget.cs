using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RollerAgentMultiTarget : Agent
{
    public Transform[] Targets;

    private int currentTarget = 0;
    private Rigidbody rBody;
    
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        if (this.transform.localPosition.y < 0)
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        foreach (var target in Targets)
            target.localPosition = new Vector3(Random.value * 8-4, 0.5f, Random.value * 8-4);
        currentTarget = 0;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Targets[currentTarget].localPosition);
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }
    public float forceMultiplier = 10;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);

        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Targets[currentTarget].localPosition);
        if(distanceToTarget < 1.42f)
        {
            currentTarget = (currentTarget + 1) % Targets.Length;
            if (currentTarget == 0)
            {
                SetReward(1.0f);
                EndEpisode();
            }
            else
                SetReward(0.1f);
        }
        else if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }
}
