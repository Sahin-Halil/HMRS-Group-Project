using UnityEngine;

public class CollectibleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject oxygenPrefab;  // Drag your OxygenCan prefab here
    public GameObject partPrefab;    // Drag your ShipPart prefab here
    public int spawnCount = 5;       // Total items to spawn (mix of oxygen/parts)
    public float spawnRadius = 50f;  // Max distance from spawner (tweak for map size)

    void Start()
    {
        SpawnCollectibles();
    }

    private void SpawnCollectibles()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            // Random position in a circle around spawner (more natural than full square)
            Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
            randomDirection += transform.position;  // Offset from spawner's position
            randomDirection.y = 1f;  // Keep on ground level

            // 50/50 chance: oxygen or part
            GameObject prefabToSpawn = (Random.value > 0.5f) ? oxygenPrefab : partPrefab;
            Instantiate(prefabToSpawn, randomDirection, Quaternion.identity);
        }

        Debug.Log($"Spawned {spawnCount} collectibles! Explore to find them.");  // Quick feedback
    }
}

