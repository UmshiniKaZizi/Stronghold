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

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        agent.stoppingDistance = stoppingDistance;
    }

    private void Update()
    {
        if (targetStronghold == null)
            return;

        // Stronghold has not been breached
        if (!targetStronghold.IsBreached)
        {
            MoveToStronghold();
        }
        // Stronghold has been breached
        else
        {
            ChaseLivingCharacter();
        }
    }


    // ==========================================
    // MOVE TO STRONGHOLD
    // ==========================================

    private void MoveToStronghold()
    {
        if (target == null)
            return;

        agent.isStopped = false;
        agent.stoppingDistance = stoppingDistance;

        agent.SetDestination(target.position);
    }


    // ==========================================
    // CHASE CHARACTER
    // ==========================================

    private void ChaseLivingCharacter()
    {
        // If we already have a valid target,
        // KEEP FOLLOWING THAT CHARACTER.
        if (HasValidTarget())
        {
            agent.isStopped = false;
            agent.stoppingDistance = stoppingDistance;

            agent.SetDestination(target.position);

            return;
        }

        // Current target is dead/unavailable,
        // so find a new living character.
        FindLivingCharacter();

        if (target == null)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        agent.stoppingDistance = stoppingDistance;

        agent.SetDestination(target.position);
    }


    // ==========================================
    // CHECK CURRENT TARGET
    // ==========================================

    private bool HasValidTarget()
    {
        if (target == null)
            return false;

        if (!target.gameObject.activeSelf)
            return false;

        PlayerHealth health =
            target.GetComponent<PlayerHealth>();

        if (health == null)
            return false;

        if (health.IsDead)
            return false;

        return true;
    }


    // ==========================================
    // FIND LIVING CHARACTER
    // ==========================================

    private void FindLivingCharacter()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        List<Transform> livingCharacters =
            new List<Transform>();

        foreach (GameObject player in players)
        {
            if (!player.activeSelf)
                continue;

            PlayerHealth health =
                player.GetComponent<PlayerHealth>();

            if (health == null)
                continue;

            if (health.IsDead)
                continue;

            livingCharacters.Add(
                player.transform
            );
        }

        if (livingCharacters.Count == 0)
        {
            target = null;

            Debug.Log(
                gameObject.name +
                " could not find a living character."
            );

            return;
        }

        // Find the closest living character
        Transform closestCharacter = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform character in livingCharacters)
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    character.position
                );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCharacter = character;
            }
        }

        target = closestCharacter;

        Debug.Log(
            gameObject.name +
            " is now targeting " +
            target.name
        );
    }


    // ==========================================
    // SET STRONGHOLD TARGET
    // ==========================================

    public void SetTarget(Stronghold stronghold)
    {
        targetStronghold = stronghold;

        if (stronghold == null)
        {
            Debug.LogWarning(
                gameObject.name +
                ": Stronghold is null."
            );

            return;
        }

        Transform enemyTarget =
            stronghold.transform.Find("EnemyTarget");

        if (enemyTarget != null)
        {
            target = enemyTarget;
        }
        else
        {
            Debug.LogWarning(
                gameObject.name +
                ": EnemyTarget was not found under " +
                stronghold.name
            );
        }
    }
}