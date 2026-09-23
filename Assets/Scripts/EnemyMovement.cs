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

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError(
                gameObject.name +
                " is missing a NavMeshAgent!"
            );
        }
    }

    private void Start()
    {
        if (agent == null)
            return;

        agent.stoppingDistance = stoppingDistance;
        agent.isStopped = false;

        Debug.Log(
            gameObject.name +
            " EnemyMovement started."
        );
    }

    private void Update()
    {
        if (agent == null)
            return;

        if (targetStronghold == null)
            return;

        // ==========================================
        // STRONGHOLD NOT BREACHED
        // ==========================================

        if (!targetStronghold.IsBreached)
        {
            MoveToStronghold();
        }

        // ==========================================
        // STRONGHOLD BREACHED
        // ==========================================

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
        {
            Debug.LogWarning(
                gameObject.name +
                " has a Stronghold target but no EnemyTarget!"
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

        agent.SetDestination(
            target.position
        );
    }


    // ==========================================
    // CHASE CHARACTER
    // ==========================================

    private void ChaseLivingCharacter()
    {
        if (HasValidTarget())
        {
            if (!agent.isOnNavMesh)
                return;

            agent.isStopped = false;
            agent.stoppingDistance = stoppingDistance;

            agent.SetDestination(
                target.position
            );

            return;
        }

        FindLivingCharacter();

        if (target == null)
        {
            agent.isStopped = true;
            return;
        }

        if (!agent.isOnNavMesh)
            return;

        agent.isStopped = false;
        agent.stoppingDistance = stoppingDistance;

        agent.SetDestination(
            target.position
        );
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

        Transform closestCharacter = null;

        float closestDistance =
            Mathf.Infinity;

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
            " is now targeting character: " +
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
            target = null;

            Debug.LogWarning(
                gameObject.name +
                ": Stronghold target is NULL."
            );

            return;
        }

        Transform enemyTarget =
            stronghold.transform.Find(
                "EnemyTarget"
            );

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

        // ==========================================
        // NAVMESH CHECK
        // ==========================================

        if (agent != null)
        {
            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning(
                    gameObject.name +
                    " is not currently on a NavMesh."
                );
            }
        }
    }


    // ==========================================
    // CLEAR TARGET
    // ==========================================

    public void ClearTarget()
    {
        target = null;
        targetStronghold = null;

        if (agent != null &&
            agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }
}