using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DieScript : MonoBehaviour
{
    // Attributes
    [SerializeField] private GameObject respawnMenuUI;
    private PlayerInput playerInput;
    private bool isDead = false;

    // Setup references
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    // Handles player death logic and UI activation
    public void Die()
    {
        toggleDeathStatus();
        respawnMenuUI.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerInput.actions.Disable();
    }

    // Toggles death state
    public void toggleDeathStatus()
    {
        isDead = !isDead;
    }

    // Returns current death state
    public bool checkDead()
    {
        return isDead;
    }
}
