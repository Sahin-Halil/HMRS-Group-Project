using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Components
    private CharacterController characterController;
    public Camera characterCamera;
    [SerializeField] private ShipPartManager shipPartManager;

    // Movement
    [SerializeField] private float standingSpeed = 5f;
    [SerializeField] private Vector3 move;
    [SerializeField] private float xMove;
    [SerializeField] private float yMove;

    // Mouse look
    [SerializeField] private float mouseSense = 0.5f;
    [SerializeField] public float xRotation;
    [SerializeField] public float yRotation;

    // Crouch
    [SerializeField] private bool crouchInput = false;
    private float crouchSpeed;
    private float crouchHeight;


    // Called when movement input is detected
    private void OnMove(InputValue value)
    {
        Vector2 moveInput = value.Get<Vector2>();
        xMove = moveInput.x;
        yMove = moveInput.y;
    }

    // Called when mouse input is detected
    private void OnLook(InputValue value)
    {
        Vector2 mouseInput = value.Get<Vector2>();
        xRotation = xRotation + (mouseInput.x * mouseSense);
        yRotation = Mathf.Clamp(yRotation - (mouseInput.y * mouseSense), -90f, 90f);
    }

    // Handles crouch toggling
    private void OnCrouch()
    {
        crouchInput = !crouchInput;
        characterController.height -= crouchHeight;
        crouchHeight *= -1f;
    }

    // Detects collisions with collectible ship parts
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("ShipPart"))
        {
            shipPartManager.AddPart();
            Destroy(collider.gameObject);
        }
    }

    // Setup components and values
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        crouchSpeed = 0.5f * standingSpeed;
        crouchHeight = 0.25f * characterController.height;
        mouseSense = PlayerPrefs.GetFloat("MouseSensitivity", mouseSense);
    }

    // Handles movement and rotation each frame
    void Update()
    {
        float currentSpeed = crouchInput ? crouchSpeed : standingSpeed;
        move = transform.right * xMove + transform.forward * yMove;

        // Prevent diagonal speed boost
        if (move.magnitude > 1)
        {
            move.Normalize();
        }

        // Apply calculated movement
        characterController.SimpleMove(move * currentSpeed);

        // Apply rotation for camera and player
        transform.rotation = Quaternion.Euler(0f, xRotation, 0f);
        characterCamera.transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);
    }

    // Below are required for sensitivity slider in settings
    // Getter for sensitivity
    public float GetSensitivity() => mouseSense;

    // Setter for sensitivity
    public void SetSensitivity(float sensitivity) => mouseSense = sensitivity;

    // Returns reference to ShipPartManager
    public ShipPartManager GetShipPartManager()
    {
        return shipPartManager;
    }
}
