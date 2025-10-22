using UnityEngine;
using UnityEngine.UI;

public class PlayerOxygen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider oxygenSlider;

    [Header("Oxygen Settings")]
    [SerializeField] private float maxOxygen = 50f;

    [Header("Respawn Handler")]
    [SerializeField] private DieScript dieScript; 
        
    private float currentOxygen;

    private void Start()
    {
        currentOxygen = maxOxygen;

        if (oxygenSlider != null)
        {
            oxygenSlider.minValue = 0f;
            oxygenSlider.maxValue = maxOxygen;
            oxygenSlider.value = currentOxygen;
        }
    }

    private void Update()
    {
        // Drain oxygen over time
        currentOxygen -= Time.deltaTime;
        if (currentOxygen < 0f) currentOxygen = 0f;
        else if (currentOxygen > maxOxygen) currentOxygen = maxOxygen;


        if (oxygenSlider != null)
            oxygenSlider.value = currentOxygen;

        if (currentOxygen <= 0f)
        {
            dieScript.Die();
        }
    }

    public void RefillOxygen(float amount)
    {
        if (currentOxygen+amount>maxOxygen)
        {
            currentOxygen = maxOxygen;
            return;
        }

        currentOxygen += amount;
    }
}
