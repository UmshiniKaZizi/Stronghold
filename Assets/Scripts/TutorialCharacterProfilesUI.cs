using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class TutorialCharacterProfilesUI : MonoBehaviour
{
    [Header("Tutorial Character Profile Roots")]
    [SerializeField] private RectTransform assaultProfile;
    [SerializeField] private RectTransform artilleryProfile;
    [SerializeField] private RectTransform sniperProfile;

    [Header("Assault Tutorial Artwork")]
    [SerializeField] private Image assaultActiveVisual;
    [SerializeField] private Image assaultInactiveVisual;

    [Header("Artillery Tutorial Artwork")]
    [SerializeField] private Image artilleryActiveVisual;
    [SerializeField] private Image artilleryInactiveVisual;

    [Header("Sniper Tutorial Artwork")]
    [SerializeField] private Image sniperActiveVisual;
    [SerializeField] private Image sniperInactiveVisual;

    [Header("Tutorial Profile Positions")]
    [SerializeField] private RectTransform activePosition;
    [SerializeField] private RectTransform inactiveLeftPosition;
    [SerializeField] private RectTransform inactiveRightPosition;

    [Header("Animation")]
    [SerializeField] private float swapDuration = 0.3f;
    [SerializeField] private float swapLift = 20f;

    [Header("Starting Character")]
    [Tooltip("0 = Assault, 1 = Artillery, 2 = Sniper")]
    [SerializeField] private int startingCharacter = 2;

    private int currentCharacter;
    private Coroutine swapCoroutine;

    private void Start()
    {
        SetCharacterLayoutImmediate(startingCharacter);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            CycleCharacter();
        }
    }

    private void CycleCharacter()
    {
        int nextCharacter = currentCharacter + 1;

        if (nextCharacter > 2)
        {
            nextCharacter = 0;
        }

        ShowCharacter(nextCharacter);
    }

    private void ShowCharacter(int selectedCharacter)
    {
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

        currentCharacter = selectedCharacter;
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

        Vector2[] startPositions =
            new Vector2[3];

        Vector2[] targetPositions =
            new Vector2[3];

        for (int i = 0; i < profiles.Length; i++)
        {
            if (profiles[i] == null)
                continue;

            startPositions[i] =
                profiles[i].anchoredPosition;

            RectTransform target =
                GetTargetPosition(
                    i,
                    selectedCharacter
                );

            if (target != null)
            {
                targetPositions[i] =
                    target.anchoredPosition;
            }
        }

        float time = 0f;

        while (time < swapDuration)
        {
            time += Time.unscaledDeltaTime;

            float progress =
                swapDuration > 0f
                ? Mathf.Clamp01(
                    time / swapDuration
                )
                : 1f;

            float smoothProgress =
                progress * progress *
                (3f - 2f * progress);

            for (int i = 0;
                 i < profiles.Length;
                 i++)
            {
                if (profiles[i] == null)
                    continue;

                Vector2 position =
                    Vector2.Lerp(
                        startPositions[i],
                        targetPositions[i],
                        smoothProgress
                    );

                position.y +=
                    Mathf.Sin(
                        progress * Mathf.PI
                    ) * swapLift;

                profiles[i].anchoredPosition =
                    position;
            }

            yield return null;
        }

        for (int i = 0;
             i < profiles.Length;
             i++)
        {
            if (profiles[i] == null)
                continue;

            profiles[i].anchoredPosition =
                targetPositions[i];
        }

        swapCoroutine = null;
    }

    private RectTransform GetTargetPosition(
        int character,
        int selectedCharacter)
    {
        int difference =
            (character - selectedCharacter + 3)
            % 3;

        if (difference == 0)
            return activePosition;

        if (difference == 1)
            return inactiveLeftPosition;

        return inactiveRightPosition;
    }

    private RectTransform GetProfile(
        int character)
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
            activeVisual.gameObject
                .SetActive(active);
        }

        if (inactiveVisual != null)
        {
            inactiveVisual.gameObject
                .SetActive(!active);
        }
    }

    private void SetCharacterLayoutImmediate(
        int selectedCharacter)
    {
        currentCharacter =
            selectedCharacter;

        UpdateProfileArtwork(
            selectedCharacter
        );

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
        }

        RectTransform selectedProfile =
            GetProfile(selectedCharacter);

        if (selectedProfile != null)
        {
            selectedProfile.SetAsLastSibling();
        }
    }
}