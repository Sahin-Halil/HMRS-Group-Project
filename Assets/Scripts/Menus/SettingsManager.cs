using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private PlayerController controller;

    private void Start()
    {
        settingsMenuUI.SetActive(false);

        float savedSense = PlayerPrefs.GetFloat("MouseSensitivity", 0.5f);

        if (controller!=null)
        {
            controller.SetSensitivity(savedSense);
        }

        sensitivitySlider.SetValueWithoutNotify(savedSense);

        // Make slider listen for changes
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    private void OnSensitivityChanged(float value)
    {
        if (value <= 0f) value = 0.01f;

        if (controller != null)
            controller.SetSensitivity(value);

        // Save for next session
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
    }
}
