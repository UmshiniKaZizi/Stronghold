using UnityEngine;

public class OnboardingUIManager : MonoBehaviour
{
    [Header("Onboarding Steps")]
    [SerializeField] private GameObject missionPanel;

    [SerializeField]
    private GameObject strongholdExplanation;

    [SerializeField]
    private TypewriterTextUI strongholdTypewriter;

    [SerializeField]
    private GameObject tutorialCharacterPanel;

    [Header("Buttons")]
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject startButton;

    [Header("Gameplay UI")]
    [SerializeField]
    private GameObject gameplayProfilesVisuals;

    [SerializeField]
    private GameObject strongholdGameplayHUD;

    private int currentStep = 0;

    private void Start()
    {
        ShowStep(0);

        if (gameplayProfilesVisuals != null)
        {
            gameplayProfilesVisuals.SetActive(false);
        }
    }

    public void NextStep()
    {
        currentStep++;

        if (currentStep > 2)
        {
            currentStep = 2;
        }

        ShowStep(currentStep);
    }

    private void ShowStep(int step)
    {
        if (missionPanel != null)
        {
            missionPanel.SetActive(
                step == 0
            );
        }

        // STEP 2:
        // Stronghold explanation text.
        if (strongholdExplanation != null)
        {
            strongholdExplanation.SetActive(
                step == 1
            );
        }

        // Show actual stronghold HUD
        // during its explanation.
        if (strongholdGameplayHUD != null)
        {
            strongholdGameplayHUD.SetActive(
                step >= 1
            );
        }

        if (step == 1 &&
            strongholdTypewriter != null)
        {
            strongholdTypewriter.PlayText();
        }

        if (tutorialCharacterPanel != null)
        {
            tutorialCharacterPanel.SetActive(
                step == 2
            );
        }

        if (nextButton != null)
        {
            nextButton.SetActive(
                step < 2
            );
        }

        if (startButton != null)
        {
            startButton.SetActive(
                step == 2
            );
        }
    }

    public void StartGameplay()
    {
        if (missionPanel != null)
            missionPanel.SetActive(false);

        if (strongholdExplanation != null)
            strongholdExplanation.SetActive(false);

        if (tutorialCharacterPanel != null)
            tutorialCharacterPanel.SetActive(false);

        if (nextButton != null)
            nextButton.SetActive(false);

        if (startButton != null)
            startButton.SetActive(false);

        if (gameplayProfilesVisuals != null)
        {
            gameplayProfilesVisuals.SetActive(true);
        }

        // Stronghold HUD stays visible
        // during gameplay.
        if (strongholdGameplayHUD != null)
        {
            strongholdGameplayHUD.SetActive(true);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }
}