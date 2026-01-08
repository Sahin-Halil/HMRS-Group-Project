// Under review - currently attempting to logically separate ground from walls
// Walls have been designed as raised terrain so lamps can spawn in walls
using System.Collections.Generic;
using UnityEngine;

public class OxygenLampSpawner : MonoBehaviour
{
    // Spawn Settings
    public GameObject oxygenLampPrefab;
    public int minLamps = 10;
    public int maxLamps = 20;
    public float spawnHeight = 1f;

    // Spawn Area
    public Vector3 spawnAreaCenter = Vector3.zero;
    public Vector3 spawnAreaSize = new Vector3(100f, 0f, 100f);

    // Spacing Rules
    public float minDistanceBetweenLamps = 15f;
    public float minDistanceFromStart = 20f;
    public LayerMask groundLayer;
    public LayerMask obstacleLayer; // Walls, consoles, etc to avoid

    // Clustered Spawning
    public bool useClusteredSpawning = true;
    public int numberOfClusters = 5;
    public float clusterRadius = 10f;
    public int lampsPerCluster = 3;

    // Debug
    public bool showDebugGizmos = true;
    public Color gizmosColor = Color.cyan;

    private List<Vector3> spawnedPositions = new List<Vector3>();
    private Transform playerStart;

    void Start()
    {
        // Find player starting position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStart = player.transform;
        }

        SpawnOxygenLamps();
    }

    void SpawnOxygenLamps()
    {
        if (oxygenLampPrefab == null)
        {
            Debug.LogError("Oxygen Lamp Prefab not assigned!");
            return;
        }

        spawnedPositions.Clear();

        if (useClusteredSpawning)
        {
            SpawnClustered();
        }
        else
        {
            SpawnRandom();
        }

        Debug.Log($"Spawned {spawnedPositions.Count} oxygen lamps");
    }

    void SpawnRandom()
    {
        int targetLamps = Random.Range(minLamps, maxLamps + 1);
        int attempts = 0;
        int maxAttempts = targetLamps * 20;

        while (spawnedPositions.Count < targetLamps && attempts < maxAttempts)
        {
            attempts++;

            Vector3 randomPos = GetRandomPositionInArea();

            if (IsValidSpawnPosition(randomPos))
            {
                SpawnLampAt(randomPos);
            }
        }

        Debug.Log($"Random spawn completed in {attempts} attempts");
    }

    void SpawnClustered()
    {
        // Create cluster centers
        List<Vector3> clusterCenters = new List<Vector3>();
        int attempts = 0;
        int maxAttempts = numberOfClusters * 20;

        while (clusterCenters.Count < numberOfClusters && attempts < maxAttempts)
        {
            attempts++;

            Vector3 centerPos = GetRandomPositionInArea();

            // Make sure cluster centers are far apart
            bool tooClose = false;
            foreach (Vector3 existingCenter in clusterCenters)
            {
                if (Vector3.Distance(centerPos, existingCenter) < clusterRadius * 3f)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose && IsValidSpawnPosition(centerPos))
            {
                clusterCenters.Add(centerPos);
            }
        }

        Debug.Log($"Created {clusterCenters.Count} cluster centers");

        // Spawn lamps around each cluster
        foreach (Vector3 center in clusterCenters)
        {
            int lampsInThisCluster = Random.Range(lampsPerCluster - 1, lampsPerCluster + 2);

            for (int i = 0; i < lampsInThisCluster; i++)
            {
                // Random position within cluster radius
                Vector2 randomCircle = Random.insideUnitCircle * clusterRadius;
                Vector3 lampPos = center + new Vector3(randomCircle.x, 0, randomCircle.y);

                if (IsValidSpawnPosition(lampPos))
                {
                    SpawnLampAt(lampPos);
                }
            }
        }
    }

    Vector3 GetRandomPositionInArea()
    {
        float randomX = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
        float randomZ = Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f);

        Vector3 randomPos = spawnAreaCenter + new Vector3(randomX, spawnHeight, randomZ);

        RaycastHit hit;
        if (Physics.Raycast(randomPos + Vector3.up * 50f, Vector3.down, out hit, 100f, groundLayer))
        {
            return hit.point + Vector3.up * spawnHeight;
        }

        return randomPos;
    }

    bool IsValidSpawnPosition(Vector3 position)
    {
        // Check distance from player start
        if (playerStart != null)
        {
            float distanceFromStart = Vector3.Distance(position, playerStart.position);
            if (distanceFromStart < minDistanceFromStart)
            {
                return false;
            }
        }

        // Check distance from other lamps
        foreach (Vector3 existingPos in spawnedPositions)
        {
            if (Vector3.Distance(position, existingPos) < minDistanceBetweenLamps)
            {
                return false;
            }
        }

        // Check for obstacles
        Collider[] obstacles = Physics.OverlapSphere(position, 2f, obstacleLayer);
        if (obstacles.Length > 0)
        {
            return false;
        }

        // Check if there's ground beneath
        RaycastHit hit;
        if (!Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out hit, 5f, groundLayer))
        {
            return false;
        }

        return true;
    }

    void SpawnLampAt(Vector3 position)
    {
        GameObject lamp = Instantiate(oxygenLampPrefab, position, Quaternion.identity, transform);
        lamp.name = $"OxygenLamp_{spawnedPositions.Count}";
        spawnedPositions.Add(position);
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Gizmos.color = gizmosColor;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            foreach (Vector3 pos in spawnedPositions)
            {
                Gizmos.DrawWireSphere(pos, 1f);
            }
        }

        if (useClusteredSpawning)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        }
    }

    public void RespawnLamps()
    {
        // Destroy existing lamps
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        spawnedPositions.Clear();
        SpawnOxygenLamps();
    }
}