
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
    [SerializeField] private float destinationUpdateInterval = 0.25f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float movementThreshold = 0.1f;

    [Header("NavMesh Recovery")]
    [SerializeField] private float navMeshSearchRadius = 3f;
    [SerializeField] private bool snapToNavMeshOnStart = true;

    private NavMeshAgent agent;
    private StrongholdObject targetObject;

    private float nextDestinationUpdate;
    private bool movementPaused;
    private bool reportedInvalidAgent;

    public Transform CurrentTarget => target;
    public Stronghold CurrentStronghold => targetStronghold;
    public StrongholdObject CurrentTargetObject => targetObject;
    public NavMeshAgent Agent => agent;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (agent == null)
        {
            Debug.LogError(
                name + " is missing a NavMeshAgent!", this);
            enabled = false;
            return;
        }

        agent.stoppingDistance = stoppingDistance;

        if (animator == null)
        {
            Debug.LogWarning(
                name + " has no Animator assigned.", this);
        }
    }

    private void Start()
    {
        if (agent == null)
            return;

        // Spawned enemies may appear slightly off the NavMesh.
        if (!agent.isOnNavMesh && snapToNavMeshOnStart)
        {
            if (NavMesh.SamplePosition(
                transform.position,
                out NavMeshHit hit,
                navMeshSearchRadius,
                NavMesh.AllAreas))
            {
                if (agent.Warp(hit.position))
                {
                    Debug.Log(
                        name + " snapped to NavMesh at " +
                        hit.position, this);
                }
            }
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError(
                name + " is NOT on the NavMesh. Position: " +
                transform.position, this);
            reportedInvalidAgent = true;
            return;
        }

        agent.isStopped = false;

        // Set the initial destination after the agent is placed.
        if (target != null)
    UpdateDestination(target);
    }

    private void Update()
    {
        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
        {
            if (!reportedInvalidAgent)
            {
                Debug.LogError(
                    name + " left the NavMesh. Position: " +
                    transform.position, this);
                reportedInvalidAgent = true;
            }

            return;
        }

        reportedInvalidAgent = false;

        // EnemyAttack controls when movement is paused.
        // Never restart movement while an attack is in progress.
        if (movementPaused)
            return;

        if (targetStronghold == null)
            return;

        if (targetStronghold.IsDestroyed)
        {
            StopMoving();
            return;
        }

        if (!targetStronghold.IsBreached)
        {
            targetObject = null;

            Transform fieldTarget =
                targetStronghold.transform.Find("EnemyTarget");

            if (fieldTarget == null)
            {
                StopMoving();
                return;
            }

            UpdateDestination(fieldTarget);
            return;
        }

        // EnemyAttack chooses the player/object target.
        if (target == null)
        {
            StopMoving();
            return;
        }

        // If the object target has been destroyed, don't keep
        // following its Transform. EnemyAttack can choose another.
        if (targetObject != null && targetObject.IsDestroyed)
        {
            targetObject = null;
            target = null;
            StopMoving();
            return;
        }

        UpdateDestination(target);
    }

    private void UpdateDestination(Transform destination)
    {
        if (destination == null ||
            agent == null ||
            !agent.isOnNavMesh ||
            movementPaused)
            return;

        if (Time.time < nextDestinationUpdate)
            return;

        nextDestinationUpdate =
            Time.time + destinationUpdateInterval;

        agent.stoppingDistance = stoppingDistance;

        if (!agent.hasPath ||
            Vector3.Distance(
                agent.destination,
                destination.position) > 0.5f)
        {
            bool accepted =
                agent.SetDestination(destination.position);

            if (!accepted)
            {
                Debug.LogWarning(
                    name + " could not calculate a path to " +
                    destination.name, this);
                return;
            }
        }

        if (agent.isStopped)
            agent.isStopped = false;
    }

    private void LateUpdate()
    {
        if (animator == null || agent == null)
            return;

        float movementSpeed = 0f;

        if (agent.isOnNavMesh &&
            !agent.isStopped &&
            !movementPaused)
        {
            movementSpeed = agent.velocity.magnitude;
        }

        animator.SetFloat(
            SpeedHash,
            movementSpeed > movementThreshold ? 1f : 0f);
    }

    // ==========================================
    // TARGET CONTROL
    // ==========================================

    public void SetTarget(Stronghold stronghold)
    {
        targetStronghold = stronghold;
        targetObject = null;
        target = null;

        if (stronghold == null)
        {
            StopMoving();
            Debug.LogWarning(
                name + ": Stronghold target is null.", this);
            return;
        }

        Transform fieldTarget =
            stronghold.transform.Find("EnemyTarget");

        if (fieldTarget == null)
        {
            StopMoving();
            Debug.LogError(
                "EnemyTarget not found under " +
                stronghold.name, stronghold);
            return;
        }

        target = fieldTarget;
        movementPaused = false;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(fieldTarget.position);
            nextDestinationUpdate =
                Time.time + destinationUpdateInterval;
        }

        Debug.Log(
            "MOVEMENT TARGET ASSIGNED | " +
            name + " -> " + stronghold.name, this);
    }

    public void SetTargetObject(StrongholdObject obj)
    {
        if (obj == null || obj.IsDestroyed)
            return;

        targetObject = obj;
        target = obj.transform;
        movementPaused = false;

        Debug.Log(
            "OBJECT TARGET ASSIGNED | " +
            name + " -> " + obj.name, this);

        UpdateDestination(target);
    }

    public void SetTargetPlayer(Transform player)
    {
        if (player == null)
            return;

        targetObject = null;
        target = player;
        movementPaused = false;

        Debug.Log(
            "PLAYER TARGET ASSIGNED | " +
            name + " -> " + player.name, this);

        UpdateDestination(target);
    }

    public void ClearTarget()
    {
        target = null;
        targetObject = null;
        targetStronghold = null;

        StopMoving();
    }

    public void StopMoving()
    {
        movementPaused = true;

        if (agent == null || !agent.isOnNavMesh)
            return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    public void ResumeMoving()
    {
        movementPaused = false;

        if (agent == null || !agent.isOnNavMesh)
            return;

        agent.isStopped = false;
        nextDestinationUpdate = 0f;
    }
}
