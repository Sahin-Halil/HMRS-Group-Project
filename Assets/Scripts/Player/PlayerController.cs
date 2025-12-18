using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

// 1 issue
// Optional: Make it so user can keep moving whilst crouched/running after slide ends and they didnt change input key

public class PlayerController : MonoBehaviour
{
    // Components
    private CharacterController characterController;
    public Camera characterCamera;
    [SerializeField] private ShipPartManager shipPartManager;

    // Movement
    private bool walkInput = false;
    private float speed;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private Vector3 move;
    private Vector3 normalMovement;
    [SerializeField] private float xMove;
    [SerializeField] private float yMove;
    private float xMoveOld;
    private float yMoveOld;

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
    private bool isSlide = false;
    private Vector3 slideDirection; 
    private float startSlideSpeed;
    private float currentSlideSpeed;
    private float slideDecay = 5f;

    // Possible player states
    private enum MovementState
    {
        Idle,
        Walk,
        Run,
        Crouch,
        Slide,
        Air
    }

    // Players initial state
    private MovementState state = MovementState.Idle;

    // Called when movement input is detected
    private void OnMove(InputValue value)
    {
        walkInput = !walkInput;
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

        characterController.height += crouchInput ? -crouchHeight : crouchHeight;
    }


    // Handles Run toggling
    private void OnRun()
    {
        runInput = !runInput;
    }


    // Handles start slide motion
    private void StartSlide()
    {
        isSlide = true;
        currentSlideSpeed = startSlideSpeed;

        slideDirection = transform.right * xMove + transform.forward * yMove;
    }


    // Updates mid slide motion
    private void UpdateSlide()
    {
        currentSlideSpeed -= slideDecay * Time.deltaTime;
        
        if (currentSlideSpeed <= 0f)
        {
            isSlide = false;
        }
    }

    // Handles players speed depending on current state
    private void PlayerState()
    {
        // True if the player is providing any movement input
        bool hasMovementInput = xMove != 0 || yMove != 0;

        // State machine controlling player movement behaviour
        switch (state)
        {
            // =======================
            // IDLE STATE
            // =======================
            case MovementState.Idle:
                // Enter crouch if crouch input is active
                if (crouchInput)
                {
                    state = MovementState.Crouch;
                }
                // Start moving if there is movement input
                else if (hasMovementInput)
                {
                    // Prefer running if run input is active
                    if (runInput)
                    {
                        state = MovementState.Run;
                    }
                    // Otherwise walk
                    else if (walkInput)
                    {
                        state = MovementState.Walk;
                    }
                }
                // No input: remain idle and stop movement
                else
                {
                    speed = 0;
                }
                break;

            // =======================
            // WALK STATE
            // =======================
            case MovementState.Walk:
                // Transition to crouch if crouch input is pressed
                if (crouchInput)
                {
                    state = MovementState.Crouch;
                }
                // Stop moving if movement input is released
                else if (!hasMovementInput)
                {
                    state = MovementState.Idle;
                }
                // Transition to run if run input is pressed
                else if (runInput)
                {
                    state = MovementState.Run;
                }
                // Continue walking
                else
                {
                    speed = walkSpeed;
                }
                break;

            // =======================
            // RUN STATE
            // =======================
            case MovementState.Run:
                // Stop moving if movement input is released
                if (!hasMovementInput)
                {
                    state = MovementState.Idle;
                }
                // Drop to walk if run input is released
                else if (!runInput)
                {
                    state = MovementState.Walk;
                }
                // Start slide if crouch is pressed while running
                else if (crouchInput)
                {
                    state = MovementState.Slide;
                    StartSlide();
                }
                // Continue running
                else
                {
                    speed = runSpeed;
                }
                break;

            // =======================
            // CROUCH STATE
            // =======================
            case MovementState.Crouch:
                // Exit crouch if crouch input is released
                if (!crouchInput)
                {
                    // If moving, decide whether to run or walk
                    if (hasMovementInput)
                    {
                        if (runInput)
                        {
                            state = MovementState.Run;
                        }
                        else if (walkInput)
                        {
                            state = MovementState.Walk;
                        }
                    }
                    // Otherwise return to idle
                    else
                    {
                        state = MovementState.Idle;
                    }
                }
                // Continue crouch movement
                else
                {
                    speed = crouchSpeed;
                }
                break;

            // =======================
            // SLIDE STATE
            // =======================
            case MovementState.Slide:
                // If slide has ended, return to idle (state resolution happens next frame)
                if (!isSlide)
                {
                    state = MovementState.Idle;
                }
                // While sliding, update slide physics and apply slide speed
                else
                {
                    UpdateSlide();
                    speed = currentSlideSpeed;
                }
                break;
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
        startSlideSpeed = 2.5f * walkSpeed;
        mouseSense = PlayerPrefs.GetFloat("MouseSensitivity", mouseSense);
    }

    // Handles movement and rotation each frame
    void Update()
    {
        // Handles players next state
        PlayerState(); 

        // Moves player in specified direction
        Vector3 inputDirection = transform.right * xMove + transform.forward * yMove;
        move = state == MovementState.Slide ? slideDirection : inputDirection;

        // Normalise for multi directional movement
        if (move.magnitude > 1)
        {
            move.Normalize();
        }

        Debug.Log(state);
        if (!isSlide) { 
            Debug.Log(speed);
        }

        // Move player
        characterController.SimpleMove(move * speed);

        // update the camera
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
