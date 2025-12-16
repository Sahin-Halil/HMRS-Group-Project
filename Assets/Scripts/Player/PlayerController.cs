using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

// 3 issues
// Player can slide in any direction (simple isSlide check in update and extra variable for slideDirection)
// Player can slide by crouching first (add check in crouch to see if run was activated first - use this new boolean to only allow slide when run is activated first)
// This one is less of an issue but slide can last way too long either add a timer or multiply slide decrease (maybe have a check for both)

public class PlayerController : MonoBehaviour
{
    // Components
    private CharacterController characterController;
    public Camera characterCamera;
    [SerializeField] private ShipPartManager shipPartManager;

    // Movement
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private Vector3 move;
    [SerializeField] private float xMove;
    [SerializeField] private float yMove;

    // Mouse look
    [SerializeField] private float mouseSense = 0.5f;
    [SerializeField] private float xRotation;
    [SerializeField] private float yRotation;

    // Crouch
    private bool crouchInput = false;
    private float crouchSpeed;
    private float crouchHeight;

    // Running 
    private bool runInput = false;
    private float runSpeed;

    // Sliding
    private bool canSlide = false;
    private bool isSliding = false;
    private Vector3 slideDirection; 
    private float startSlideSpeed;
    private float currentSlideSpeed;

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

        // Start slide ONLY if run was active first
        if (runInput && crouchInput) 
        {
            canSlide = true;
        }
    }

    // Handles Run toggling
    private void OnRun() 
    {
        runInput = !runInput;
    }

    // Handles start slide motion
    private void startSlide()
    {
        if (isSliding) return;
        isSliding = true;
        currentSlideSpeed = startSlideSpeed;
        slideDirection = transform.right * xMove +transform.forward * yMove;
    }

    // Handles mid slide motion
    private void handleSlide()
    {
        currentSlideSpeed -= Time.deltaTime;

        if (!crouchInput || !runInput)
        {
            isSliding = false;
        }
    }

    // Handles end slide motion
    private void stopSlide()
    {
    }

    // Handles players speed depending on current state
    private float MovementSpeedHandler() 
    {
        Debug.Log(currentSlideSpeed);
        if (isSliding)
        {
            // motion whilst sliding
            handleSlide();
            return Math.Max(currentSlideSpeed, 0);
        }
        if (canSlide)
        {
            // start slide mechanic
            canSlide = false;
            startSlide();
            return startSlideSpeed;
        }
        else if (crouchInput)
        {
            return crouchSpeed;
        }
        else if (runInput)
        {
            return runSpeed;
        }
        else
        {
            return walkSpeed;
        }
    }

    // Detects collisions with collectible ship parts
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("ShipPart"))
        {
            shipPartManager.addPart();
            Destroy(collider.gameObject);
        }
    }

    // Setup components and values
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        crouchSpeed = 0.5f * walkSpeed;
        crouchHeight = 0.25f * characterController.height;
        runSpeed = 1.5f * walkSpeed;
        startSlideSpeed = 2f * walkSpeed;
        mouseSense = PlayerPrefs.GetFloat("MouseSensitivity", mouseSense);
    }

    // Handles movement and rotation each frame
    void Update()
    {
        // Adjust speed for player depending on current state
        float currentSpeed = MovementSpeedHandler();

        // Either move normally or in if sliding, go in that direction
        Vector3 normalMovement = transform.right * xMove + transform.forward * yMove;
        move = isSliding ? slideDirection : normalMovement;

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
