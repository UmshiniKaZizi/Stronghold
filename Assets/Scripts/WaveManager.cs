using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Spawner")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Wave Settings")]
    [SerializeField] private int startingEnemyCount = 5;
    [SerializeField] private int enemiesAddedPerWave = 2;

    [Header("Wave Timing")]
    [SerializeField] private float timeBetweenWaves = 5f;

    [Header("Strongholds")]
    [SerializeField] private Stronghold[] strongholds;

    private int currentWave = 0;
    private int enemiesRemaining = 0;

    private void Start()
    {
        StartCoroutine(
            StartNextWave()
        );
    }

    private IEnumerator StartNextWave()
    {
        currentWave++;

        int enemyCount =
            startingEnemyCount +
            (
                (currentWave - 1) *
                enemiesAddedPerWave
            );

        enemiesRemaining = enemyCount;

        Debug.Log(
            "WAVE " +
            currentWave +
            " STARTED | Enemies: " +
            enemyCount
        );

        if (enemySpawner != null)
        {
            enemySpawner.SpawnWave(
                enemyCount
            );
        }
        else
        {
            Debug.LogWarning(
                "WaveManager: Enemy Spawner is not assigned!"
            );

            yield break;
        }

        // Wait until all enemies are dead
        while (enemiesRemaining > 0)
        {
            yield return null;
        }

        Debug.Log(
            "WAVE " +
            currentWave +
            " COMPLETE!"
        );

        // Restore 50% of each Stronghold's
        // maximum force field
        RestoreStrongholds();

        // Wait before starting next wave
        yield return new WaitForSeconds(
            timeBetweenWaves
        );

        StartCoroutine(
            StartNextWave()
        );
    }

    private void RestoreStrongholds()
    {
        if (strongholds == null ||
            strongholds.Length == 0)
        {
            Debug.LogWarning(
                "WaveManager: No Strongholds assigned!"
            );

            return;
        }

        Debug.Log(
            "===== RESTORING STRONGHOLDS ====="
        );

        foreach (Stronghold stronghold in strongholds)
        {
            if (stronghold == null)
            {
                Debug.LogWarning(
                    "WaveManager: Found empty Stronghold slot."
                );

                continue;
            }

            float oldForceField =
                stronghold.currentForceField;

            float restoreAmount =
                stronghold.maxForceField * 0.5f;

            stronghold.RestoreForceField(
                restoreAmount
            );

            Debug.Log(
                stronghold.name +
                " | Force Field: " +
                oldForceField +
                " → " +
                stronghold.currentForceField
            );
        }

        Debug.Log(
            "===== RESTORATION COMPLETE ====="
        );
    }

    public void EnemyKilled()
{
    enemiesRemaining--;

    enemiesRemaining = Mathf.Max(enemiesRemaining, 0);

    Debug.Log(
        "ENEMY KILLED | Enemies Remaining: " +
        enemiesRemaining
    );

    if (enemiesRemaining == 0)
    {
        Debug.Log("ALL ENEMIES KILLED - WAVE COMPLETE");
    }
}

    public int GetCurrentWave()
    {
        return currentWave;
    }

    public int GetEnemiesRemaining()
    {
        return enemiesRemaining;
    }
}