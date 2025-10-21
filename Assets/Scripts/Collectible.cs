using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Collectible Type")]
    public CollectibleType type;  // Enum: Oxygen or ShipPart
    public float value = 10f;     // Oxygen adds 10s, ShipPart adds 1 unit
    public ParticleSystem collectEffect;  // Optional particle burst on pickup
    public AudioSource collectSound;      // Pickup sound effect

    [System.Serializable]
    public enum CollectibleType { Oxygen, ShipPart }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Ensure Player GameObject has "Player" tag
        {
            Collect(other.gameObject);
        }
    }

    private void Collect(GameObject player)
    {
        // Play feedback effects
        if (collectEffect != null) collectEffect.Play();
        if (collectSound != null) collectSound.Play();

        switch (type)
        {
            case CollectibleType.Oxygen:
                PlayerOxygen oxygen = player.GetComponent<PlayerOxygen>();
                if (oxygen != null)
                {
                    oxygen.RefillOxygen(value);  // Use Refill method from PlayerOxygen script
                    Debug.Log($"Oxygen +{value}s! Remaining: {oxygen.currentOxygen}s");  //debugging
                }
                break;
            case CollectibleType.ShipPart:
                ShipRepair repair = FindObjectOfType<ShipRepair>();  // Singleton-style find for repair manager
                if (repair != null)
                {
                    repair.AddPart(1);  // Add 1 ship part
                    Debug.Log("Ship part +1 collected!");  
                }
                break;
        }

        // Destroy the collectible (for performance in procedural spawns, consider Object Pooling instead)
        // Example pooling: Use a PoolManager to Deactivate() this instead of Destroy()
        Destroy(gameObject);
    }
}
