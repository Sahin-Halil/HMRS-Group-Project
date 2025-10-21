using UnityEngine;
using UnityEngine.Events;

public class PlayerOxygen : MonoBehaviour
{
    [Header("Oxygen Settings")]
    public float maxOxygen = 25f;  // Initial timer: 25 seconds for urgency
    public float currentOxygen;
    public float normalDrainRate = 0.5f;  // Base oxygen consumption per second (static)
    public float movementDrainMultiplier = 2f;  // Multiplier for movement/jumping drain
    public UnityEvent onOxygenChanged;  // Invoked when oxygen value changes (for UI updates)
    public UnityEvent onOxygenDepleted;  // Triggered when oxygen hits zero (game over)

    private PlayerController playerController;  // Reference to movement script (avoid hard conflicts)
    private bool hasInvokedDepleted = false;  // Prevent repeated depletion events

    void Start()
    {
        currentOxygen = maxOxygen;  // Set initial oxygen to max (25s)
        playerController = GetComponent<PlayerController>();  // Soft reference to group member's controller
        onOxygenChanged?.Invoke();  // Initial UI refresh
    }

    void Update()
    {
        // Calculate drain rate based on movement
        float drainRate = normalDrainRate;
        if (playerController != null && (playerController.isMoving || playerController.isJumping))
        {
            drainRate *= movementDrainMultiplier;  // Accelerate drain during activity
        }

        // Apply drain and clamp
        currentOxygen -= Time.deltaTime * drainRate;
        currentOxygen = Mathf.Clamp(currentOxygen, 0, maxOxygen);

        // Invoke change event only if value actually changed (optimize UI calls)
        if (Mathf.Approximately(Time.deltaTime, 0) == false)  // Simple change check via delta
        {
            onOxygenChanged?.Invoke();
        }

        // Handle depletion once
        if (currentOxygen <= 0 && !hasInvokedDepleted)
        {
            hasInvokedDepleted = true;
            onOxygenDepleted?.Invoke();  // Link to pause menu or lose scene
            Debug.Log("Oxygen depleted! Emergency shutdown initiated.");
        }
    }

    public void RefillOxygen(float amount)
    {
        if (amount <= 0) return;  // Safety check

        float previousOxygen = currentOxygen;
        currentOxygen += amount;
        currentOxygen = Mathf.Clamp(currentOxygen, 0, maxOxygen);  // Cap at max to prevent overflow

        // Only invoke if changed
        if (!Mathf.Approximately(previousOxygen, currentOxygen))
        {
            onOxygenChanged?.Invoke();
            Debug.Log($"Oxygen refilled by {amount}s! Current: {currentOxygen}s");
        }
    }

    public float GetOxygenPercentage() => currentOxygen / maxOxygen;  // For UI slider (0-1 normalized)
}
