using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameScoreManager : MonoBehaviour
{
    public static GameScoreManager Instance;

    [Header("Survival Timer")]
    [SerializeField] private TMP_Text survivalTimerText;

    private float survivalTime;
    private bool timerRunning;
    private bool scoringHasStarted;

    [Header("Wave Scoring")]
    private Dictionary<int, int> waveKills =
        new Dictionary<int, int>();

    public float SurvivalTime => survivalTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        survivalTime = 0f;
        timerRunning = false;
        scoringHasStarted = false;

        // Hide timer during onboarding.
        if (survivalTimerText != null)
        {
            survivalTimerText.enabled = false;
        }
    }

    private void Update()
    {
        // Wait until the actual game has started.
        if (!scoringHasStarted)
        {
            if (GameManager.Instance == null ||
                !GameManager.Instance.HasGameStarted)
            {
                return;
            }

            StartScoring();
        }

        if (!timerRunning)
            return;

        survivalTime += Time.deltaTime;

        UpdateTimerUI();
    }

    // Called when gameplay officially begins.
    public void StartScoring()
    {
        // Prevent the timer from starting during onboarding.
        if (GameManager.Instance != null &&
            !GameManager.Instance.HasGameStarted)
        {
            return;
        }

        // Prevent it from resetting if called twice.
        if (scoringHasStarted)
            return;

        survivalTime = 0f;
        timerRunning = true;
        scoringHasStarted = true;

        waveKills.Clear();

        if (survivalTimerText != null)
        {
            survivalTimerText.enabled = true;
        }

        UpdateTimerUI();

        Debug.Log("SCORING STARTED");
    }

    // Stop the timer when the session ends.
    public void StopScoring()
    {
        timerRunning = false;

        Debug.Log(
            "SCORING STOPPED | Time: " +
            GetFormattedTime()
        );
    }

    // Record an enemy kill for the current wave.
    public void RegisterEnemyKill(int waveNumber)
    {
        if (!timerRunning ||
            waveNumber <= 0)
        {
            return;
        }

        if (!waveKills.ContainsKey(waveNumber))
        {
            waveKills[waveNumber] = 0;
        }

        waveKills[waveNumber]++;

        Debug.Log(
            "WAVE " +
            waveNumber +
            " | ENEMIES KILLED: " +
            waveKills[waveNumber]
        );
    }

    // Return the kills recorded for a particular wave.
    public int GetKillsForWave(int waveNumber)
    {
        if (waveKills.ContainsKey(waveNumber))
        {
            return waveKills[waveNumber];
        }

        return 0;
    }

    // Return the recorded wave results.
    public Dictionary<int, int> GetWaveResults()
    {
        return waveKills;
    }

    // Format the survival time into minutes and seconds.
    public string GetFormattedTime()
    {
        int minutes =
            Mathf.FloorToInt(
                survivalTime / 60f
            );

        int seconds =
            Mathf.FloorToInt(
                survivalTime % 60f
            );

        return string.Format(
            "{0:00}:{1:00}",
            minutes,
            seconds
        );
    }

    private void UpdateTimerUI()
    {
        if (survivalTimerText != null)
        {
            survivalTimerText.text =
                GetFormattedTime();
        }
    }
}
