using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private PlayerController controller;
    public static float mouseSense = 0.5f;

    private void Start()
    {
        settingsMenuUI.SetActive(false);
        // Set sens to 0.5 by default
        sensitivitySlider.value = controller != null ? controller.GetSensitivity() : 0.5f;

        // Make slider listen for changes
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    private void OnSensitivityChanged(float value)
    {
        if (controller != null)
        {
            if (value == 0) controller.SetSensitivity(0.01f);

            else
            {
                controller.SetSensitivity(value);
            }
        }
    }
}
