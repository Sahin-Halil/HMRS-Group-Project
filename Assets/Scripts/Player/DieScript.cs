using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DieScript : MonoBehaviour
{
    [SerializeField] private GameObject respawnMenuUI;
    private PlayerInput playerInput;
    private bool isDead = false;

    void Awake() {
        playerInput = GetComponent<PlayerInput>();
    }

    public void Die() {
        toggleDeathStatus();
        respawnMenuUI.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerInput.actions.Disable();
    }

    public void toggleDeathStatus() {
        isDead = !isDead;
    }

    public bool checkDead() {
        return isDead;
    }
}
