
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Strongholds")]
    [SerializeField] private Stronghold[] strongholds;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDelay = 1f;
    [SerializeField] private float navMeshSearchRadius = 3f;

    [Header("Wave Manager")]
    [SerializeField] private WaveManager waveManager;

    private void Awake()
    {
        if (waveManager == null)
            waveManager = FindFirstObjectByType<WaveManager>();
    }

    public void SpawnWave(int enemyCount)
    {
        StartCoroutine(SpawnEnemies(enemyCount));
    }

    private IEnumerator SpawnEnemies(int enemyCount)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: Enemy Prefab is missing.");
            FinishSpawning();
            yield break;
        }

        if (strongholds == null || strongholds.Length == 0)
        {
            Debug.LogError("EnemySpawner: No strongholds assigned.");
            FinishSpawning();
            yield break;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("EnemySpawner: No spawn points assigned.");
            FinishSpawning();
            yield break;
        }

        int successfullySpawned = 0;

        for (int i = 0; i < enemyCount; i++)
        {
            Stronghold stronghold = GetAvailableStronghold();
            Transform spawnPoint = GetRandomSpawnPoint();

            if (stronghold == null)
            {
                Debug.LogWarning(
                    "EnemySpawner: No active strongholds remain."
                );
                break;
            }

            if (spawnPoint == null)
            {
                Debug.LogError(
                    "EnemySpawner: All configured spawn points are missing."
                );
                break;
            }

            Vector3 spawnPosition = spawnPoint.position;

            // Prefer a nearby NavMesh position, but don't prevent spawning
            // if the check fails.
            if (NavMesh.SamplePosition(
                spawnPoint.position,
                out NavMeshHit hit,
                navMeshSearchRadius,
                NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
            }
            else
            {
                Debug.LogWarning(
                    "No NavMesh found near " + spawnPoint.name
                    + ". Spawning at the original position: "
                    + spawnPoint.position
                );
            }

            GameObject enemy = Instantiate(
                enemyPrefab,
                spawnPosition,
                spawnPoint.rotation
            );

            EnemyMovement movement =
                enemy.GetComponent<EnemyMovement>();

            EnemyAttack attack =
                enemy.GetComponent<EnemyAttack>();

            // Assign the target once.
            if (attack != null)
                attack.SetTargetStronghold(stronghold);
            else if (movement != null)
                movement.SetTarget(stronghold);

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

            if (agent == null)
            {
                Debug.LogError(
                    enemy.name + " has no NavMeshAgent component."
                );
            }
            else if (!agent.isOnNavMesh)
            {
                Debug.LogWarning(
                    enemy.name + " spawned but is not currently "
                    + "on a NavMesh. Check the NavMesh and agent settings."
                );
            }

            if (waveManager != null)
                waveManager.EnemySpawned(stronghold);

            successfullySpawned++;

            Debug.Log(
                "ENEMY SPAWNED: " + enemy.name
                + " | Position: " + spawnPosition
                + " | Target: " + stronghold.name
            );

            if (i < enemyCount - 1)
                yield return new WaitForSeconds(spawnDelay);
        }

        Debug.Log(
            "Finished spawning " + successfullySpawned
            + " of " + enemyCount + " planned enemies."
        );

        FinishSpawning();
    }

    private void FinishSpawning()
    {
        if (waveManager != null)
            waveManager.SpawningFinished();
    }

    private Transform GetRandomSpawnPoint()
    {
        List<Transform> validPoints = new List<Transform>();

        foreach (Transform point in spawnPoints)
        {
            if (point != null)
                validPoints.Add(point);
        }

        if (validPoints.Count == 0)
            return null;

        return validPoints[Random.Range(0, validPoints.Count)];
    }

    private Stronghold GetAvailableStronghold()
    {
        List<Stronghold> available = new List<Stronghold>();

        foreach (Stronghold stronghold in strongholds)
        {
            if (stronghold != null && !stronghold.IsDestroyed)
                available.Add(stronghold);
        }

        if (available.Count == 0)
            return null;

        return available[Random.Range(0, available.Count)];
    }

    public void ReassignEnemiesFrom(Stronghold destroyedStronghold)
    {
        if (destroyedStronghold == null)
            return;

        EnemyMovement[] enemies =
            FindObjectsByType<EnemyMovement>(
                FindObjectsSortMode.None
            );

        foreach (EnemyMovement movement in enemies)
        {
            if (movement == null ||
                movement.CurrentStronghold != destroyedStronghold)
                continue;

            Stronghold newTarget = GetNearestAvailableStronghold(
                movement.transform.position,
                destroyedStronghold
            );

            EnemyAttack attack =
                movement.GetComponent<EnemyAttack>();

            if (newTarget == null)
            {
                movement.ClearTarget();

                if (attack != null)
                    attack.SetTargetStronghold(null);

                continue;
            }

            if (attack != null)
                attack.SetTargetStronghold(newTarget);
            else
                movement.SetTarget(newTarget);

            Debug.Log(
                movement.name + " reassigned to " + newTarget.name
            );
        }
    }

    private Stronghold GetNearestAvailableStronghold(
        Vector3 position,
        Stronghold excludedStronghold)
    {
        Stronghold nearest = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Stronghold stronghold in strongholds)
        {
            if (stronghold == null ||
                stronghold == excludedStronghold ||
                stronghold.IsDestroyed)
                continue;

            float distance = Vector3.Distance(
                position,
                stronghold.transform.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = stronghold;
            }
        }

        return nearest;
    }
}
