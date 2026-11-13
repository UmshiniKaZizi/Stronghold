
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

public class GameOverSequenceUI : MonoBehaviour
{
    [Header("Death Screen")]
    [SerializeField] private CanvasGroup deathHeaderGroup;
    [SerializeField] private TMP_Text deathHeaderText;

    [Header("Mission Report")]
    [SerializeField] private CanvasGroup reportGroup;
    [SerializeField] private TMP_Text reportText;

    [Header("Bottom Buttons")]
    [SerializeField] private CanvasGroup buttonsGroup;
    [SerializeField] private RectTransform buttonsTransform;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;

    [Header("Background Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip gameOverMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource typingAudioSource;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private AudioClip reportCompleteSound;
    [SerializeField] private AudioClip restartButtonSound;
    [SerializeField] private AudioClip exitButtonSound;

    [Header("Timing")]
    [SerializeField] private float headerFadeDuration = 1.5f;
    [SerializeField] private float headerHoldDuration = 2.5f;
    [SerializeField] private float reportFadeDuration = 1f;
    [SerializeField] private float characterDelay = 0.025f;
    [SerializeField] private int charactersPerSound = 3;
    [SerializeField] private float buttonFadeDuration = 1f;
    [SerializeField] private float buttonSoundDelay = 0.25f;

    [Header("Button Animation")]
    [SerializeField] private float buttonRiseDistance = 40f;

    private Vector2 buttonStartPosition;
    private bool sequenceStarted;
    private bool buttonActionStarted;

    private void Awake()
    {
        Debug.Log("[GAME OVER UI] Awake called.");

        if (buttonsTransform != null)
        {
            buttonStartPosition = buttonsTransform.anchoredPosition;
        }

        // Prepare the screen before the sequence starts.
        SetGroupState(deathHeaderGroup, 0f, false);
        SetGroupState(reportGroup, 0f, false);
        SetGroupState(buttonsGroup, 0f, false);

        if (deathHeaderText != null)
            deathHeaderText.text = "";

        if (reportText != null)
        {
            reportText.text = "";
            reportText.maxVisibleCharacters = 0;
        }

        // Connect button events through this script.
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);

        CheckReferences();
    }

    private void SetGroupState(
        CanvasGroup group,
        float alpha,
        bool interactive)
    {
        if (group == null)
            return;

        group.alpha = alpha;
        group.interactable = interactive;
        group.blocksRaycasts = interactive;
    }

    private void CheckReferences()
    {
        Debug.Log("[GAME OVER UI] Checking references...");

        CheckReference(deathHeaderGroup, "Death Header CanvasGroup");
        CheckReference(deathHeaderText, "Death Header TMP Text");
        CheckReference(reportGroup, "Report CanvasGroup");
        CheckReference(reportText, "Report TMP Text");
        CheckReference(buttonsGroup, "Buttons CanvasGroup");
        CheckReference(buttonsTransform, "Buttons RectTransform");
        CheckReference(restartButton, "Restart Button");
        CheckReference(exitButton, "Exit Button");
        CheckReference(musicSource, "Music AudioSource");
        CheckReference(typingAudioSource, "Sound Effects AudioSource");

        if (EventSystem.current == null)
        {
            Debug.LogError(
                "[GAME OVER UI ERROR] No EventSystem found in the scene!"
            );
        }
        else
        {
            Debug.Log("[GAME OVER UI] EventSystem found.");
        }
    }

    private void CheckReference(Object reference, string referenceName)
    {
        if (reference == null)
        {
            Debug.LogError(
                "[GAME OVER UI ERROR] Missing reference: " +
                referenceName
            );
        }
    }

    public void PlaySequence()
    {
        if (sequenceStarted)
        {
            Debug.LogWarning(
                "[GAME OVER UI] Sequence was already started. Ignoring duplicate call."
            );
            return;
        }

        sequenceStarted = true;

        Debug.Log("[GAME OVER UI] Starting Game Over sequence.");

        // Make sure the UI elements are visible in the hierarchy.
        if (deathHeaderGroup != null)
            deathHeaderGroup.gameObject.SetActive(true);

        if (reportGroup != null)
            reportGroup.gameObject.SetActive(true);

        if (buttonsGroup != null)
            buttonsGroup.gameObject.SetActive(true);

        // Reset all visual states.
        SetGroupState(deathHeaderGroup, 0f, false);
        SetGroupState(reportGroup, 0f, false);
        SetGroupState(buttonsGroup, 0f, false);

        if (reportText != null)
        {
            reportText.text = "";
            reportText.maxVisibleCharacters = 0;
        }

        if (buttonsTransform != null)
            buttonsTransform.anchoredPosition = buttonStartPosition;

        // Start Game Over music.
        StartGameOverMusic();

        // Check that the header can actually display.
        LogHeaderState();

        StartCoroutine(GameOverSequence());
    }

    private void StartGameOverMusic()
    {
        if (musicSource == null || gameOverMusic == null)
        {
            Debug.LogWarning(
                "[GAME OVER AUDIO] Music source or music clip is missing."
            );
            return;
        }

        musicSource.clip = gameOverMusic;
        musicSource.loop = true;
        musicSource.Play();

        Debug.Log("[GAME OVER AUDIO] Game Over music started.");
    }

    private void LogHeaderState()
    {
        if (deathHeaderGroup == null || deathHeaderText == null)
            return;

        Debug.Log(
            "[HEADER DEBUG] Active: " +
            deathHeaderText.gameObject.activeInHierarchy +
            " | CanvasGroup Alpha: " +
            deathHeaderGroup.alpha +
            " | Text Alpha: " +
            deathHeaderText.color.a +
            " | Text: " +
            deathHeaderText.text
        );
    }

    private IEnumerator GameOverSequence()
    {
        Debug.Log("[GAME OVER UI] STAGE 1: Showing death header.");

        if (deathHeaderText != null)
            deathHeaderText.text = "STRONGHOLDS LOST";

        yield return FadeGroup(
            deathHeaderGroup,
            0f,
            1f,
            headerFadeDuration
        );

        LogHeaderState();

        Debug.Log("[GAME OVER UI] STAGE 1 COMPLETE: Header displayed.");

        Debug.Log("[GAME OVER UI] STAGE 2: Holding death header.");

        yield return new WaitForSecondsRealtime(headerHoldDuration);

        Debug.Log("[GAME OVER UI] STAGE 3: Fading death header.");

        yield return FadeGroup(
            deathHeaderGroup,
            1f,
            0f,
            headerFadeDuration
        );

        Debug.Log("[GAME OVER UI] STAGE 3 COMPLETE: Header hidden.");

        Debug.Log("[GAME OVER UI] STAGE 4: Revealing mission report.");

        yield return FadeGroup(
            reportGroup,
            0f,
            1f,
            reportFadeDuration
        );

        Debug.Log("[GAME OVER UI] STAGE 5: Typing mission report.");

        string report = BuildMissionReport();

        yield return TypeReport(report);

        Debug.Log("[GAME OVER UI] STAGE 5 COMPLETE: Report finished.");

        Debug.Log("[GAME OVER UI] STAGE 6: Showing buttons.");

        yield return AnimateButtons();

        Debug.Log("[GAME OVER UI] SEQUENCE COMPLETE.");
    }

    private IEnumerator FadeGroup(
        CanvasGroup group,
        float startAlpha,
        float targetAlpha,
        float duration)
    {
        if (group == null)
        {
            Debug.LogError(
                "[GAME OVER UI ERROR] FadeGroup received a missing CanvasGroup."
            );
            yield break;
        }

        float elapsed = 0f;

        group.alpha = startAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            float smoothProgress =
                Mathf.SmoothStep(0f, 1f, progress);

            group.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                smoothProgress
            );

            yield return null;
        }

        group.alpha = targetAlpha;

        Debug.Log(
            "[GAME OVER UI] CanvasGroup faded to alpha: " +
            targetAlpha
        );
    }

    private string BuildMissionReport()
    {
        StringBuilder report = new StringBuilder();

        report.AppendLine("AFTER-ACTION REPORT");
        report.AppendLine("------------------------------");
        report.AppendLine();

        if (GameScoreManager.Instance != null)
        {
            report.AppendLine(
                "TOTAL SURVIVAL TIME: " +
                GameScoreManager.Instance.GetFormattedTime()
            );

            report.AppendLine();
            report.AppendLine("WAVE PERFORMANCE");
            report.AppendLine("-----------------------------------");

            Dictionary<int, int> results =
                GameScoreManager.Instance.GetWaveResults();

            List<int> waveNumbers = new List<int>(results.Keys);
            waveNumbers.Sort();

            foreach (int wave in waveNumbers)
            {
                report.AppendLine(
                    "WAVE " + wave.ToString("00") +
                    "                 " +
                    results[wave] + " KILLS"
                );
            }

            if (results.Count == 0)
                report.AppendLine("NO ENEMY ELIMINATIONS RECORDED");
        }
        else
        {
            Debug.LogError(
                "[GAME OVER UI ERROR] GameScoreManager.Instance is missing."
            );

            report.AppendLine("SCORING DATA UNAVAILABLE");
        }

        report.AppendLine();
        report.AppendLine("-----------------------------------");
        report.AppendLine("MISSION STATUS: FAILED");

        return report.ToString();
    }

    private IEnumerator TypeReport(string message)
    {
        if (reportText == null)
        {
            Debug.LogError(
                "[GAME OVER UI ERROR] Report TMP Text is missing."
            );
            yield break;
        }

        reportText.text = message;
        reportText.maxVisibleCharacters = 0;

        reportText.ForceMeshUpdate();

        int totalCharacters = reportText.textInfo.characterCount;

        if (typingAudioSource != null && typingSound != null)
        {
            typingAudioSource.clip = typingSound;
            typingAudioSource.loop = true;
            typingAudioSource.Play();
        }

        for (int i = 0; i <= totalCharacters; i++)
        {
            reportText.maxVisibleCharacters = i;

            yield return new WaitForSecondsRealtime(characterDelay);
        }

        // Stop typing sound once all characters are displayed.
        if (typingAudioSource != null)
        {
            typingAudioSource.Stop();
        }

        reportText.maxVisibleCharacters = totalCharacters;

        // Play completion sound.
        if (audioSource != null && reportCompleteSound != null)
        {
            audioSource.PlayOneShot(reportCompleteSound);
        }

        Debug.Log(
            "[REPORT DEBUG] Total characters to display: " +
            totalCharacters
        );



    }
    
        //for (int i = 0; i <= totalCharacters; i++)
       // {
       //     reportText.maxVisibleCharacters = i;

        //    if (charactersPerSound > 0 &&
         //       i > 0 &&
             //   i % charactersPerSound == 0)
           // {
           //     if (soundEffectsSource != null && typingSound != null)
          //     {
           //         soundEffectsSource.PlayOneShot(typingSound);
           //     }
           // }

          //  yield return new WaitForSecondsRealtime(characterDelay);
     //   }

     //   reportText.maxVisibleCharacters = totalCharacters;

     //   if (soundEffectsSource != null && reportCompleteSound != null)
      //  {
      //      soundEffectsSource.PlayOneShot(reportCompleteSound);
      //  }

     //   Debug.Log("[REPORT DEBUG] Typewriter finished.");
   // }

    private IEnumerator AnimateButtons()
    {
        if (buttonsGroup == null || buttonsTransform == null)
        {
            Debug.LogError(
                "[GAME OVER UI ERROR] Button CanvasGroup or RectTransform is missing."
            );
            yield break;
        }

        buttonsGroup.alpha = 0f;
        buttonsGroup.interactable = false;
        buttonsGroup.blocksRaycasts = false;

        Vector2 startPosition =
            buttonStartPosition -
            Vector2.up * buttonRiseDistance;

        buttonsTransform.anchoredPosition = startPosition;

        float elapsed = 0f;

        while (elapsed < buttonFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsed / buttonFadeDuration
            );

            float smoothProgress =
                Mathf.SmoothStep(0f, 1f, progress);

            buttonsGroup.alpha = smoothProgress;

            buttonsTransform.anchoredPosition = Vector2.Lerp(
                startPosition,
                buttonStartPosition,
                smoothProgress
            );

            yield return null;
        }

        buttonsGroup.alpha = 1f;
        buttonsTransform.anchoredPosition = buttonStartPosition;

        buttonsGroup.interactable = true;
        buttonsGroup.blocksRaycasts = true;

        // Explicitly enable both buttons.
        if (restartButton != null)
            restartButton.interactable = true;

        if (exitButton != null)
            exitButton.interactable = true;

        Debug.Log(
            "[BUTTON DEBUG] Buttons enabled. " +
            "CanvasGroup Alpha: " + buttonsGroup.alpha +
            " | Interactable: " + buttonsGroup.interactable +
            " | Blocks Raycasts: " + buttonsGroup.blocksRaycasts +
            " | Restart Button: " +
            (restartButton != null && restartButton.interactable) +
            " | Exit Button: " +
            (exitButton != null && exitButton.interactable)
        );

        yield return null;
    }

    // Called when Restart is clicked.
    private void OnRestartClicked()
    {
        if (buttonActionStarted)
            return;

        buttonActionStarted = true;

        Debug.Log("[BUTTON DEBUG] RESTART BUTTON CLICKED.");

        buttonsGroup.interactable = false;
        buttonsGroup.blocksRaycasts = false;

        if (typingAudioSource != null && restartButtonSound != null)
            typingAudioSource.PlayOneShot(restartButtonSound);

        StartCoroutine(ExecuteRestart());
    }

    private IEnumerator ExecuteRestart()
    {
        yield return new WaitForSecondsRealtime(buttonSoundDelay);

        if (GameManager.Instance != null)
        {
            Debug.Log("[BUTTON DEBUG] Calling GameManager.RestartGame().");
            GameManager.Instance.RestartGame();
        }
        else
        {
            Debug.LogError(
                "[BUTTON DEBUG ERROR] GameManager.Instance is missing."
            );
        }
    }

    // Called when Exit is clicked.
    private void OnExitClicked()
    {
        if (buttonActionStarted)
            return;

        buttonActionStarted = true;

        Debug.Log("[BUTTON DEBUG] EXIT BUTTON CLICKED.");

        buttonsGroup.interactable = false;
        buttonsGroup.blocksRaycasts = false;

        if (typingAudioSource != null && exitButtonSound != null)
            typingAudioSource.PlayOneShot(exitButtonSound);

        StartCoroutine(ExecuteExit());
    }

    private IEnumerator ExecuteExit()
    {
        yield return new WaitForSecondsRealtime(buttonSoundDelay);

        if (GameManager.Instance != null)
        {
            Debug.Log("[BUTTON DEBUG] Calling GameManager.ExitGame().");
            GameManager.Instance.ExitGame();
        }
        else
        {
            Debug.LogError(
                "[BUTTON DEBUG ERROR] GameManager.Instance is missing."
            );
        }

        Debug.Log("Exiting Stronghold...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnDestroy()
    {
        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestartClicked);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(OnExitClicked);
    }
}
