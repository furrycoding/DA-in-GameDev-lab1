using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.SearchService;

public class ArenaSpawner : MonoBehaviour
{
    public Vector2 arenaSize = new Vector2(50, 50);

    public float spawnPointRadius = 8f;
    public float playerRadius = 1f;
    public float spawnY = 1.5f;

    public Vector2 spawnPointA, spawnPointB;
    [ObjectSelectorHandlerWithTags]
    public string teamATag, teamBTag;

    public ArenaPlayerAgent playerTemplate;


    private List<GameObject> obstacles = new List<GameObject>();
    private List<ArenaPlayerAgent> players = new List<ArenaPlayerAgent>();

    private SimpleMultiAgentGroup teamA, teamB;

    void Start()
    {
        if (Academy.IsInitialized)
            Academy.Instance.OnEnvironmentReset += OnEnvironmentReset;

        teamA = new SimpleMultiAgentGroup();
        teamB = new SimpleMultiAgentGroup();
        playerTemplate.gameObject.SetActive(false);
        OnEnvironmentReset();
    }

    void OnDestroy()
    {
        if (Academy.IsInitialized)
            Academy.Instance.OnEnvironmentReset -= OnEnvironmentReset;
    }

    public void TeamEliminated(int teamIndex)
    {
        Debug.Log($"Team {teamIndex} has been eliminated", this);
        teamA.EndGroupEpisode();
        teamB.EndGroupEpisode();
        OnEnvironmentReset();
    }

    private void OnEnvironmentReset()
    {
        FindNewSpawnPoints();
        GenerateObstacles();
        SpawnPlayers();
    }

    private void FindNewSpawnPoints()
    {
        var t = 360 * Random.value;
        var p = Quaternion.AngleAxis(t, Vector3.forward) * Vector3.right * Mathf.Sqrt(2);
        p.x = Mathf.Clamp(p.x, -1, 1);
        p.y = Mathf.Clamp(p.y, -1, 1);
        p *= arenaSize * Random.Range(0.6f, 0.9f);

        spawnPointA = (Vector2)p;
        spawnPointB = -spawnPointA;
    }

    private void GenerateObstacles()
    {
        var obstacleCount = Random.Range(20, 80);

        // Allocate more obstacles if we don't already have enough
        while (obstacles.Count < obstacleCount)
        {
            var newObstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObstacle.transform.parent = transform;
            newObstacle.tag = tag;
            obstacles.Add(newObstacle);
        }

        // Generate new rotations/positions/scales for obstacles and asssign them
        for (var i = 0; i < obstacleCount; i++)
        {
            var obstacle = obstacles[i];

            // Find a position that's far enough away from both spawn points
            var pos = Vector2.zero;
            for (var j = 0; j < 100; j++)
            {
                pos = arenaSize * new Vector2(2 * Random.value - 1, 2 * Random.value - 1);
                if (Vector2.Distance(pos, spawnPointA) < spawnPointRadius)
                    continue;
                if (Vector2.Distance(pos, spawnPointB) < spawnPointRadius)
                    continue;
                break;
            }

            var size = Random.Range(1f, 3f);
            var rotation = Random.Range(0, 360);

            obstacle.transform.localRotation = Quaternion.AngleAxis(rotation, Vector3.up);
            obstacle.transform.localScale = Vector3.one * size;
            obstacle.transform.localPosition = new Vector3(pos.x, 0.5f * size, pos.y);

            obstacle.SetActive(true);
        }

        // The rest of the obstacles aren't needed for now(but it's a good idea to still keep them around for performance)
        for (var i = obstacleCount; i < obstacles.Count; i++)
            obstacles[i].SetActive(false);
    }

    private void SpawnPlayers()
    {
        var playersPerTeam = Random.Range(4, 8);
        var totalPlayers = 2 * playersPerTeam;

        // Allocate more players if we don't already have enough
        while (players.Count < totalPlayers)
        {
            var newPlayer = Instantiate(playerTemplate.gameObject).GetComponent<ArenaPlayerAgent>();
            newPlayer.transform.parent = transform;
            players.Add(newPlayer);
        }

        // Spawn players
        var gridSize = Mathf.CeilToInt(Mathf.Sqrt(playersPerTeam));
        for (var i = 0; i < totalPlayers; i++)
        {
            // Figure out player's team, and grab that team's attributes
            var isTeamA = i < playersPerTeam;
            var tag = isTeamA ? teamATag : teamBTag;
            var index = isTeamA ? 0 : 1;
            var group = isTeamA ? teamA : teamB;
            var spawnPos = isTeamA ? spawnPointA : spawnPointB;

            // Pick a point where to spawn the player
            var y = i / gridSize;
            var x = i - y * gridSize;
            var pos = new Vector2(x, y) / (gridSize - 1);
            pos = (2 * pos - Vector2.one) * playerRadius * gridSize;
            pos += spawnPos;

            // Set properties of the player, and spawn it with SetActive
            var ply = players[i];
            ply.name = $"Team {index} Player {i % playersPerTeam}";
            ply.tag = tag;
            ply.teamIndex = index;
            ply.spawner = this;
            ply.transform.localPosition = new Vector3(pos.x, spawnY, pos.y);
            ply.GetComponent<Rigidbody>().velocity = Vector3.zero;
            
            ply.team?.UnregisterAgent(ply);
            ply.team = group;
            ply.team.RegisterAgent(ply);

            ply.gameObject.SetActive(true);
        }

        // Same deal as with obstacles
        for (var i = totalPlayers; i < players.Count; i++)
            players[i].gameObject.SetActive(false);
    }
}
