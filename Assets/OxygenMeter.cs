using UnityEngine;
using UnityEngine.UI;

public class OxygenMeter : MonoBehaviour
{
    public float maxOxygen = 100f;
    public float currentOxygen;
    public Slider oxygenSlider;  // UI Slider for oxygen bar

    private void Start()
    {
        currentOxygen = maxOxygen;
        oxygenSlider.maxValue = maxOxygen;
        oxygenSlider.value = currentOxygen;
    }

    private void Update()
    {
        // Decrease oxygen over time (adjust the rate as needed)
        if (currentOxygen > 0)
        {
            currentOxygen -= Time.deltaTime;  // Decrease by 1 unit per second
            oxygenSlider.value = currentOxygen;
        }
        else
        {
            currentOxygen = 0;
            // You can trigger death or game over here
            Debug.Log("Out of oxygen!");
        }
    }

    // You can create a method to refill oxygen
    public void RefillOxygen(float amount)
    {
        currentOxygen = Mathf.Min(currentOxygen + amount, maxOxygen);
        oxygenSlider.value = currentOxygen;
    }
}

