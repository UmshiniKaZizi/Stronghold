
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
    [SerializeField] private float firstWaveDelay = 2f;

    [Header("Strongholds")]
    [SerializeField] private Stronghold[] strongholds;

    [Header("Wave UI")]
    [SerializeField] private WaveMessageUI waveMessageUI;

    [Header("Artillery Rewards")]
    [SerializeField] private ArtilleryBombInventory artilleryBombInventory;

    private int currentWave;
    private int enemiesRemaining;

    private bool spawningFinished;
    private bool waveInProgress;

    private readonly Dictionary<Stronghold, int> enemiesPerStronghold =
        new Dictionary<Stronghold, int>();

    private readonly HashSet<Stronghold> assignedStrongholds =
        new HashSet<Stronghold>();

    private readonly HashSet<Stronghold> restoredStrongholds =
        new HashSet<Stronghold>();

    private void Start()
    {
        if (enemySpawner == null)
        {
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
        }

        if (enemySpawner == null)
        {
            Debug.LogError(
                "[WaveManager] EnemySpawner is missing!",
                this
            );
            enabled = false;
            return;
        }

        if (artilleryBombInventory == null)
        {
            Debug.LogError(
                "[WaveManager] ArtilleryBombInventory is NOT ASSIGNED! " +
                "Kill rewards cannot work.",
                this
            );
        }
        else
        {
            Debug.Log(
                $"[WaveManager] Artillery inventory connected: " +
                $"{artilleryBombInventory.name}",
                this
            );
        }

        StartCoroutine(WaitForGameStart());
    }

    private IEnumerator WaitForGameStart()
    {
        while (GameManager.Instance == null ||
               !GameManager.Instance.HasGameStarted)
        {
            yield return null;
        }

        yield return new WaitForSeconds(firstWaveDelay);

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
            $"[WaveManager] WAVE {currentWave} STARTED | " +
            $"Planned enemies: {enemyCount}",
            this
        );

        if (waveMessageUI != null)
        {
            waveMessageUI.ShowWaveIncoming();
        }

        enemySpawner.SpawnWave(enemyCount);

        while (!spawningFinished)
        {
            yield return null;
        }

        TryRestoreEligibleStrongholds();

        while (enemiesRemaining > 0)
        {
            yield return null;
        }

        TryRestoreEligibleStrongholds();

        waveInProgress = false;

        Debug.Log(
            $"[WaveManager] WAVE {currentWave} COMPLETE!",
            this
        );

        if (waveMessageUI != null)
        {
            waveMessageUI.ShowWaveComplete();
        }
    }

    // ==========================================
    // SPAWNING CALLBACKS
    // ==========================================

    public void EnemySpawned(Stronghold assignedStronghold)
    {
        if (!waveInProgress)
        {
            Debug.LogWarning(
                "[WaveManager] EnemySpawned called outside an active wave.",
                this
            );
            return;
        }

        enemiesRemaining++;

        if (assignedStronghold != null &&
            !assignedStronghold.IsDestroyed)
        {
            if (!enemiesPerStronghold.ContainsKey(assignedStronghold))
            {
                enemiesPerStronghold[assignedStronghold] = 0;
            }

            enemiesPerStronghold[assignedStronghold]++;

            assignedStrongholds.Add(assignedStronghold);
        }

        Debug.Log(
            $"[WaveManager] ENEMY SPAWNED | " +
            $"Wave: {currentWave} | " +
            $"Alive: {enemiesRemaining} | " +
            $"Stronghold: {(assignedStronghold != null ? assignedStronghold.name : "None")}",
            this
        );
    }

    public void SpawningFinished()
    {
        spawningFinished = true;

        Debug.Log(
            $"[WaveManager] SPAWNING FINISHED | Wave {currentWave}",
            this
        );

        TryRestoreEligibleStrongholds();
    }

    // ==========================================
    // ENEMY DEATH
    // ==========================================

    private void RegisterArtilleryKill()
    {
        Debug.Log(
            "[WaveManager] RegisterArtilleryKill() CALLED.",
            this
        );

        if (artilleryBombInventory == null)
        {
            Debug.LogError(
                "[WaveManager] Cannot register artillery kill. " +
                "Inventory reference is NULL!",
                this
            );
            return;
        }

        Debug.Log(
            $"[WaveManager] Sending kill to inventory: " +
            $"{artilleryBombInventory.name}",
            this
        );

        artilleryBombInventory.RegisterKill();
    }

    public void EnemyKilled(Stronghold assignedStronghold)
    {
        Debug.Log(
            $"[WaveManager] EnemyKilled(Stronghold) RECEIVED | " +
            $"Stronghold: {(assignedStronghold != null ? assignedStronghold.name : "None")}",
            this
        );

        // Register the kill for the current wave.
        if (GameScoreManager.Instance != null)
        {
            GameScoreManager.Instance.RegisterEnemyKill(currentWave);
        }


        enemiesRemaining = Mathf.Max(enemiesRemaining - 1, 0);

        RegisterArtilleryKill();

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
            $"[WaveManager] ENEMY KILLED | Remaining in wave: {enemiesRemaining}",
            this
        );

        TryRestoreEligibleStrongholds();
    }

    public void EnemyKilled(Stronghold assignedStronghold, GameObject killer)
    {
        if (assignedStronghold != null)
            assignedStronghold.RegisterPlayerKill(killer);

        EnemyKilled(assignedStronghold);
    }


    public void EnemyKilled()
    {
        Debug.Log(
            "[WaveManager] EnemyKilled() RECEIVED (no stronghold).",
            this
        );

        // Register kills even when no stronghold is assigned.
        if (GameScoreManager.Instance != null)
        {
            GameScoreManager.Instance.RegisterEnemyKill(currentWave);
        }


        enemiesRemaining = Mathf.Max(enemiesRemaining - 1, 0);

        RegisterArtilleryKill();

        Debug.Log(
            $"[WaveManager] ENEMY KILLED | Remaining in wave: {enemiesRemaining}",
            this
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

            stronghold.RestoreForceField(
                stronghold.maxForceField * 1f //changed this to 100% integrity
            );

            restoredStrongholds.Add(stronghold);

            Debug.Log(
                $"[WaveManager] STRONGHOLD RESTORED | " +
                $"{stronghold.name} | Wave {currentWave}",
                this
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
