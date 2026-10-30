using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterWheelUI : MonoBehaviour
{
    [Header("Character Profile Roots")]
    [SerializeField] private RectTransform assaultProfile;
    [SerializeField] private RectTransform artilleryProfile;
    [SerializeField] private RectTransform sniperProfile;

    [Header("Assault Profile Artwork")]
    [SerializeField] private Image assaultActiveVisual;
    [SerializeField] private Image assaultInactiveVisual;

    [Header("Artillery Profile Artwork")]
    [SerializeField] private Image artilleryActiveVisual;
    [SerializeField] private Image artilleryInactiveVisual;

    [Header("Sniper Profile Artwork")]
    [SerializeField] private Image sniperActiveVisual;
    [SerializeField] private Image sniperInactiveVisual;

    [Header("Profile Positions")]
    [SerializeField] private RectTransform activePosition;
    [SerializeField] private RectTransform inactiveLeftPosition;
    [SerializeField] private RectTransform inactiveRightPosition;

    [Header("Profile Swap Animation")]
    [SerializeField] private float activeScale = 1f;
    [SerializeField] private float inactiveScale = 0.85f;
    [SerializeField] private float swapDuration = 0.3f;
    [SerializeField] private float swapLift = 20f;

    [Header("Starting Character")]
    [Tooltip("0 = Assault, 1 = Artillery, 2 = Sniper")]
    [SerializeField] private int startingCharacter = 2;

    [Header("Dead Character Appearance")]
    [SerializeField]
    private Color deadProfileColor =
        new Color(0.35f, 0.35f, 0.35f, 1f);

    [Header("Health Bar Fills")]
    [SerializeField] private Image assaultActiveHealthFill;
    [SerializeField] private Image assaultInactiveHealthFill;

    [SerializeField] private Image artilleryActiveHealthFill;
    [SerializeField] private Image artilleryInactiveHealthFill;

    [SerializeField] private Image sniperActiveHealthFill;
    [SerializeField] private Image sniperInactiveHealthFill;

    [Header("Player Health References")]
    [SerializeField] private PlayerHealth assaultHealth;
    [SerializeField] private PlayerHealth artilleryHealth;
    [SerializeField] private PlayerHealth sniperHealth;

    [Header("Selection Sounds")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip assaultSound;
    [SerializeField] private AudioClip artillerySound;
    [SerializeField] private AudioClip sniperSound;

    private int currentSelectedCharacter = -1;
    private Coroutine swapCoroutine;

    private void Start()
    {
        UpdateAllHealthBars();
        SetCharacterLayoutImmediate(startingCharacter);
        UpdateDeadProfileVisuals();
    }

    private void Update()
    {
        UpdateAllHealthBars();
        UpdateDeadProfileVisuals();
    }

    // Called by CharacterSwap whenever the active character changes.
    public void UpdateCharacterSelection(int selectedCharacter)
    {
        if (selectedCharacter < 0 || selectedCharacter > 2)
            return;

        if (selectedCharacter == currentSelectedCharacter)
            return;

        UpdateProfileArtwork(selectedCharacter);

        RectTransform selectedProfile =
            GetProfile(selectedCharacter);

        if (selectedProfile != null)
        {
            selectedProfile.SetAsLastSibling();
        }

        if (swapCoroutine != null)
        {
            StopCoroutine(swapCoroutine);
        }

        swapCoroutine = StartCoroutine(
            AnimateCharacterSwap(selectedCharacter)
        );

        PlaySelectionSound(selectedCharacter);

        currentSelectedCharacter = selectedCharacter;
    }

    private IEnumerator AnimateCharacterSwap(
        int selectedCharacter)
    {
        RectTransform[] profiles =
        {
            assaultProfile,
            artilleryProfile,
            sniperProfile
        };

        Vector2[] startPositions = new Vector2[3];
        Vector2[] targetPositions = new Vector2[3];

        Vector3[] startScales = new Vector3[3];
        Vector3[] targetScales = new Vector3[3];

        for (int i = 0; i < profiles.Length; i++)
        {
            RectTransform profile = profiles[i];

            if (profile == null)
                continue;

            startPositions[i] =
                profile.anchoredPosition;

            startScales[i] =
                profile.localScale;

            RectTransform target =
                GetTargetPosition(i, selectedCharacter);

            if (target != null)
            {
                targetPositions[i] =
                    target.anchoredPosition;
            }

            float scale =
                i == selectedCharacter
                ? activeScale
                : inactiveScale;

            targetScales[i] =
                Vector3.one * scale;
        }

        float time = 0f;

        while (time < swapDuration)
        {
            time += Time.unscaledDeltaTime;

            float progress =
                swapDuration > 0f
                ? Mathf.Clamp01(time / swapDuration)
                : 1f;

            float smoothProgress =
                progress * progress *
                (3f - 2f * progress);

            for (int i = 0; i < profiles.Length; i++)
            {
                RectTransform profile = profiles[i];

                if (profile == null)
                    continue;

                Vector2 position = Vector2.Lerp(
                    startPositions[i],
                    targetPositions[i],
                    smoothProgress
                );

                position.y +=
                    Mathf.Sin(progress * Mathf.PI)
                    * swapLift;

                profile.anchoredPosition = position;

                profile.localScale = Vector3.Lerp(
                    startScales[i],
                    targetScales[i],
                    smoothProgress
                );
            }

            yield return null;
        }

        for (int i = 0; i < profiles.Length; i++)
        {
            RectTransform profile = profiles[i];

            if (profile == null)
                continue;

            profile.anchoredPosition =
                targetPositions[i];

            profile.localScale =
                targetScales[i];
        }

        swapCoroutine = null;
    }

    private RectTransform GetTargetPosition(
        int character,
        int selectedCharacter)
    {
        // 0 = Assault
        // 1 = Artillery
        // 2 = Sniper

        int difference =
            (character - selectedCharacter + 3) % 3;

        if (difference == 0)
            return activePosition;

        if (difference == 1)
            return inactiveLeftPosition;

        return inactiveRightPosition;
    }

    private RectTransform GetProfile(int character)
    {
        switch (character)
        {
            case 0:
                return assaultProfile;

            case 1:
                return artilleryProfile;

            case 2:
                return sniperProfile;
        }

        return null;
    }

    private void UpdateProfileArtwork(
        int selectedCharacter)
    {
        SetProfileArtwork(
            assaultActiveVisual,
            assaultInactiveVisual,
            selectedCharacter == 0
        );

        SetProfileArtwork(
            artilleryActiveVisual,
            artilleryInactiveVisual,
            selectedCharacter == 1
        );

        SetProfileArtwork(
            sniperActiveVisual,
            sniperInactiveVisual,
            selectedCharacter == 2
        );
    }

    private void SetProfileArtwork(
        Image activeVisual,
        Image inactiveVisual,
        bool active)
    {
        if (activeVisual != null)
        {
            activeVisual.gameObject.SetActive(active);
        }

        if (inactiveVisual != null)
        {
            inactiveVisual.gameObject.SetActive(!active);
        }
    }

    private void SetCharacterLayoutImmediate(
        int selectedCharacter)
    {
        currentSelectedCharacter =
            selectedCharacter;

        UpdateProfileArtwork(selectedCharacter);

        for (int i = 0; i < 3; i++)
        {
            RectTransform profile =
                GetProfile(i);

            RectTransform target =
                GetTargetPosition(
                    i,
                    selectedCharacter
                );

            if (profile == null ||
                target == null)
                continue;

            profile.anchoredPosition =
                target.anchoredPosition;

            float scale =
                i == selectedCharacter
                ? activeScale
                : inactiveScale;

            profile.localScale =
                Vector3.one * scale;
        }

        RectTransform selectedProfile =
            GetProfile(selectedCharacter);

        if (selectedProfile != null)
        {
            selectedProfile.SetAsLastSibling();
        }
    }

    // -----------------------------
    // DEAD CHARACTER APPEARANCE
    // -----------------------------

    private void UpdateDeadProfileVisuals()
    {
        UpdateDeadProfile(
            assaultHealth,
            assaultActiveVisual,
            assaultInactiveVisual
        );

        UpdateDeadProfile(
            artilleryHealth,
            artilleryActiveVisual,
            artilleryInactiveVisual
        );

        UpdateDeadProfile(
            sniperHealth,
            sniperActiveVisual,
            sniperInactiveVisual
        );
    }

    private void UpdateDeadProfile(
        PlayerHealth playerHealth,
        Image activeVisual,
        Image inactiveVisual)
    {
        if (playerHealth == null)
            return;

        Color profileColor =
            playerHealth.IsDead
            ? deadProfileColor
            : Color.white;

        if (activeVisual != null)
        {
            activeVisual.color =
                profileColor;
        }

        if (inactiveVisual != null)
        {
            inactiveVisual.color =
                profileColor;
        }
    }

    // -----------------------------
    // HEALTH BARS
    // -----------------------------

    private void UpdateAllHealthBars()
    {
        UpdateHealthBars(
            assaultHealth,
            assaultActiveHealthFill,
            assaultInactiveHealthFill
        );

        UpdateHealthBars(
            artilleryHealth,
            artilleryActiveHealthFill,
            artilleryInactiveHealthFill
        );

        UpdateHealthBars(
            sniperHealth,
            sniperActiveHealthFill,
            sniperInactiveHealthFill
        );
    }

    private void UpdateHealthBars(
        PlayerHealth playerHealth,
        Image activeHealthFill,
        Image inactiveHealthFill)
    {
        if (playerHealth == null)
            return;

        float healthPercent =
            playerHealth.MaxHealth > 0f
            ? playerHealth.CurrentHealth
              / playerHealth.MaxHealth
            : 0f;

        healthPercent =
            Mathf.Clamp01(healthPercent);

        if (playerHealth.IsDead)
        {
            healthPercent = 0f;
        }

        if (activeHealthFill != null)
        {
            activeHealthFill.fillAmount =
                healthPercent;
        }

        if (inactiveHealthFill != null)
        {
            inactiveHealthFill.fillAmount =
                healthPercent;
        }
    }

    // -----------------------------
    // SOUND SYSTEM
    // -----------------------------

    private void PlaySelectionSound(
        int selectedCharacter)
    {
        if (audioSource == null)
            return;

        switch (selectedCharacter)
        {
            case 0:
                if (assaultSound != null)
                {
                    audioSource.PlayOneShot(
                        assaultSound
                    );
                }
                break;

            case 1:
                if (artillerySound != null)
                {
                    audioSource.PlayOneShot(
                        artillerySound
                    );
                }
                break;

            case 2:
                if (sniperSound != null)
                {
                    audioSource.PlayOneShot(
                        sniperSound
                    );
                }
                break;
        }
    }
}