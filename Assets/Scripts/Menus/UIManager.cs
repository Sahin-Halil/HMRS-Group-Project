using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // UI references and player components
    [SerializeField] private GameObject startMenuUI;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject winMenuUI;
    [SerializeField] private GameObject notEnoughPartsUI;
    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject startSettingsMenuUI;
    [SerializeField] private GameObject creditsMenuUI;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private DieScript playerDeath;

    // Input Actions
    private InputAction pauseAction;

    // State tracking
    private bool isPaused = false;
    private bool hasWon = false;

    // Static flag to track if this is a restart (persists across scene reloads)
    private static bool isRestarting = false;

    // Makes sure that game is not paused when starting
    private void Start()
    {
        pauseMenuUI.SetActive(false);
        hasWon = false;

        // Get the Pause action from the PlayerInput component and subscribe to it
        pauseAction = playerInput.actions["Pause"];
        pauseAction.performed += OnPause;

        // Check if this scene load is due to a checkpoint respawn
        if (GameManager.Instance != null && GameManager.Instance.IsRespawning())
        {
            // Respawn flow → skip menus entirely
            StartGame();
            return;
        }

        // If restarting, skip the main menu and start the game directly
        if (isRestarting)
        {
            isRestarting = false;
            StartGame();
        }
        else
        {
            // First time loading - show the start menu
            Time.timeScale = 0f; // Pause game until they click Start
            OpenMainMenu();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the action when the object is destroyed
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPause;
        }
    }

    // Handles pause toggling (can be called from UI buttons or input action)
    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (playerDeath.checkDead() || hasWon) return;

        if (!isPaused)
            PauseGame();
        else
            ResumeGame();
    }


    // Pauses game by stopping timescale to 0, and locking all player movement
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Resumes game by undoing all the actions taken by PauseGame()
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerInput.actions.Enable();
    }

    // Exits the game or returns to the title screen
    public void ExitGame()
    {
        SceneManager.LoadScene("Main");
        OpenMainMenu();
    }

    // Opens settings menu from pause menu
    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerInput.actions.Disable();
        settingsMenuUI.SetActive(true);
    }

    // Closes settings menu and returns to pause menu
    public void CloseSettings()
    {
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    // Restarts the current scene
    public void RestartGame()
    {
        isRestarting = true; // Set flag so Start() knows this is a restart
        Time.timeScale = 1f; // Reset time scale before reloading
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    // Handles win state and displays win menu
    public void Win()
    {
        hasWon = true;
        Time.timeScale = 0f;
        winMenuUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerInput.actions.Disable();
    }

    // Toggles the 'Not Enough Parts' UI display
    public void NotEnoughParts(bool value)
    {
        notEnoughPartsUI.SetActive(value);
    }

    public bool getPauseState()
    {
        return isPaused;
    }

    public void StartGame()
    {
        startMenuUI.SetActive(false);
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(false);
        winMenuUI.SetActive(false);
        notEnoughPartsUI.SetActive(false);
        creditsMenuUI.SetActive(false);
        startSettingsMenuUI.SetActive(false);

        startMenuUI.SetActive(false);
        Time.timeScale = 1f; // Unpause the game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        HUD.SetActive(true);

        playerInput.actions.Enable();
    }

    public void OpenMainMenu()
    {
        startMenuUI.SetActive(true);
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(false);
        winMenuUI.SetActive(false);
        notEnoughPartsUI.SetActive(false);
        creditsMenuUI.SetActive(false);
        HUD.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerInput.actions.Disable();
    }

    public void OpenStartSettings()
    {
        startSettingsMenuUI.SetActive(true);
        startMenuUI.SetActive(false);
    }

    public void CloseStartSettings()
    {
        startMenuUI.SetActive(true);
        startSettingsMenuUI.SetActive(false);
    }

    public void OpenCredits()
    {
        startMenuUI.SetActive(false);
        creditsMenuUI.SetActive(true);
    }
}