
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterWheelUI : MonoBehaviour
{
    [Header("Character Slots")]
    [SerializeField] private Image assaultSlot;
    [SerializeField] private Image artillerySlot;
    [SerializeField] private Image sniperSlot;

    [Header("Health Bar Fills")]
    [SerializeField] private Image assaultHealthFill;
    [SerializeField] private Image artilleryHealthFill;
    [SerializeField] private Image sniperHealthFill;

    [Header("Player Health References")]
    [SerializeField] private PlayerHealth assaultHealth;
    [SerializeField] private PlayerHealth artilleryHealth;
    [SerializeField] private PlayerHealth sniperHealth;

    [Header("Selection")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    [Header("Animation")]
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float animationSpeed = 8f;

    [Header("Selection Sounds")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip assaultSound;
    [SerializeField] private AudioClip artillerySound;
    [SerializeField] private AudioClip sniperSound;

    private void Start()
    {
        UpdateAllHealthBars();
    }

    private void Update()
    {
        UpdateAllHealthBars();
    }

    public void UpdateCharacterSelection(int selectedCharacter)
    {
        StartCoroutine(AnimateSlot(assaultSlot, selectedCharacter == 0));
        StartCoroutine(AnimateSlot(artillerySlot, selectedCharacter == 1));
        StartCoroutine(AnimateSlot(sniperSlot, selectedCharacter == 2));

        PlaySelectionSound(selectedCharacter);

        if (assaultSlot != null)
            assaultSlot.color = selectedCharacter == 0
                ? selectedColor : normalColor;

        if (artillerySlot != null)
            artillerySlot.color = selectedCharacter == 1
                ? selectedColor : normalColor;

        if (sniperSlot != null)
            sniperSlot.color = selectedCharacter == 2
                ? selectedColor : normalColor;
    }

    private void UpdateAllHealthBars()
    {
        UpdateHealthBar(assaultHealth, assaultHealthFill);
        UpdateHealthBar(artilleryHealth, artilleryHealthFill);
        UpdateHealthBar(sniperHealth, sniperHealthFill);
    }

    private void UpdateHealthBar(
        PlayerHealth playerHealth,
        Image healthFill)
    {
        if (playerHealth == null || healthFill == null)
            return;

        float healthPercent = playerHealth.MaxHealth > 0f
            ? playerHealth.CurrentHealth / playerHealth.MaxHealth
            : 0f;

        healthFill.fillAmount = Mathf.Clamp01(healthPercent);

        // Dead characters have empty health bars.
        if (playerHealth.IsDead)
            healthFill.fillAmount = 0f;
    }

    private void PlaySelectionSound(int selectedCharacter)
    {
        if (audioSource == null)
            return;

        switch (selectedCharacter)
        {
            case 0:
                if (assaultSound != null)
                    audioSource.PlayOneShot(assaultSound);
                break;

            case 1:
                if (artillerySound != null)
                    audioSource.PlayOneShot(artillerySound);
                break;

            case 2:
                if (sniperSound != null)
                    audioSource.PlayOneShot(sniperSound);
                break;
        }
    }

    private IEnumerator AnimateSlot(Image slot, bool selected)
    {
        if (slot == null)
            yield break;

        Transform slotTransform = slot.transform;

        if (!selected)
        {
            slotTransform.localScale = Vector3.one * normalScale;
            yield break;
        }

        slotTransform.localScale = Vector3.one * normalScale;

        yield return ScaleTo(slotTransform, 1.25f, 0.08f);
        yield return ScaleTo(slotTransform, 1.12f, 0.06f);
        yield return ScaleTo(slotTransform, selectedScale, 0.08f);
    }

    private IEnumerator ScaleTo(
        Transform target,
        float targetScale,
        float duration)
    {
        Vector3 startScale = target.localScale;
        Vector3 endScale = Vector3.one * targetScale;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float progress = duration > 0f
                ? Mathf.Clamp01(time / duration)
                : 1f;

            target.localScale = Vector3.Lerp(
                startScale,
                endScale,
                progress
            );

            yield return null;
        }

        target.localScale = endScale;
    }
}
