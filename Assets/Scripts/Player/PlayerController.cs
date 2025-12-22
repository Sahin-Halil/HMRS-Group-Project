using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// 1 issue
// Optional: Make it so user can keep moving whilst crouched/running after slide ends and they didnt change input key

// Need to fix height issue when crouching
// Add jump feature

// Needs fixing
// Jump doesn't work for all states
// Crouch goes underneath floor (needs fixing for snappy anyways)
// Optional add slide option after jump ends 
// Fix Comments

public class PlayerController : MonoBehaviour
{
    // Components
    private CharacterController characterController;
    public Camera characterCamera;
    private ShipPartManager shipPartManager;
    private float originalHeight;
    [SerializeField] private PauseManager pauseManager;
    private PlayerInput playerInput;
    private InputAction walkAction;
    private InputAction runAction;
    private InputAction crouchAction;
    private InputAction jumpAction;
    private InputAction dashAction;

    // Movement
    private bool walkInput = false;
    private float playerHorizontalSpeed = 0f;
    private float walkSpeed = 5f;
    private Vector3 move;
    private float xMove;
    private float yMove;
    private float xMoveOld;
    private float yMoveOld;

    // Mouse look
    private float mouseSense = 0.5f;
    private float xRotation;
    private float yRotation;

    // Crouch
    private bool crouchInput = false;
    [SerializeField] private float crouchSpeed = 2.5f;
    private float crouchHeight;
    private float crouchTransitionSpeed = 100f;

    // Running 
    private bool runInput = false;
    public float runSpeed = 7.5f;

    // Sliding
    private bool isSlide = false;
    private bool canSlide = true;
    private Vector3 slideDirection;
    public float startSlideSpeed;
    private float currentSlideSpeed = 12f;
    public float slideDecay = 17f;
    [SerializeField] private float slideCoolDown = 1f;
    private float slideCoolDownTimer = 0f;

    // Dashing
    private bool dashInput = false;
    private bool isDash = false;
    private bool canDash = true;
    private float dashDistance = 7f;
    private float dashDuration = 0.2f;
    private float dashCooldown = 1f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;
    private float dashTimeElapsed = 0f;

    // Gravity
    private float gravity = -9.81f;
    private float gravityMultiplier = 0.001f;

    // Jumping
    private float jumpValue = 0.001f;
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
        Jump,
        Dash
    }

    // Players initial state
    private MovementState state = MovementState.Idle;

    // Called when movement input is detected
    private void OnMove(InputValue value)
    {
        walkInput = true;
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
        crouchInput = true;
        canSlide = true;
    }

    private void HandleCrouchTransition()
    {
        float targetHeight = crouchInput ? crouchHeight : originalHeight;

        // Use crouchTransitionSpeed to control the smoothing time
        characterController.height = Mathf.Lerp(
            characterController.height,
            targetHeight,
            crouchTransitionSpeed * Time.deltaTime
        );
    }

    // Handles Run toggling
    private void OnRun()
    {
        runInput = true;
    }

    // Handles Jump toggling
    private void OnJump()
    {
        jumpInput = true;

        // possible for jump to start
        canJump = true;
    }

    private void OnDash()
    {
        if (canDash && dashCooldownTimer <= 0)
        {
            dashInput = true;
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
            slideCoolDownTimer = slideCoolDown;
        }
    }

    private void StartDash()
    {
        canDash = false;
        isDash = true;
        dashTimeElapsed = 0f;

        Vector3 inputDirection = transform.right * xMove + transform.forward * yMove;

        if (inputDirection.magnitude > 0.1f)
        {
            dashDirection = inputDirection.normalized;
        }
        else
        {
            dashDirection = transform.forward;
        }
    }

    private void UpdateDash()
    {
        dashTimeElapsed += Time.deltaTime;
        float dashProgress = dashTimeElapsed / dashDuration;

        if (dashProgress >= 1f)
        {
            StopDash();
            return;
        }

        float dashSpeed = dashDistance / dashDuration;
        Vector3 dashMovement = dashDirection * dashSpeed * Time.deltaTime;
        Vector3 verticalMove = transform.up * playerHeightSpeed * Time.deltaTime;

        CollisionFlags collisionFlags = characterController.Move(dashMovement + verticalMove);

        if ((collisionFlags & CollisionFlags.Sides) != 0)
        {
            StopDash();
        }
    }

    private void StopDash()
    {
        if (!isDash) return;
        isDash = false;
        dashCooldownTimer = dashCooldown;
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
                if (dashInput && canDash && dashCooldownTimer <= 0)
                {
                    state = MovementState.Dash;
                    StartDash();
                }
                else if (jumpInput && canJump)
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
                else
                {
                    playerHorizontalSpeed = 0;
                }
                break;

            // =======================
            // WALK STATE
            // =======================
            case MovementState.Walk:
                // Transition to dash if dash input is pressed
                if (dashInput && canDash && dashCooldownTimer <= 0)
                {
                    state = MovementState.Dash;
                    StartDash();
                }
                else if (jumpInput && canJump)
                {
                    state = MovementState.Jump;
                    StartJump();
                }
                // Transition to crouch if crouch input is pressed
                else if (crouchInput)
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
                if (dashInput && canDash && dashCooldownTimer <= 0)
                {
                    state = MovementState.Dash;
                    StartDash();
                }
                else if (jumpInput && canJump)
                {
                    state = MovementState.Jump;
                    StartJump();
                }
                // Stop moving if movement input is released
                else if (!hasMovementInput)
                {
                    state = MovementState.Idle;
                }
                else if (!runInput)
                {
                    state = MovementState.Walk;
                }
                else if (crouchInput && canSlide)
                {
                    state = MovementState.Slide;
                    StartSlide();
                }
                else if (crouchInput)
                {
                    state = MovementState.Crouch;
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
                // Exit crouch if jump input is pressed
                if (jumpInput && canJump)
                {
                    state = MovementState.Jump;
                    StartJump();
                }

                //Exit crouch if dash input is pressed
                else if (dashInput && canDash && dashCooldownTimer <= 0)
                {
                    state = MovementState.Dash;
                    StartDash();
                }

                else if (!crouchInput)
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
                if (dashInput && canDash && dashCooldownTimer <= 0)
                {
                    state = MovementState.Dash;
                    StartDash();
                }
                else if (characterController.isGrounded)
                {
                    if (crouchInput && canSlide)
                    {
                        state = MovementState.Slide;
                        StartSlide();
                    }
                    // Transition to crouch if crouch is still pressed
                    else if (crouchInput)
                    {
                        state = MovementState.Crouch;
                    }
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
                else if (runInput)
                {
                    playerHorizontalSpeed = runSpeed;
                }
                else if (walkInput)
                {
                    playerHorizontalSpeed = walkSpeed;
                }
                break;

            // =======================
            // SLIDE STATE
            // =======================
            case MovementState.Slide:
                if (jumpInput && canJump)
                {
                    state = MovementState.Jump;
                    StartJump();
                }
                else if (!isSlide)
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

            // =======================
            // DASH STATE
            // =======================
            case MovementState.Dash:
                if (!isDash)
                {
                    if (crouchInput)
                    {
                        state = MovementState.Crouch;
                    }
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
                    else
                    {
                        state = MovementState.Idle;
                    }
                }
                else
                {
                    dashTimeElapsed += Time.deltaTime;
                    float dashProgress = dashTimeElapsed / dashDuration;

                    if (dashProgress >= 1f)
                    {
                        StopDash();
                    }
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
        if (pauseManager.getPauseState())
        {
            return;
        }

        if (state == MovementState.Dash)
        {
            UpdateDash();
            return;
        }

        // Moves player in horizontal direction
        Vector3 horizontalDirection = transform.right * xMove + transform.forward * yMove;
        Vector3 horizontalMove;

        if (state == MovementState.Slide)
        {
            horizontalMove = slideDirection;
        }
        else if (state == MovementState.Dash && isDash)
        {
            float dashSpeed = dashDistance / dashDuration;
            horizontalMove = dashDirection * dashSpeed * Time.deltaTime;
            Vector3 dashVerticalMove = transform.up * playerHeightSpeed * Time.deltaTime;
            
            CollisionFlags collisionFlags = characterController.Move(horizontalMove + dashVerticalMove);
            
            if ((collisionFlags & CollisionFlags.Sides) != 0)
            {
                StopDash();
            }
            return;
        }
        else
        {
            horizontalMove = horizontalDirection;
        }

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

    private void PollHeldActions()
    {
        if (walkInput && !walkAction.IsPressed())
        {
            walkInput = false;
        }
        if (runInput && !runAction.IsPressed())
        {
            runInput = false;
        }
        if (crouchInput && !crouchAction.IsPressed()) 
        {
            crouchInput = false;
        }
        if (jumpInput && !jumpAction.IsPressed())
        {
            jumpInput = false;
        }
        if (dashInput && !dashAction.IsPressed())
        {
            dashInput = false;
        }
    }

    private void UpdateCoolDowns()
    {
        if (slideCoolDownTimer > 0f)
        {
            slideCoolDownTimer -= Time.deltaTime;
            if (slideCoolDownTimer <= 0f)
            {
                canSlide = true;
            }
        }
    }

    private void UpdateCoolDowns()
    {
        if (slideCoolDownTimer > 0f)
        {
            slideCoolDownTimer -= Time.deltaTime;
            if (slideCoolDownTimer <= 0f)
            {
                canSlide = true;
            }
        }
    }

    // Setup components and values
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        originalHeight = characterController.height;
        crouchSpeed = 0.5f * walkSpeed;
        crouchHeight = 0.7f * originalHeight;
        runSpeed = 1.5f * walkSpeed;
        startSlideSpeed = 13;
        mouseSense = PlayerPrefs.GetFloat("MouseSensitivity", mouseSense);
        
        playerInput = GetComponent<PlayerInput>();
        walkAction = playerInput.actions["Move"];
        runAction = playerInput.actions["Run"];
        crouchAction = playerInput.actions["Crouch"];
        jumpAction = playerInput.actions["Jump"];
        dashAction = playerInput.actions["Dash"];
    }

    // Handles movement and rotation each frame
    void Update()
    {
        PollHeldActions();
        
        UpdateCoolDowns();

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0)
            {
                canDash = true;
            }
        }

        PlayerState();

        HandleCrouchTransition();

        ApplyGravity();

        MovePlayer();

        Debug.Log(state);
        Debug.Log(runInput);

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