using UnityEngine;
using UnityEngine.UI;

public class PlayerOxygen : MonoBehaviour
{
    // Oxygen and UI references
    [SerializeField] private Slider oxygenSlider;
    [SerializeField] private float maxOxygen = 50f;
    [SerializeField] private DieScript dieScript;

    // Players oxygen level
    private float currentOxygen;

    // Initialize oxygen values and UI
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

    // Handles oxygen depletion and death trigger
    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.IsGameplayLocked())
            return;

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

    // Refills oxygen by a given amount
    public void RefillOxygen(float amount)
    {
        if (currentOxygen + amount > maxOxygen)
        {
            currentOxygen = maxOxygen;
            return;
        }

        currentOxygen += amount;
    }
}
