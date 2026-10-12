
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

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float movementThreshold = 0.1f;

    private NavMeshAgent agent;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    public Transform CurrentTarget => target;
    public Stronghold CurrentStronghold => targetStronghold;
    public NavMeshAgent Agent => agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (agent == null)
        {
            Debug.LogError(
                gameObject.name +
                " is missing a NavMeshAgent!"
            );
        }

        if (animator == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " has no Animator assigned or found in children."
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

        // Move toward the stronghold until it is breached.
        if (!targetStronghold.IsBreached)
        {
            MoveToStronghold();
            return;
        }

        // After the breach, chase a living character.
        ChaseLivingCharacter();
    }

    private void LateUpdate()
    {
        UpdateLocomotionAnimation();
    }

    private void UpdateLocomotionAnimation()
    {
        if (animator == null || agent == null)
            return;

        // Ignore movement while navigation is stopped.
        float movementSpeed = 0f;

        if (agent.isOnNavMesh && !agent.isStopped)
        {
            movementSpeed = agent.velocity.magnitude;
        }

        // This Blend Tree uses 0 for Idle and 1 for Run.
        float animationSpeed =
            movementSpeed > movementThreshold ? 1f : 0f;

        animator.SetFloat(SpeedHash, animationSpeed);
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
        if (!HasValidTarget())
        {
            Debug.Log(
                gameObject.name +
                " target is dead or invalid. Finding new target..."
            );

            FindLivingCharacter();
        }

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

        // Characters remain active when control switches.
        if (health.IsDead)
            return false;

        return true;
    }

    private void FindLivingCharacter()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        Transform closestCharacter = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            if (player == null)
                continue;

            PlayerHealth health =
                GetPlayerHealth(player.transform);

            if (health == null || health.IsDead)
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

        PlayerHealth health =
            character.GetComponent<PlayerHealth>();

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
