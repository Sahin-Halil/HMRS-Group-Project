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

    private PlayerController playerController;  

    void Start()
    {
        currentOxygen = maxOxygen;
        playerController = GetComponent<PlayerController>();  
        onOxygenChanged?.Invoke();
        oxygenSlider.minValue = 0f;
        oxygenSlider.maxValue = maxOxygen;
        oxygenSlider.value = currentOxygen;
    }

    void Update()
    {
        float drainRate = normalDrainRate;
        //if (playerController != null && (playerController.isMoving || playerController.isJumping))  // Linked mobility
        //    drainRate *= movementDrainMultiplier;

        currentOxygen -= Time.deltaTime * drainRate;
        currentOxygen = Mathf.Clamp(currentOxygen, 0, maxOxygen);
        oxygenSlider.value = currentOxygen;

        onOxygenChanged?.Invoke();

        if (currentOxygen <= 0)
        {
            onOxygenDepleted?.Invoke();  
            Debug.Log("Oxygen depleted! Emergency shutdown.");
        }
    }

    public void RefillOxygen(float amount)
    {
        currentOxygen += amount;
        currentOxygen = Mathf.Clamp(currentOxygen, 0, maxOxygen);
        onOxygenChanged?.Invoke();
    }

    public float GetOxygenPercentage() => currentOxygen / maxOxygen;
}
