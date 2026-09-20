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

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.stoppingDistance = stoppingDistance;
    }

    private void Start()
    {
        UpdateDestination();
    }

    private void Update()
    {
        if (targetStronghold == null)
            return;

        if (targetStronghold.IsBreached)
        {
            EnterStronghold();
        }
        else
        {
            AttackPosition();
        }
    }

    private void AttackPosition()
    {
        if (target == null)
            return;

        agent.isStopped = false;
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(target.position);
    }

    private void EnterStronghold()
    {
        agent.isStopped = false;
        agent.stoppingDistance = 0f;

        Vector3 strongholdPosition = targetStronghold.transform.position;

        agent.SetDestination(strongholdPosition);
    }

    private void UpdateDestination()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }
}