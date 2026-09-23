using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Stronghold")]
    [SerializeField] private Stronghold targetStronghold;

    [Header("Movement")]
    [SerializeField] private float stoppingDistance = 1.5f;

    private NavMeshAgent agent;

    public Transform CurrentTarget => target;
    public Stronghold CurrentStronghold => targetStronghold;
    public NavMeshAgent Agent => agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError(gameObject.name + " is missing a NavMeshAgent!");
        }
    }

    private void Start()
    {
        if (agent == null)
            return;

        agent.stoppingDistance = stoppingDistance;
        agent.isStopped = false;

        Debug.Log(gameObject.name + " EnemyMovement started.");
    }

    private void Update()
    {
        if (agent == null)
            return;

        if (targetStronghold == null)
            return;

        // Before the stronghold is breached,
        // move toward the stronghold.
        if (!targetStronghold.IsBreached)
        {
            MoveToStronghold();
            return;
        }

        // Once breached, chase a living character.
        ChaseLivingCharacter();
    }

    private void MoveToStronghold()
    {
        if (target == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " has a Stronghold target but no EnemyTarget."
            );

            agent.isStopped = true;
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning(
                gameObject.name +
                " is NOT on the NavMesh!"
            );

            return;
        }

        agent.isStopped = false;
        agent.stoppingDistance = stoppingDistance;

        agent.SetDestination(target.position);
    }

    private void ChaseLivingCharacter()
    {
        // If our current target is dead,
        // immediately find another living character.
        if (!HasValidTarget())
        {
            Debug.Log(
                gameObject.name +
                " target is dead or invalid. Finding new target..."
            );

            FindLivingCharacter();
        }

        // No living characters left.
        if (target == null)
        {
            agent.isStopped = true;
            return;
        }

        if (!agent.isOnNavMesh)
            return;

        agent.isStopped = false;
        agent.stoppingDistance = stoppingDistance;

        agent.SetDestination(target.position);
    }

    private bool HasValidTarget()
    {
        if (target == null)
            return false;

        PlayerHealth health = GetPlayerHealth(target);

        if (health == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " could not find PlayerHealth on " +
                target.name
            );

            return false;
        }

        // IMPORTANT:
        // Do NOT check activeSelf here because your
        // character switching system keeps characters
        // alive in the world while changing control.
        if (health.IsDead)
            return false;

        return true;
    }

    private void FindLivingCharacter()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        Transform closestCharacter = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            if (player == null)
                continue;

            PlayerHealth health = GetPlayerHealth(player.transform);

            if (health == null)
                continue;

            // Skip dead characters.
            if (health.IsDead)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                player.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCharacter = player.transform;
            }
        }

        if (closestCharacter == null)
        {
            target = null;

            Debug.Log(
                gameObject.name +
                " could not find a living character."
            );

            return;
        }

        target = closestCharacter;

        Debug.Log(
            "NEW TARGET | " +
            gameObject.name +
            " → " +
            target.name
        );
    }

    private PlayerHealth GetPlayerHealth(Transform character)
    {
        if (character == null)
            return null;

        PlayerHealth health = character.GetComponent<PlayerHealth>();

        if (health == null)
            health = character.GetComponentInParent<PlayerHealth>();

        if (health == null)
            health = character.GetComponentInChildren<PlayerHealth>();

        return health;
    }

    public void SetTarget(Stronghold stronghold)
    {
        targetStronghold = stronghold;

        if (stronghold == null)
        {
            target = null;

            Debug.LogWarning(
                gameObject.name +
                ": Stronghold target is NULL."
            );

            return;
        }

        Transform enemyTarget =
            stronghold.transform.Find("EnemyTarget");

        if (enemyTarget == null)
        {
            target = null;

            Debug.LogError(
                gameObject.name +
                ": Could NOT find EnemyTarget under " +
                stronghold.name
            );

            return;
        }

        target = enemyTarget;

        Debug.Log(
            "MOVEMENT TARGET ASSIGNED | " +
            gameObject.name +
            " → " +
            stronghold.name +
            " | EnemyTarget: " +
            target.name
        );

        if (agent != null && !agent.isOnNavMesh)
        {
            Debug.LogWarning(
                gameObject.name +
                " is not currently on a NavMesh."
            );
        }
    }

    public void ClearTarget()
    {
        target = null;
        targetStronghold = null;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }
}