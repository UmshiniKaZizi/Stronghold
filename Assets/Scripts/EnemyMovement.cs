using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Stronghold")]
    [SerializeField] private Stronghold targetStronghold;

    [Header("Movement")]
    [SerializeField] private float stoppingDistance = 1.5f;

    [Header("Player Targeting")]
    [SerializeField] private float targetRefreshTime = 2f;

    private NavMeshAgent agent;

    private float targetTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        agent.stoppingDistance =
            stoppingDistance;
    }

    private void Update()
    {
        if (targetStronghold == null)
            return;

        if (targetStronghold.IsBreached)
        {
            ChaseLivingCharacter();
        }
        else
        {
            MoveToStronghold();
        }
    }

    private void MoveToStronghold()
    {
        if (target == null)
            return;

        agent.isStopped = false;
        agent.stoppingDistance =
            stoppingDistance;

        agent.SetDestination(
            target.position
        );
    }

    private void ChaseLivingCharacter()
    {
        targetTimer -= Time.deltaTime;

        // Find a new character if we don't have one
        // or our current target has died.
        if (target == null ||
            !target.gameObject.activeInHierarchy ||
            IsTargetDead() ||
            targetTimer <= 0f)
        {
            FindLivingCharacter();

            targetTimer =
                targetRefreshTime;
        }

        if (target == null)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        agent.stoppingDistance = 1.5f;

        agent.SetDestination(
            target.position
        );
    }

    private bool IsTargetDead()
    {
        if (target == null)
            return true;

        PlayerHealth health =
            target.GetComponent<PlayerHealth>();

        if (health == null)
            return false;

        return health.IsDead;
    }

    private void FindLivingCharacter()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        System.Collections.Generic.List<Transform>
            livingCharacters =
            new System.Collections.Generic.List<Transform>();

        foreach (GameObject player in players)
        {
            if (!player.activeInHierarchy)
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
            return;
        }

        int randomIndex =
            Random.Range(
                0,
                livingCharacters.Count
            );

        target =
            livingCharacters[randomIndex];

        Debug.Log(
            gameObject.name +
            " is targeting " +
            target.name
        );
    }

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
            stronghold.transform.Find(
                "EnemyTarget"
            );

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