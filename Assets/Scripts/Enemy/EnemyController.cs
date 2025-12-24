using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // Behaviour settings
    public AIBehaviour behaviourMode = AIBehaviour.Chase;

    // Movement settings
    public float moveSpeed = 2f;
    public float rotationSpeed = 6f;
    public float detectionRange = 10f;
    public float fleeDistance = 8f;

    // Wander settings
    public float wanderRadius = 5f;
    public float wanderChangeInterval = 3f;

    // Combat settings
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    // Line of sight settings
    public bool requireLineOfSight = true;
    public LayerMask obstacleLayer;

    // Internal
    private Transform player;
    private HealthSystem playerHealth;
    private Vector3 wanderTarget;
    private float wanderTimer;
    private Vector3 startPosition;
    private float lastAttackTime;
    private bool canSeePlayer;

    public enum AIBehaviour { Chase, Wander }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
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
                    ChasePlayer(distanceToPlayer);
                else
                    Wander();
                break;

            case AIBehaviour.Wander:
                Wander();
                break;
        }
    }

    // ---------- CORE MOVEMENT HELPERS ----------

    void MoveTowards(Vector3 target)
    {
        Vector3 dir = (target - transform.position);
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
        {
            // Rotate smoothly toward direction
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);

            // Move forward
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
    }

    // ---------- BEHAVIOURS ----------

    void ChasePlayer(float distanceToPlayer)
    {
        if (distanceToPlayer > attackRange)
        {
            MoveTowards(player.position);
        }
        else
        {
            AttackPlayer();
        }
    }

    void Wander()
    {
        wanderTimer += Time.deltaTime;

        if (wanderTimer >= wanderChangeInterval)
        {
            SetNewWanderTarget();
            wanderTimer = 0f;
        }

        MoveTowards(wanderTarget);

        if (Vector3.Distance(transform.position, wanderTarget) < 0.5f)
            SetNewWanderTarget();
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
    }

    // ---------- SUPPORT ----------

    bool CheckLineOfSight()
    {
        if (!requireLineOfSight)
            return true;

        Vector3 dir = player.position - transform.position;

        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, detectionRange, ~obstacleLayer))
            return hit.transform == player;

        return false;
    }

    void SetNewWanderTarget()
    {
        Vector2 circle = UnityEngine.Random.insideUnitCircle * wanderRadius;
        wanderTarget = startPosition + new Vector3(circle.x, 0, circle.y);
    }
}
