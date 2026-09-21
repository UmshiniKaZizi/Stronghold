using UnityEngine;
using System.Collections;

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

    public void SpawnWave(int enemyCount)
    {
        StartCoroutine(SpawnEnemies(enemyCount));
    }

    private IEnumerator SpawnEnemies(int enemyCount)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning(
                "EnemySpawner: Enemy Prefab is not assigned!"
            );

            yield break;
        }

        if (strongholds == null ||
            strongholds.Length == 0)
        {
            Debug.LogWarning(
                "EnemySpawner: No Strongholds assigned!"
            );

            yield break;
        }

        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            Debug.LogWarning(
                "EnemySpawner: No Spawn Points assigned!"
            );

            yield break;
        }

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();

            yield return new WaitForSeconds(
                spawnDelay
            );
        }

        Debug.Log(
            "Finished spawning " +
            enemyCount +
            " enemies."
        );
    }

    private void SpawnEnemy()
    {
        // Pick a random spawn point
        Transform spawnPoint =
            spawnPoints[
                Random.Range(
                    0,
                    spawnPoints.Length
                )
            ];

        // Pick a random Stronghold
        Stronghold targetStronghold =
            strongholds[
                Random.Range(
                    0,
                    strongholds.Length
                )
            ];

        // Spawn enemy
        GameObject enemy =
            Instantiate(
                enemyPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        // Give enemy its Stronghold target
        EnemyMovement movement =
            enemy.GetComponent<EnemyMovement>();

        EnemyAttack attack =
            enemy.GetComponent<EnemyAttack>();

        if (movement != null)
        {
            movement.SetTarget(
                targetStronghold
            );
        }

        if (attack != null)
        {
            attack.SetTargetStronghold(
                targetStronghold
            );
        }

        Debug.Log(
            enemy.name +
            " targeting " +
            targetStronghold.name
        );
    }
}