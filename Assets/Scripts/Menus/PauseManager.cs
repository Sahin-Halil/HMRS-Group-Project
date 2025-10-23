using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    // UI references and player components
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject settingsMenuUI;
    [SerializeField] private GameObject winMenuUI;
    [SerializeField] private GameObject notEnoughPartsUI;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private DieScript playerDeath;

    // State tracking
    private bool isPaused = false;
    private bool hasWon = false;

    // Makes sure that game is not paused when starting
    private void Start()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        hasWon = false;
    }

    // Checks every frame to see if the player has pressed the escape key to open the settings menu
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame && !playerDeath.checkDead() && !hasWon)
        {
            if (isPaused) { ResumeGame(); }
            else { PauseGame(); }
        }
    }

    // Pauses game by stopping timescale to 0, and locking all player movement
    public void PauseGame()
    {
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
        Application.Quit();
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
}
