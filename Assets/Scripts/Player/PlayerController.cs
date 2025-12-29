using UnityEngine;
using UnityEngine.InputSystem;
using Cursor = UnityEngine.Cursor;

public class PlayerController : MonoBehaviour
{
    // Components
    private CharacterController characterController;
    public Camera characterCamera;
    private ShipPartManager shipPartManager;
    private float originalHeight;
    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private DieScript playerDeath; // Ensure this is assigned in Inspector
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
    private float gravityMultiplier = 0.003f;

    // Jumping
    private float jumpValue = 0.003f;
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

    // ===================== INPUT CALLBACKS =====================

    private void OnMove(InputValue value)
    {
        Debug.Log("OnMove CALLED");
        walkInput = true;
        Vector2 moveInput = value.Get<Vector2>();
        xMove = moveInput.x;
        yMove = moveInput.y;
    }

    private void OnLook(InputValue value)
    {
        Vector2 mouseInput = value.Get<Vector2>();
        xRotation += mouseInput.x * mouseSense;
        yRotation = Mathf.Clamp(yRotation - mouseInput.y * mouseSense, -90f, 90f);
    }

    private void OnRun() => runInput = true;
    private void OnCrouch() => crouchInput = true;

    private void OnJump()
    {
        jumpInput = true;
        if (characterController.isGrounded)
            StartJump();
    }

    private void OnDash()
    {
        if (canDash && dashCooldownTimer <= 0)
            dashInput = true;
    }

    // ===================== CORE LOGIC =====================

    private void StartJump()
    {
        canJump = false;
        playerHeightSpeed = Mathf.Sqrt(2f * jumpValue * -gravity * gravityMultiplier);
        state = MovementState.Jump;
    }

    private void StartDash()
    {
        canDash = false;
        isDash = true;
        dashTimeElapsed = 0f;

        Vector3 inputDir = transform.right * xMove + transform.forward * yMove;
        dashDirection = inputDir.magnitude > 0.1f ? inputDir.normalized : transform.forward;
    }

    private void UpdateDash()
    {
        dashTimeElapsed += Time.deltaTime;
        if (dashTimeElapsed >= dashDuration)
        {
            isDash = false;
            dashCooldownTimer = dashCooldown;
        }

        float dashSpeed = dashDistance / dashDuration;
        characterController.Move(dashDirection * dashSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && playerHeightSpeed < 0)
            playerHeightSpeed = -0.01f;
        else
            playerHeightSpeed += gravity * gravityMultiplier * Time.deltaTime;
    }

    private void MovePlayer()
    {
        if (state == MovementState.Dash)
        {
            UpdateDash();
            return;
        }

        Vector3 horizontal = (transform.right * xMove + transform.forward * yMove);
        if (horizontal.magnitude > 1) horizontal.Normalize();

        horizontal *= playerHorizontalSpeed * Time.deltaTime;
        Vector3 vertical = Vector3.up * playerHeightSpeed;

        characterController.Move(horizontal + vertical);
    }

    private void MoveCamera()
    {
        transform.rotation = Quaternion.Euler(0f, xRotation, 0f);
        characterCamera.transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);
    }

    private void UpdateState()
    {
        bool hasMove = xMove != 0 || yMove != 0;

        switch (state)
        {
            case MovementState.Idle:
                playerHorizontalSpeed = 0;
                if (dashInput) { state = MovementState.Dash; StartDash(); }
                else if (jumpInput) StartJump();
                else if (hasMove) state = runInput ? MovementState.Run : MovementState.Walk;
                break;

            case MovementState.Walk:
                playerHorizontalSpeed = walkSpeed;
                if (!hasMove) state = MovementState.Idle;
                if (runInput) state = MovementState.Run;
                break;

            case MovementState.Run:
                playerHorizontalSpeed = runSpeed;
                if (!hasMove) state = MovementState.Idle;
                if (!runInput) state = MovementState.Walk;
                break;

            case MovementState.Jump:
                if (characterController.isGrounded)
                    state = hasMove ? MovementState.Walk : MovementState.Idle;
                break;

            case MovementState.Dash:
                if (!isDash)
                    state = MovementState.Idle;
                break;
        }
    }

    private void ResetInputs()
    {
        if (!walkAction.IsPressed()) walkInput = false;
        if (!runAction.IsPressed()) runInput = false;
        if (!crouchAction.IsPressed()) crouchInput = false;
        if (!jumpAction.IsPressed()) jumpInput = false;
        if (!dashAction.IsPressed()) dashInput = false;
    }

    // ===================== UNITY =====================

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        walkAction = playerInput.actions["Move"];
        runAction = playerInput.actions["Run"];
        crouchAction = playerInput.actions["Crouch"];
        jumpAction = playerInput.actions["Jump"];
        dashAction = playerInput.actions["Dash"];

        mouseSense = PlayerPrefs.GetFloat("MouseSensitivity", mouseSense);
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (pauseManager != null && pauseManager.getPauseState())
            return;

        ResetInputs();
        UpdateState();
        ApplyGravity();
        MovePlayer();
        MoveCamera();

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0) canDash = true;
        }
    }

    // ===================== SETTINGS =====================

    public float GetSensitivity() => mouseSense;
    public void SetSensitivity(float sensitivity) => mouseSense = sensitivity;
}
