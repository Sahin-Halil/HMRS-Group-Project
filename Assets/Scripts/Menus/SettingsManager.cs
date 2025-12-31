using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    // UI references and player controller
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject pauseMenuUI;
    
    // Sensitivity sliders - one for main menu, one for pause menu
    [SerializeField] private Slider mainMenuSensitivitySlider;
    [SerializeField] private Slider pauseMenuSensitivitySlider;
    
    [SerializeField] private PlayerController controller;

    // Initializes settings and loads saved sensitivity
    private void Start()
    {
        if (settingsMenuUI != null)
        {
            settingsMenuUI.SetActive(false);
        }

        float savedSense = PlayerPrefs.GetFloat("MouseSensitivity", 0.5f);

        // Apply saved sensitivity to player controller
        if (controller != null)
        {
            controller.SetSensitivity(savedSense);
        }

        // Initialize both sliders if they exist
        if (mainMenuSensitivitySlider != null)
        {
            mainMenuSensitivitySlider.SetValueWithoutNotify(savedSense);
            mainMenuSensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }

        if (pauseMenuSensitivitySlider != null)
        {
            pauseMenuSensitivitySlider.SetValueWithoutNotify(savedSense);
            pauseMenuSensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (mainMenuSensitivitySlider != null)
        {
            mainMenuSensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
        }

        if (pauseMenuSensitivitySlider != null)
        {
            pauseMenuSensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
        }
    }

    // Handles sensitivity slider changes and saves preference
    // This works for both mouse and controller since it's a universal sensitivity value
    private void OnSensitivityChanged(float value)
    {
        if (value <= 0f) value = 0.01f;

        // Update the player controller
        if (controller != null)
        {
            controller.SetSensitivity(value);
        }

        // Sync both sliders to the same value (without triggering their listeners again)
        if (mainMenuSensitivitySlider != null && !Mathf.Approximately(mainMenuSensitivitySlider.value, value))
        {
            mainMenuSensitivitySlider.SetValueWithoutNotify(value);
        }

        if (pauseMenuSensitivitySlider != null && !Mathf.Approximately(pauseMenuSensitivitySlider.value, value))
        {
            pauseMenuSensitivitySlider.SetValueWithoutNotify(value);
        }

        // Save the preference
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
    }
}
