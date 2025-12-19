using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

// 1 issue
// Optional: Make it so user can keep moving whilst crouched/running after slide ends and they didnt change input key

// Need to fix height issue when crouching
// Add jump feature

// Needs fixing
// Jump doesn't work
// Crouch goes underneath floor (needs fixing for snappy anyways)
// Fix Comments

public class PlayerController : MonoBehaviour
{
    // Components
    private CharacterController characterController;
    public Camera characterCamera;
    [SerializeField] private ShipPartManager shipPartManager;

    // Movement
    private bool walkInput = false;
    private float playerHorizontalSpeed = 0f;
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
    private bool canSlide = true;
    private Vector3 slideDirection; 
    private float startSlideSpeed;
    private float currentSlideSpeed;
    private float slideDecay = 5f;

    // Gravity
    private float gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 0.005f;

    // Jumping
    [SerializeField] private float jumpValue = 0.005f;
    private float playerHeightSpeed = 0f;
    private bool jumpInput = false;
    private bool canJump = false;

    // Possible player states
    private enum MovementState
    {
        Idle,
        Walk,
        Run,
        Crouch,
        Slide,
        Jump
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
        characterController.height = crouchInput ? -crouchHeight : crouchHeight;

        // possible to begin slide
        if (crouchInput)
        {
            canSlide = true;
        }
    }

    // Handles Run toggling
    private void OnRun()
    {
        runInput = !runInput;
    }

    // Handles Jump toggling
    private void OnJump()
    {
        jumpInput = !jumpInput;

        // possible for jump to start
        if (jumpInput)
        {
            canJump = true;
        }
    }

    private void StartJump()
    {
        canJump = false;

        // Apply vertical velocity formula
        playerHeightSpeed = Mathf.Sqrt(2f * jumpValue * -gravity * gravityMultiplier);
    }

    // Handles start slide motion
    private void StartSlide()
    {
        canSlide = false;
        isSlide = true;
        currentSlideSpeed = startSlideSpeed;

        slideDirection = transform.right * xMove + transform.forward * yMove;

        xMoveOld = xMove;
        yMoveOld = yMove;
    }

    // Updates mid slide motion
    private void UpdateSlide()
    {
        currentSlideSpeed -= slideDecay * Time.deltaTime;

        bool changedDirection = xMove != xMoveOld || yMove != yMoveOld;
        bool releasedInput = !runInput || !crouchInput;

        if (changedDirection || releasedInput || currentSlideSpeed <= 0f)
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
                if (jumpInput && canJump)
                {
                    state = MovementState.Jump;
                    StartJump();
                }
                else if (crouchInput)
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
                    playerHorizontalSpeed = 0;
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
                    playerHorizontalSpeed = walkSpeed;
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
                else if (crouchInput && canSlide)
                {
                    state = MovementState.Slide;
                    StartSlide();
                }
                // Continue running
                else
                {
                    playerHorizontalSpeed = runSpeed;
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
                    playerHorizontalSpeed = crouchSpeed;
                }
                break;

            case MovementState.Jump:
                if (characterController.isGrounded)
                {
                    state = MovementState.Idle;
                }
                break;

            // =======================
            // SLIDE STATE
            // =======================
            case MovementState.Slide:
                // If slide has ended, return to idle (state resolution happens next frame)
                if (!isSlide)
                {
                    // Transition to crouch if crouch is still pressed
                    if (crouchInput) 
                    { 
                        state = MovementState.Crouch;
                    }
                    // If still moving, decide whether to run or walk
                    else if (hasMovementInput)
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
                // While sliding, update slide physics and apply slide speed
                else
                {
                    UpdateSlide();
                    playerHorizontalSpeed = currentSlideSpeed;
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

    private void ApplyGravity()
    {
        if (characterController.isGrounded && playerHeightSpeed <= 0f)
        {
            playerHeightSpeed = 0f;
        }
        else
        {
            playerHeightSpeed += gravity * gravityMultiplier * Time.deltaTime;
        }
    }

    private void MovePlayer()
    {
        // Moves player in horizontal direction
        Vector3 horizontalDirection = transform.right * xMove + transform.forward * yMove;
        Vector3 horizontalMove = state == MovementState.Slide ? slideDirection : horizontalDirection;
        
        // Normalise for multi horizontal directional movement
        if (horizontalMove.magnitude > 1)
        {
            horizontalMove.Normalize();
        }

        // Apply speed and delta Time
        horizontalMove *= playerHorizontalSpeed * Time.deltaTime;

        // Moves player in vertical direction
        Vector3 verticalMove = transform.up * playerHeightSpeed;

        // Combine both horizontal and vertical movement
        move = horizontalMove + verticalMove;

        // Move player
        characterController.Move(move);
    }
    

    private void MovePlayerCamera()
    {
        transform.rotation = Quaternion.Euler(0f, xRotation, 0f);
        characterCamera.transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);
    }

    // Setup components and values
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        crouchSpeed = 0.5f * walkSpeed;
        crouchHeight = 0.25f * characterController.height;
        runSpeed = 1.5f * walkSpeed;
        startSlideSpeed = 1.5f * walkSpeed;
        mouseSense = PlayerPrefs.GetFloat("MouseSensitivity", mouseSense);
    }

    // Handles movement and rotation each frame
    void Update()
    {
        // Handles players next state
        PlayerState(); 

        ApplyGravity();
        
        MovePlayer();

        Debug.Log(state);
        if (!isSlide) { 
          //  Debug.Log(speed);
        }

        // update the camera
        MovePlayerCamera();
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
