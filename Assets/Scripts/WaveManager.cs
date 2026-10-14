
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private int currentWave;
    private int enemiesRemaining;

    private bool spawningFinished;
    private bool waveInProgress;

    // Number of living enemies assigned to each stronghold.
    private readonly Dictionary<Stronghold, int> enemiesPerStronghold =
        new Dictionary<Stronghold, int>();

    // Strongholds that received enemies during this wave.
    private readonly HashSet<Stronghold> assignedStrongholds =
        new HashSet<Stronghold>();

    // Prevents a stronghold from restoring more than once per wave.
    private readonly HashSet<Stronghold> restoredStrongholds =
        new HashSet<Stronghold>();

    private void Start()
    {
        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();

        if (enemySpawner == null)
        {
            Debug.LogError("WaveManager: EnemySpawner is missing!");
            enabled = false;
            return;
        }

        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        while (true)
        {
            yield return StartCoroutine(StartNextWave());

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private IEnumerator StartNextWave()
    {
        currentWave++;

        int enemyCount =
            startingEnemyCount +
            ((currentWave - 1) * enemiesAddedPerWave);

        enemiesRemaining = 0;
        spawningFinished = false;
        waveInProgress = true;

        enemiesPerStronghold.Clear();
        assignedStrongholds.Clear();
        restoredStrongholds.Clear();

        Debug.Log(
            "WAVE " + currentWave +
            " STARTED | Planned enemies: " + enemyCount
        );

        enemySpawner.SpawnWave(enemyCount);

        // Wait until the spawner confirms all spawning is finished.
        while (!spawningFinished)
            yield return null;

        // Handle strongholds that received no enemies.
        // They should not receive a restoration for this wave.
        TryRestoreEligibleStrongholds();

        // Wait until all spawned enemies have been eliminated.
        while (enemiesRemaining > 0)
            yield return null;

        // Final check in case a stronghold became eligible at wave end.
        TryRestoreEligibleStrongholds();

        waveInProgress = false;

        Debug.Log("WAVE " + currentWave + " COMPLETE!");
    }

    // ==========================================
    // SPAWNING CALLBACKS
    // ==========================================

    public void EnemySpawned(Stronghold assignedStronghold)
    {
        if (!waveInProgress)
            return;

        enemiesRemaining++;

        if (assignedStronghold != null &&
            !assignedStronghold.IsDestroyed)
        {
            if (!enemiesPerStronghold.ContainsKey(assignedStronghold))
                enemiesPerStronghold[assignedStronghold] = 0;

            enemiesPerStronghold[assignedStronghold]++;

            assignedStrongholds.Add(assignedStronghold);
        }

        Debug.Log(
            "ENEMY SPAWNED | Wave: " + currentWave +
            " | Alive: " + enemiesRemaining +
            " | Stronghold: " +
            (assignedStronghold != null
                ? assignedStronghold.name
                : "None")
        );
    }

    public void SpawningFinished()
    {
        spawningFinished = true;

        Debug.Log(
            "SPAWNING FINISHED | Wave " + currentWave
        );

        TryRestoreEligibleStrongholds();
    }

    // ==========================================
    // ENEMY DEATH
    // ==========================================

    // Called by EnemyHealth when an enemy dies.
    public void EnemyKilled(Stronghold assignedStronghold)
    {
        enemiesRemaining = Mathf.Max(enemiesRemaining - 1, 0);

        if (assignedStronghold != null &&
            enemiesPerStronghold.ContainsKey(assignedStronghold))
        {
            enemiesPerStronghold[assignedStronghold] =
                Mathf.Max(
                    enemiesPerStronghold[assignedStronghold] - 1,
                    0
                );
        }

        Debug.Log(
            "ENEMY KILLED | Remaining in wave: " +
            enemiesRemaining
        );

        TryRestoreEligibleStrongholds();
    }

    // Compatibility for any existing scripts calling EnemyKilled().
    public void EnemyKilled()
    {
        enemiesRemaining = Mathf.Max(enemiesRemaining - 1, 0);

        Debug.Log(
            "ENEMY KILLED | Remaining in wave: " +
            enemiesRemaining
        );
    }

    // ==========================================
    // PER-STRONGHOLD RESTORATION
    // ==========================================

    private void TryRestoreEligibleStrongholds()
    {
        if (!spawningFinished)
            return;

        foreach (Stronghold stronghold in assignedStrongholds)
        {
            if (stronghold == null ||
                stronghold.IsDestroyed ||
                restoredStrongholds.Contains(stronghold))
            {
                continue;
            }

            if (!enemiesPerStronghold.ContainsKey(stronghold))
                continue;

            if (enemiesPerStronghold[stronghold] > 0)
                continue;

            // All enemies assigned to this stronghold are dead.
            stronghold.RestoreForceField(
                stronghold.maxForceField * 0.5f
            );

            restoredStrongholds.Add(stronghold);

            Debug.Log(
                "STRONGHOLD RESTORED | " + stronghold.name +
                " | Wave " + currentWave
            );
        }
    }

    // ==========================================
    // PUBLIC GETTERS
    // ==========================================

    public int GetCurrentWave()
    {
        return currentWave;
    }

    public int GetEnemiesRemaining()
    {
        return enemiesRemaining;
    }

    public int GetEnemiesRemaining(Stronghold stronghold)
    {
        if (stronghold == null ||
            !enemiesPerStronghold.ContainsKey(stronghold))
        {
            return 0;
        }

        return enemiesPerStronghold[stronghold];
    }
}
