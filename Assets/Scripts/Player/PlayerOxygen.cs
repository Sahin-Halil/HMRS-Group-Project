using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerOxygen : MonoBehaviour
{
    [Header("UI References")]
    public Slider oxygenSlider;

    [Header("Oxygen Settings")]
    public float maxOxygen = 50f;
    public float currentOxygen;
    public float normalDrainRate = 1f;  // Static oxygen consumption
    public float movementDrainMultiplier = 2f;  // Accelerate when moving
    public UnityEvent onOxygenChanged;
    public UnityEvent onOxygenDepleted;

    private PlayerController playerController;  // Reference to movement script (avoid hard conflicts)
    private bool hasInvokedDepleted = false;  // Prevent repeated depletion events

    void Start()
    {
        currentOxygen = maxOxygen;
        playerController = GetComponent<PlayerController>();  
        onOxygenChanged?.Invoke();
        oxygenSlider.minValue = 0f;
        oxygenSlider.maxValue = maxOxygen;
        oxygenSlider.value = currentOxygen;
        currentOxygen = maxOxygen;  // Set initial oxygen to max (25s)
        playerController = GetComponent<PlayerController>();  // Soft reference to group member's controller
        onOxygenChanged?.Invoke();  // Initial UI refresh
    }

    void Update()
    {
        // Calculate drain rate based on movement
        float drainRate = normalDrainRate;

        // Apply drain and clamp
        currentOxygen -= Time.deltaTime * drainRate;
        currentOxygen = Mathf.Clamp(currentOxygen, 0, maxOxygen);
        oxygenSlider.value = currentOxygen;

        onOxygenChanged?.Invoke();

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
