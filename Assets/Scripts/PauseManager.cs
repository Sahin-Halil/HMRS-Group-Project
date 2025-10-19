using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private PlayerInput playerInput;

    private bool isPaused = false;

    // Makes sure that game is not paused when starting
    private void Start()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    // Checks every frame to see if the player has pressed the escape key to open the settings menu
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) { ResumeGame(); }

            else { PauseGame(); }
        }
    }

    // Pauses game by stopping timescale to 0, and locking all player movement
    public void PauseGame() {
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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerInput.actions.Enable();
    }

    public void ExitGame()
    {
        // Need to make this return to Title Screen when we create it
        Application.Quit();
    }
}
