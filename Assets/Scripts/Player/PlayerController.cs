using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class PlayerController : MonoBehaviour
{
    // Components
    private CharacterController characterController;
    public Camera characterCamera;
    private Animator animator;
    private ShipPartManager shipPartManager;
    private AudioSource audioSource;
    private float originalHeight;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private DieScript playerDeath;

    // Player Inputs
    private PlayerInput playerInput;
    private InputAction walkAction;
    private InputAction runAction;
    private InputAction crouchAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction attackAction;

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
    private float gamepadSensitivityMultiplier = 200f; // Multiplier to convert mouse sens to gamepad sens
    private float xRotation;
    private float yRotation;
    private Vector2 lookInput; // Store current look input

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
    private float slideCoolDown = 0.5f;
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
    private float gravity = -11.00f;
    private float gravityMultiplier = 1.8f;
    private bool isTouchingWall = false;

    // Slope Sliding
    private bool isOnSteepSlope = false;
    private Vector3 slopeSlideDirection;
    [SerializeField] private float slopeSlideSpeed = 8f;
    [SerializeField] private float maxSlopeAngle = 45f; // Maximum walkable angle

    // Jumping
    private float jumpValue = 1.8f;
    private float playerHeightSpeed = -2.0f;
    private bool jumpInput = false;
    private bool canJump = false;

    // Combat
    // TODO Make all values private after being satisfied with them
    private bool attackInput = false;
    private bool isAttack = false;
    private bool canAttack = true;
    private float attackDuration = 0.4f;
    private float attackTimeElapsed = 0f;
    private float attackCooldown = 0.5f;
    private float attackCooldownTimer = 0f;
    public float attackDistance = 3f;
    public float attackDelay = 0.4f;
    public float attackSpeed = 1f;
    public int attackDamage = 10;
    public LayerMask attackLayer;
    public GameObject hitEffect;
    public AudioClip swordSwing;
    public AudioClip hitSound;
    int attackCount;
    private bool hasHitThisAttack = false; // Track if we've already hit during this attack
    
    // Combo system
    private int comboCount = 0; // Determines attack animation
    private int previousComboCount = -1; // Track previous combo to detect changes
    private bool comboQueued = false; // Track if next attack is queued
    private float comboResetTime = 1.0f; // Time before combo resets
    private float timeSinceLastAttack = 0f;
    private float comboWindowStart = 0.4f; // When combo window opens (40% through animation)
    private float comboTransitionPoint = 0.75f; // When to start next attack (75% through animation)

    // Possible player states
    private enum MovementState
    {
        Idle,
        Walk,
        Run,
        Crouch,
        Slide,
        Jump,
        Dash,
        Attack
    }

    // Players initial state
    private MovementState state = MovementState.Idle;
    private MovementState previousState = MovementState.Idle;

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
        lookInput = value.Get<Vector2>();
    }

    // Handles crouch toggling
    private void OnCrouch()
    {
        crouchInput = true;
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

    bool CanStandUp()
    {
        float radius = characterController.radius * 0.7f;
        float standHeight = originalHeight;
        Vector3 origin = transform.position + Vector3.up * (radius + 0.05f);

        float checkDistance = standHeight - characterController.height;

        return !Physics.SphereCast(
            origin,
            radius * 0.9f,
            Vector3.up,
            out _,
            checkDistance,
            ~0,
            QueryTriggerInteraction.Ignore
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

        // Allow jump if on ground - slope sliding will handle steep surfaces naturally
        // Also allow small jumps while sliding down steep slopes for natural feel
        if (characterController.isGrounded || isOnSteepSlope)
        {
            canJump = true;
        }
    }

    // Handles Dash toggling
    private void OnDash()
    {
        if (canDash && dashCooldownTimer <= 0)
        {
            dashInput = true;
        }
    }

    // Handles Attack toggling
    private void OnFire()
    {
        // Allow attack if not on cooldown
        if (canAttack && attackCooldownTimer <= 0)
        {
            attackInput = true;
        }
        // Allow combo queueing only during combo window (after certain % of attack)
        else if (isAttack && !comboQueued)
        {
            float attackProgress = attackTimeElapsed / attackDuration;
            if (attackProgress >= comboWindowStart)
            {
                comboQueued = true;
            }
        }
    }

    // Starts start Jump motion
    private void StartJump()
    {
        canJump = false;

        // Apply vertical velocity formula
        // Reduce jump strength significantly when on steep slopes for tiny natural jumps
        float effectiveJumpValue = isOnSteepSlope ? jumpValue * 0.3f : jumpValue;
        playerHeightSpeed = Mathf.Sqrt(2f * effectiveJumpValue * -gravity * gravityMultiplier);
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
        bool releasedInput = !runInput || !crouchInput || jumpInput;

        if (changedDirection || releasedInput || currentSlideSpeed <= 0f)
        {
            isSlide = false;
            slideCoolDownTimer = slideCoolDown;
        }
    }

    // Handles start dash motion
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

    // Handles mid dash motion
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

    // Handles end of Dash
    private void StopDash()
    {
        if (!isDash) return;
        isDash = false;
        dashCooldownTimer = dashCooldown;
    }

    // Handles start attack motion
    private void StartAttack()
    {
        canAttack = false;
        isAttack = true;
        attackTimeElapsed = 0f;
        hasHitThisAttack = false; // Reset hit flag for new attack
        timeSinceLastAttack = 0f; // Reset combo timer
        
        // Progress combo if queued, otherwise reset to first attack
        if (comboQueued)
        {
            comboCount = (comboCount + 1) % 3; // Cycle through 0, 1, 2
            comboQueued = false;
        }
        else
        {
            // Reset combo if starting fresh attack (not queued from previous)
            comboCount = 0;
        }
        
        // Play swing sound with randomized pitch
        PlaySwingSound();
    }

    // Handles mid attack motion
    private void UpdateAttack()
    {
        attackTimeElapsed += Time.deltaTime;
        float attackProgress = attackTimeElapsed / attackDuration;

        // If combo is queued, transition earlier for smooth chaining
        if (comboQueued && attackProgress >= comboTransitionPoint)
        {
            StopAttack();
            return;
        }

        if (attackProgress >= 1f)
        {
            StopAttack();
            return;
        }

        AttackRaycast();
    }

    void AttackRaycast()
    {
        // Only check for hits if we haven't already hit something this attack
        if (!hasHitThisAttack && Physics.Raycast(characterCamera.transform.position, characterCamera.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        {
            hasHitThisAttack = true; // Mark that we've hit something
            HitTarget(hit);
        }
    }

    void HitTarget(RaycastHit hit)
    {
        // Play hit sound with randomized pitch
        if (audioSource != null && hitSound != null)
        {
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.PlayOneShot(hitSound);
        }

        // Spawn hit effect at hit point
        GameObject GO = Instantiate(hitEffect, hit.point, Quaternion.identity);
        
        // Deal damage to the enemy if it has an EnemyHealth component
        EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            // Parent the hit effect to the enemy so it follows them
            GO.transform.SetParent(hit.collider.transform);
            
            // Pass the hit effect GameObject to the enemy so it can destroy it on death
            enemyHealth.TakeDamage(attackDamage, GO);
            // Destroy the hit effect after 0.7 seconds
            Destroy(GO, 0.7f);
        }
        

    }

    // Plays swing sound with randomized pitch
    private void PlaySwingSound()
    {   
        if (audioSource != null && swordSwing != null)
        {
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.PlayOneShot(swordSwing);
        }
    }

    void handleAnimations(MovementState newState)
    {
        bool stateChanged = newState != previousState;
        bool comboChanged = comboCount != previousComboCount;
        
        // Only trigger animation if state changed or combo changed during attack
        if (!stateChanged && !(newState == MovementState.Attack && comboChanged))
        {
            return;
        }

        if (newState == MovementState.Idle) {
            animator.CrossFadeInFixedTime("Idle-Animation", 0.2f);
        }
        else if (newState == MovementState.Attack)
        {
            // Play the correct attack animation based on combo count
            string attackAnimName = "Attack-Animation-" + (comboCount + 1).ToString();
            // Use shorter crossfade for combo attacks for snappier feel
            float fadeTime = comboCount > 0 ? 0.05f : 0.1f;
            animator.CrossFadeInFixedTime(attackAnimName, fadeTime);
            previousComboCount = comboCount; // Update after playing animation
        }
        // Add other states as needed (Walk, Run, etc.)
        // For now, non-attack movement states will use Idle animation
        else if (newState == MovementState.Walk || newState == MovementState.Run || 
                 newState == MovementState.Crouch || newState == MovementState.Slide ||
                 newState == MovementState.Jump || newState == MovementState.Dash)
        {
            animator.CrossFadeInFixedTime("Idle-Animation", 0.2f);
        }

        previousState = newState;
    }

    // Handles end of Attack
    private void StopAttack()
    {
        if (!isAttack) return;
        isAttack = false;
        
        // If combo is queued, start next attack immediately
        if (comboQueued)
        {
            attackCooldownTimer = 0f; // No cooldown for smooth combo
            canAttack = true; // Allow immediate attack
            attackInput = true; // Trigger next attack
            // comboQueued will be cleared by StartAttack()
        }
        else
        {
            attackCooldownTimer = attackCooldown;
        }

        // Reset hit tracking for next attack
        hasHitThisAttack = false;
    }

    // Handles players speed depending on current state
    private void PlayerState()
    {
        // True if the player is providing any movement input
        bool hasMovementInput = xMove != 0 || yMove != 0;

        // Check if player can stand
        bool canStand = CanStandUp();

        // State machine controlling player movement behaviour
        switch (state)
        {
            // =======================
            // IDLE STATE
            // =======================
            case MovementState.Idle:
                if (attackInput && canAttack && attackCooldownTimer <= 0)
                {
                    state = MovementState.Attack;
                    StartAttack();
                }
                else if (dashInput && canDash && dashCooldownTimer <= 0)
                {
                    state = MovementState.Dash;
                    StartDash();
                }
                else if (jumpInput && canJump)
                {
                    state = MovementState.Jump;
                    StartJump();
                }
                else if (characterController.isGrounded && crouchInput)
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
                // Transition to attack if attack input is pressed
                if (attackInput && canAttack && attackCooldownTimer <= 0)
                {
                    state = MovementState.Attack;
                    StartAttack();
                }
                // Transition to dash if dash input is pressed
                else if (dashInput && canDash && dashCooldownTimer <= 0)
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
                else if (characterController.isGrounded && crouchInput)
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
                // Transition to attack if attack input is pressed
                if (attackInput && canAttack && attackCooldownTimer <= 0)
                {
                    state = MovementState.Attack;
                    StartAttack();
                }
                else if (dashInput && canDash && dashCooldownTimer <= 0)
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
                else if (characterController.isGrounded && crouchInput && canSlide)
                {
                    state = MovementState.Slide;
                    StartSlide();
                }
                else if (characterController.isGrounded && crouchInput)
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
                if (canStand) 
                { 
                    // Exit crouch if dash input is pressed
                    if (attackInput && canAttack && attackCooldownTimer <= 0)
                    {
                        state = MovementState.Attack;
                        StartAttack();
                    }

                    //Exit crouch if dash input is pressed
                    else if (dashInput && canDash && dashCooldownTimer <= 0)
                    {
                        state = MovementState.Dash;
                        StartDash();
                    }
                    // Exit crouch if jump input is pressed
                    else if (jumpInput && canJump)
                    {
                        state = MovementState.Jump;
                        StartJump();
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
                }
                // Continue crouch movement
                else
                {
                    playerHorizontalSpeed = crouchSpeed;
                }
                break;
            // =======================
            // JUMP STATE
            // =======================
            case MovementState.Jump:
                // Transition to attack if attack input is pressed
                if (attackInput && canAttack && attackCooldownTimer <= 0)
                {
                    state = MovementState.Attack;
                    StartAttack();
                }
                else if (dashInput && canDash && dashCooldownTimer <= 0)
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
                if (!isSlide)
                {
                    if (!canStand)
                    {
                        state = MovementState.Crouch;
                    }
                    else if (jumpInput && canJump)
                    {
                        state = MovementState.Jump;
                        StartJump();
                    }
                    // Transition to crouch if crouch is still pressed
                    else if (crouchInput)
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
                    if (characterController.isGrounded && crouchInput)
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

            // =======================
            // ATTACK STATE
            // =======================
            case MovementState.Attack:
                if (!isAttack)
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
                    UpdateAttack();
                    // Maintain current movement speed during attack
                    // Check what speed the player had before attacking
                    if (hasMovementInput)
                    {
                        if (runInput)
                        {
                            playerHorizontalSpeed = runSpeed;
                        }
                        else if (walkInput)
                        {
                            playerHorizontalSpeed = walkSpeed;
                        }
                    }
                    else
                    {
                        playerHorizontalSpeed = 0;
                    }
                }
                break;
        }
        
        // Only update animations if state has changed
        handleAnimations(state);
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

    // Checks if player is on a steep slope and should slide down
    // Uses multiple raycasts to detect steep terrain even when standing on protruding vertices
    private void CheckSlopeSliding()
    {
        isOnSteepSlope = false;

        if (!characterController.isGrounded)
        {
            return;
        }

        Vector3 rayOrigin = transform.position + characterController.center;
        float rayDistance = (characterController.height / 2f) + 0.3f;
        float checkRadius = characterController.radius * 0.8f;

        // Cast multiple rays in a circle around the player to detect steep slopes
        // This catches cases where player stands on a flat vertex but surrounding terrain is steep
        Vector3 steepestNormal = Vector3.up;
        float steepestAngle = 0f;
        bool foundSteepSlope = false;

        // Check center ray
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit centerHit, rayDistance))
        {
            float angle = Vector3.Angle(centerHit.normal, Vector3.up);
            if (angle > maxSlopeAngle && angle < 90f)
            {
                foundSteepSlope = true;
                steepestAngle = angle;
                steepestNormal = centerHit.normal;
            }
        }

        // Check 8 rays around the player in a circle
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * checkRadius;
            Vector3 rayPos = rayOrigin + offset;

            if (Physics.Raycast(rayPos, Vector3.down, out RaycastHit hit, rayDistance))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (slopeAngle > maxSlopeAngle && slopeAngle < 90f)
                {
                    foundSteepSlope = true;
                    if (slopeAngle > steepestAngle)
                    {
                        steepestAngle = slopeAngle;
                        steepestNormal = hit.normal;
                    }
                }
            }
        }

        if (foundSteepSlope)
        {
            isOnSteepSlope = true;
            // Calculate slide direction down the steepest slope found
            slopeSlideDirection = Vector3.ProjectOnPlane(Vector3.down, steepestNormal).normalized;
        }
    }

    // Applies constant gravity to player
    private void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            isTouchingWall = false;
        }
        // Don't reset gravity if on steep slope - let the slide physics work
        if (characterController.isGrounded && playerHeightSpeed <= 0f && !isOnSteepSlope)
        {
            playerHeightSpeed = -2.0f;
        }
        else
        { 
            // Use stronger gravity when touching steep walls or on steep slopes for natural slip-off
            float currentGravityMultiplier = (isTouchingWall || isOnSteepSlope) ? gravityMultiplier * 3f : gravityMultiplier;
            playerHeightSpeed += gravity * currentGravityMultiplier * Time.deltaTime;
        }
    }

    // Called by Unity when CharacterController hits a collider during Move
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        float surfaceAngle = Vector3.Angle(hit.normal, Vector3.up);
        
        // Mark if touching a steep wall to apply stronger gravity
        if (surfaceAngle > characterController.slopeLimit && !characterController.isGrounded)
        {
            isTouchingWall = true;
        }
    }

    // Handles player movement
    private void MovePlayer()
    { 
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

        // If on a steep slope, apply sliding force that overrides player input
        if (isOnSteepSlope)
        {
            // Override player horizontal movement with strong sliding force
            // This ensures they slip off even when trying to move forward
            Vector3 slideForce = slopeSlideDirection * slopeSlideSpeed * Time.deltaTime;
            
            // Reduce player's control on steep slopes (keep only 20% of their input)
            horizontalMove *= 0.2f;
            
            // Apply the slide force - this dominates over player input
            horizontalMove += slideForce;
            
            // Apply strong downward force to ensure player slides off vertices
            // This overrides the normal grounded behavior
            playerHeightSpeed = Mathf.Min(playerHeightSpeed, -5f);
        }

        // Moves player in vertical direction
        Vector3 verticalMove = transform.up * playerHeightSpeed * Time.deltaTime;

        // Combine both horizontal and vertical movement
        move = horizontalMove + verticalMove;

        // Move player
        characterController.Move(move);
    }

    // Handles camera movement
    private void MovePlayerCamera()
    {
        // Detect if using gamepad by checking current control scheme
        bool isGamepad = playerInput.currentControlScheme == "Gamepad";
        
        if (isGamepad)
        {
            // Gamepad: Scale sensitivity by multiplier and apply with Time.deltaTime for smooth movement
            float gamepadSens = mouseSense * gamepadSensitivityMultiplier;
            xRotation = (xRotation + (lookInput.x * gamepadSens * Time.deltaTime)) % 360f;
            yRotation = Mathf.Clamp(yRotation - (lookInput.y * gamepadSens * Time.deltaTime), -90f, 90f);
        }
        else
        {
            // Mouse: Apply directly (delta is already frame-independent)
            xRotation = (xRotation + (lookInput.x * mouseSense)) % 360f;
            yRotation = Mathf.Clamp(yRotation - (lookInput.y * mouseSense), -90f, 90f);
            // Clear mouse delta after applying (mouse sends delta, gamepad sends continuous values)
            lookInput = Vector2.zero;
        }
        
        // Normalize rotation values to prevent NaN/Infinity issues
        if (float.IsNaN(xRotation) || float.IsInfinity(xRotation)) xRotation = 0f;
        if (float.IsNaN(yRotation) || float.IsInfinity(yRotation)) yRotation = 0f;
        
        transform.rotation = Quaternion.Euler(0f, xRotation, 0f);
        characterCamera.transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);
    }

    // Polls inputs to make sure they are false when not pressed
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
            // Only allow uncrouch if there is space above
            if (CanStandUp())
            {
                crouchInput = false;
            }
        }
        if (jumpInput && !jumpAction.IsPressed())
        {
            jumpInput = false;
        }
        if (dashInput && !dashAction.IsPressed())
        {
            dashInput = false;
        }
        if (attackInput && !attackAction.IsPressed())
        {
            attackInput = false;
        }
    }

    // Updates cooldowns for dash, slide, and attack
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

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0)
            {
                canDash = true;
            }
        }

        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer <= 0)
            {
                canAttack = true;
            }
        }
        
        // Reset combo if too much time has passed since last attack
        // But don't reset if we have a combo queued
        if (!isAttack && !comboQueued)
        {
            timeSinceLastAttack += Time.deltaTime;
            if (timeSinceLastAttack >= comboResetTime)
            {
                comboCount = 0;
            }
        }
    }

    // Setup components and values
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = characterCamera.GetComponentInChildren<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        originalHeight = characterController.height;
        crouchHeight = 0.7f * originalHeight;
        mouseSense = PlayerPrefs.GetFloat("MouseSensitivity", mouseSense);
        
        // Use CharacterController's slopeLimit if not set in inspector
        if (maxSlopeAngle == 0f)
        {
            maxSlopeAngle = characterController.slopeLimit;
        }
        
        // Initialize rotation values from current transform rotation
        xRotation = transform.eulerAngles.y;
        yRotation = characterCamera.transform.localEulerAngles.x;
        // Handle angle wrapping for camera pitch
        if (yRotation > 180f) yRotation -= 360f;
        
        playerInput = GetComponent<PlayerInput>();
        audioSource = GetComponent<AudioSource>();
        walkAction = playerInput.actions["Move"];
        runAction = playerInput.actions["Run"];
        crouchAction = playerInput.actions["Crouch"];
        jumpAction = playerInput.actions["Jump"];
        dashAction = playerInput.actions["Dash"];
        attackAction = playerInput.actions["Fire"];
    }

    // Handles movement and rotation each frame
    void Update()
    {
        if (uiManager.getPauseState() || playerDeath.checkDead())
        {
            return;
        }
        PollHeldActions();
        
        UpdateCoolDowns();

        PlayerState();

        HandleCrouchTransition();

        CheckSlopeSliding();

        ApplyGravity();

        MovePlayer();

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