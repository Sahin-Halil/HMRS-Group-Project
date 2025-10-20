using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("AI Behaviour")]
    public AIBehaviour behaviourMode = AIBehaviour.Chase;

    [Header("Movement")]
    public float moveSpeed = 0.3f;
    public float detectionRange = 10f;
    public float fleeDistance = 8f;

    [Header("Wander Settings")]
    public float wanderRadius = 5f;
    public float wanderChangeInterval = 3f;

    [Header("Combat")]
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    [Header("Line of Sight")]
    public bool requireLineOfSight = true;
    public LayerMask obstacleLayer; // Set to walls/obstacles when complete

    private Transform player;
    private HealthSystem playerHealth;
    private Vector3 wanderTarget;
    private float wanderTimer;
    private Vector3 startPosition;
    private float lastAttackTime;
    private bool canSeePlayer;

    public enum AIBehaviour
    {
        Chase,      // Aggressive - chase player
        Flee,       // Scared - runs away from player
        Wander,     // Passive - just wanders
        Territorial // Chases only within territory
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if(playerObject != null)
        {
            player = playerObject.transform;
            playerHealth = playerObject.GetComponent<HealthSystem>();
        }

        startPosition = transform.position;
        SetNewWanderTarget();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        canSeePlayer = CheckLineOfSight();

        switch(behaviourMode)
        {
            case AIBehaviour.Chase:
                if(canSeePlayer && distanceToPlayer < detectionRange)
                {
                    ChasePlayer(distanceToPlayer);
                }
                else
                {
                    Wander();
                }
                break;

            case AIBehaviour.Flee:
                if(canSeePlayer && distanceToPlayer < detectionRange)
                {
                    FleeFromPlayer();
                }
                else
                {
                    Wander();
                }
                break;

            case AIBehaviour.Wander:
                Wander();
                break;

            case AIBehaviour.Territorial:
                float distanceFromStart = Vector3.Distance(transform.position, startPosition);
                if(canSeePlayer && distanceToPlayer < detectionRange && distanceFromStart < wanderRadius)
                {
                    ChasePlayer(distanceToPlayer);
                }
                else if(distanceFromStart > wanderRadius)
                {
                    // Return to territory
                    Vector3 directionHome = (startPosition - transform.position).normalized;
                    transform.position += directionHome * moveSpeed * Time.deltaTime;
                }
                else
                {
                    Wander();
                }
                break;
        }
    }

    bool CheckLineOfSight()
    {
        if(!requireLineOfSight) return true;

        Vector3 directionToPlayer = player.position - transform.position;

        if(Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, detectionRange, ~obstacleLayer))
        {
            return hit.transform == player;
        }
        return false;
    }

    void ChasePlayer(float distanceToPlayer)
    {
        if(distanceToPlayer > attackRange)
        {
            // Move towards player
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
        else
        {
            // In attack range
            AttackPlayer();
        }
    }

    void FleeFromPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if(distanceToPlayer < fleeDistance)
        {
            // Run away from player
            Vector3 direction = (transform.position - player.position).normalized;
            transform.position += direction * moveSpeed * 1.2f * Time.deltaTime; // Flee faster

            // Face away from player
            transform.LookAt(transform.position + direction);
        }
        else
        {
            Wander();
        }
    }

    void AttackPlayer()
    {
        // Face player
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        if(Time.time >= lastAttackTime + attackCooldown)
        {
            if(playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"{gameObject.name} attacked player for {attackDamage} damage!");
            }
            lastAttackTime = Time.time;
        }
    }

    void Wander()
    {
        wanderTimer += Time.deltaTime;

        // Change direction every few seconds
        if(wanderTimer >= wanderChangeInterval)
        {
            SetNewWanderTarget();
            wanderTimer = 0f;
        }

        // Move towards wander target
        Vector3 direction = (wanderTarget - transform.position).normalized;
        transform.position += direction * (moveSpeed * 0.5f) * Time.deltaTime;

        // Stop when close to target
        if(Vector3.Distance(transform.position, wanderTarget) < 0.5f)
        {
            SetNewWanderTarget();
        }
    }

    void SetNewWanderTarget()
    {
        // Pick random point between wanderRadius and start pos
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * wanderRadius;
        wanderTarget = startPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    // Visualise chase range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (behaviourMode == AIBehaviour.Flee)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, fleeDistance);
        }

        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPosition, wanderRadius);
            Gizmos.DrawLine(transform.position, wanderTarget);

            if (player != null && canSeePlayer)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, player.position);
            }
        }
    }
}
