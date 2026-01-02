using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class DataLogUI : MonoBehaviour
{
    public static DataLogUI Instance;

    public GameObject dataLogPanel;
    public TMP_Text titleText;
    public TMP_Text contentText;
    public Button closeButton;

    private bool isShowing = false;
    private PlayerInput playerInput;
    private float savedXRotation;
    private float savedYRotation;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (dataLogPanel != null)
        {
            dataLogPanel.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseDataLog);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
        }
    }

    void Update()
    {
        // Also allow ESC or E to close
        if (isShowing && (Keyboard.current.escapeKey.wasPressedThisFrame ||
                          Keyboard.current.eKey.wasPressedThisFrame))
        {
            CloseDataLog();
        }
    }

    public void ShowDataLog(string title, string content)
    {
        if (dataLogPanel == null) return;

        isShowing = true;
        dataLogPanel.SetActive(true);

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (contentText != null)
        {
            contentText.text = content;
        }

        Time.timeScale = 0f;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                savedXRotation = controller.xRotation;
                savedYRotation = controller.yRotation;
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerInput != null)
        {
            playerInput.actions.Disable();
        }
    }

    public void CloseDataLog()
    {
        if (dataLogPanel != null)
        {
            dataLogPanel.SetActive(false);
        }

        isShowing = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.xRotation = savedXRotation;
                controller.yRotation = savedYRotation;
            }
        }

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerInput != null)
        {
            playerInput.actions.Enable();
        }
    }
}