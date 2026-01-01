using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    // UI references and player controller
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private PlayerController controller;

    // Initializes settings and loads saved sensitivity
    private void Start()
    {
        settingsMenuUI.SetActive(false);

        float savedSense = PlayerPrefs.GetFloat("MouseSensitivity", 0.5f);

        if (controller != null)
        {
            controller.SetSensitivity(savedSense);
        }

        sensitivitySlider.SetValueWithoutNotify(savedSense);
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    // Handles sensitivity slider changes and saves preference
    private void OnSensitivityChanged(float value)
    {
        if (value <= 0f) value = 0.01f;

        if (controller != null)
            controller.SetSensitivity(value);

        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
    }
}
