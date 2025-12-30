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

    // Makes sure that game is not paused when starting
    private void Start()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 0f; // Pause game until they click Start
        hasWon = false;
        
        // Get the Pause action from the PlayerInput component and subscribe to it
        pauseAction = playerInput.actions["Pause"];
        pauseAction.performed += ctx => OnPause();

        OpenMainMenu();
    }

    private void OnDestroy()
    {
        // Unsubscribe from the action when the object is destroyed
        if (pauseAction != null)
        {
            pauseAction.performed -= ctx => OnPause();
        }
    }

    // Handles pause toggling (can be called from UI buttons or input action)
    public void OnPause()
    {
        // Check if objects still exist (they might be destroyed during scene reload)
        if (pauseMenuUI == null || playerDeath == null) return;
        
        if (!playerDeath.checkDead() && !hasWon)
        {
            if (isPaused) { ResumeGame(); }
            else { PauseGame(); }
        }
    }

    // Pauses game by stopping timescale to 0, and locking all player movement
    public void PauseGame()
    {
        if (pauseMenuUI == null || playerInput == null) return;
        
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerInput.actions.Disable();
    }

    // Resumes game by undoing all the actions taken by PauseGame()
    public void ResumeGame()
    {
        if (pauseMenuUI == null || settingsMenuUI == null || playerInput == null) return;
        
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerInput.actions.Enable();
    }

    // Exits the game or returns to the title screen
    public void ExitGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
        Time.timeScale = 1f; // Reset time scale before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
