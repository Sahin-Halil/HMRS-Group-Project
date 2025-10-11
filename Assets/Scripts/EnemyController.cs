using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Behaviour Settings")]
    public bool shouldChase = true;

    [Header("Movement")]
    public float moveSpeed = 0.3f;
    public float chaseRange = 10f;

    [Header("Wander Settings")]
    public float wanderRadius = 5f;
    public float wanderChangeInterval = 3f;

    private Transform player;
    private Vector3 wanderTarget;
    private float wanderTimer;
    private Vector3 startPosition;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if(playerObject != null)
        {
            player = playerObject.transform;
        }
        startPosition = transform.position;
        SetNewWanderTarget();
    }

    // Update is called once per frame
    void Update()
    {
        if(shouldChase && player != null)
        {
            ChasePlayer();
        }
        else
        {
            Wander();
        }
    }

    void ChasePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if(distanceToPlayer < chaseRange)
        {
            // Travel towards the player
            Vector3 direction = (player.position - transform.position);
            transform.position += direction * moveSpeed * Time.deltaTime;

            // Face enemy towards player
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }
        else
        {
            // Wander if the player is too far
            Wander();
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
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        if(Application.isPlaying)
        {
            Gizmos.DrawWireSphere(startPosition, wanderRadius);
            Gizmos.DrawLine(transform.position, wanderTarget);
        }
    }
}
