using UnityEngine;
using UnityEngine.UI;
using System.IO;

[System.Serializable]
public class BarConfig
{
    public RectTransformData healthBar;
    public RectTransformData oxygenBar;
}

[System.Serializable]
public class RectTransformData  
{
    public float x, y, width, height;
}

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthSlider;
    public Slider oxygenSlider;
    public Image healthFill;  
    public Image oxygenFill;

    private PlayerHealth health;
    private PlayerOxygen oxygen;
    private BarConfig config;

    void Start()
    {
        health = FindObjectOfType<PlayerHealth>();  
        oxygen = FindObjectOfType<PlayerOxygen>();
        
        LoadUIConfig();  
        SetupEvents();   
        
        // Initial update
        UpdateHealthBar();
        UpdateOxygenBar();
    }

    void LoadUIConfig()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("UIConfig");  // Place the "Resources" folder
        if (jsonText != null)
        {
            config = JsonUtility.FromJson<BarConfig>(jsonText.text);
            // Application Domain Placement
            if (healthSlider != null)
            {
                RectTransform rt = healthSlider.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(config.healthBar.x, config.healthBar.y);
                rt.sizeDelta = new Vector2(config.healthBar.width, config.healthBar.height);
            }
        }
    }

    void SetupEvents()
    {
        if (health != null) health.onHealthChanged.AddListener(UpdateHealthBar);
        if (oxygen != null) oxygen.onOxygenChanged.AddListener(UpdateOxygenBar);
        
        // Link to the pause menu
        if (health != null) health.onDeath.AddListener(() => Time.timeScale = 0);  
        if (oxygen != null) oxygen.onOxygenDepleted.AddListener(() => Time.timeScale = 0);
    }

    void UpdateHealthBar()
    {
        if (health != null && healthSlider != null)
        {
            healthSlider.value = health.GetHealthPercentage();
            healthFill.color = health.currentHealth < 30 ? Color.red : Color.green;
        }
    }

    void UpdateOxygenBar()
    {
        if (oxygen != null && oxygenSlider != null)
        {
            oxygenSlider.value = oxygen.GetOxygenPercentage();
            oxygenFill.color = oxygen.currentOxygen < 20 ? Color.red : Color.blue;
        }
    }

    void OnDestroy()
    {
        if (health != null) health.onHealthChanged.RemoveListener(UpdateHealthBar);
    }
}
