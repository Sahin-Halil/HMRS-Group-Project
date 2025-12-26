using System;
using UnityEngine;
using System.Linq;

public class NPCController : MonoBehaviour
{
    // Behaviour settings
    public AIBehaviour behaviourMode = AIBehaviour.Chase;

    // Movement settings
    public float moveSpeed = 2f;
    public float rotationSpeed = 6f;
    public float detectionRange = 13f;

    // Wander settings
    public float wanderRadius = 7f;

    // Combat settings
    public float attackRange = 3f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    public float attackStickTime = 1f;
    private float nextAttackCheckTime = 0f;

    // Line of sight settings
    public bool requireLineOfSight = true;
    public LayerMask obstacleLayer;

    // Internal
    private Transform player;
    private HealthSystem playerHealth;
    private Vector3 wanderTarget;
    private Vector3 startPosition;
    private float lastAttackTime;
    private bool canSeePlayer;

    // Animations
    private Animator animator;
    private string[] currentAnimations;
    public string[] idleAnimations;
    public string[] runAnimations;
    public string[] attackAnimations;

    private int animIndex = 0;
    private float animEndTime = 0f;

    // Idle 
    private bool isIdling = false;
    private float idleEndTime = 0f;
    private Vector2 idleTimeRange = new Vector2(2f, 4f);
    private float idleChance = 0.2f;
    private bool alreadyIdled = false;

    public enum AIBehaviour { Chase, Idle }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            var clips = animator.runtimeAnimatorController.animationClips;

            idleAnimations = Array.ConvertAll(
                clips.Where(c => c.name.StartsWith("idle")).ToArray(),
                c => c.name
            );
            runAnimations = Array.ConvertAll(
                clips.Where(c => c.name.StartsWith("run")).ToArray(),
                c => c.name
            );
            attackAnimations = Array.ConvertAll(
                clips.Where(c => c.name.StartsWith("attack")).ToArray(),
                c => c.name
            );
        }
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerHealth = playerObject.GetComponent<HealthSystem>();
        }

        startPosition = transform.position;
        SetNewWanderTarget();
    }

    void Update()
    {
        if (!player) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        canSeePlayer = CheckLineOfSight();

        switch (behaviourMode)
        {
            case AIBehaviour.Chase:

                if (canSeePlayer && distanceToPlayer < detectionRange)
                {
                    ChasePlayer(distanceToPlayer);
                }
                else if (isIdling)
                {
                    Idle();
                }
                else
                {
                    Wander();
                }
                break;

            case AIBehaviour.Idle:
                Idle();
                break;
        }
    }

    // ---------- CORE MOVEMENT HELPERS ----------

    void MoveTowards(Vector3 target, float moveSpeedMultiplier)
    {
        if (animator) PlayAnimationFromSet(runAnimations);
        Vector3 dir = (target - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
        {
            // Rotate smoothly toward direction
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);

            // Move forward
            transform.position += transform.forward * moveSpeed * moveSpeedMultiplier * Time.deltaTime;
        }
    }

    // ---------- BEHAVIOURS ----------

    void StartIdle()
    {
        isIdling = true;
        idleEndTime = Time.time + UnityEngine.Random.Range(idleTimeRange.x, idleTimeRange.y);
    }

    void Idle()
    {
        if (animator) PlayAnimationFromSet(idleAnimations);
        // stay idle until timer ends
        if (Time.time < idleEndTime)
            return;

        // stop idling 
        isIdling = false;
    }

    void ChasePlayer(float distanceToPlayer)
    {
        // Stay in attack mode briefly even if distance changes slightly
        if (Time.time < nextAttackCheckTime)
        {
            AttackPlayer();
            return;
        }

        if (distanceToPlayer > attackRange)
        {
            MoveTowards(player.position, 1.5f);
        }
        else
        {
            AttackPlayer();
            nextAttackCheckTime = Time.time + attackStickTime;
        }
    }

    void Wander()
    {
        Vector3 dir = wanderTarget - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.25f)   
        {
            if (!alreadyIdled && UnityEngine.Random.value < idleChance)
            {
                alreadyIdled = true;
                StartIdle();
                return;
            }

            alreadyIdled = false;
            SetNewWanderTarget();
        }
        // If we are still moving
        MoveTowards(wanderTarget, 1f);
    }

    void AttackPlayer()
    {
        // Face the player while attacking
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0;
        Quaternion rot = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSpeed * Time.deltaTime);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            playerHealth?.TakeDamage(attackDamage);
            lastAttackTime = Time.time;
        }
        if (animator) PlayAnimationFromSet(attackAnimations, 0.05f);
    }

    // ---------- SUPPORT ----------

    bool CheckLineOfSight()
    {
        if (!requireLineOfSight)
            return true;

        Vector3 dir = player.position - transform.position;

        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, detectionRange, ~obstacleLayer))
        {
            Transform root = hit.transform.root;

            // Anything belonging to the player is valid
            return root == player;

        }

        return false;
    }

    void SetNewWanderTarget()
    {
        Vector2 circle = UnityEngine.Random.insideUnitCircle * wanderRadius;
        wanderTarget = startPosition + new Vector3(circle.x, 0, circle.y);
    }


    void PlayAnimationFromSet(string[] animSet, float fade = 0.1f)
    {
        if (!animator || animSet == null || animSet.Length == 0)
            return;

        // If still playing and state hasn't ended → do nothing
        if (currentAnimations == animSet && Time.time < animEndTime)
            return;

        currentAnimations = animSet;
        // Loop through animations in sequence
        animIndex = (animIndex + 1) % animSet.Length;
        string nextAnim = animSet[animIndex];
        //Debug.Log(nextAnim);
        AnimationClip clip = animator.runtimeAnimatorController.animationClips.First(c => c.name == nextAnim);
        float clipLength = clip.length;

        animEndTime = Time.time + clipLength;

        animator.CrossFadeInFixedTime(nextAnim, fade);
    }
}
