using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterWheelUI : MonoBehaviour
{
    [Header("Character Slots")]
    [SerializeField] private Image assaultSlot;
    [SerializeField] private Image artillerySlot;
    [SerializeField] private Image sniperSlot;

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



    public void UpdateCharacterSelection(int selectedCharacter)
    {
        StartCoroutine(
            AnimateSlot(
                assaultSlot,
                selectedCharacter == 0
            )
        );

        StartCoroutine(
            AnimateSlot(
                artillerySlot,
                selectedCharacter == 1
            )
        );

        StartCoroutine(
            AnimateSlot(
                sniperSlot,
                selectedCharacter == 2
            )
        );

        PlaySelectionSound(selectedCharacter);


        // Reset all character slots
        assaultSlot.color = normalColor;
        artillerySlot.color = normalColor;
        sniperSlot.color = normalColor;

        // Highlight the currently selected character
        switch (selectedCharacter)
        {
            case 0:
                assaultSlot.color = selectedColor;
                break;

            case 1:
                artillerySlot.color = selectedColor;
                break;

            case 2:
                sniperSlot.color = selectedColor;
                break;
        }
    }

    private void PlaySelectionSound(int selectedCharacter)
    {
        if (audioSource == null)
            return;

        switch (selectedCharacter)
        {
            case 0:
                audioSource.PlayOneShot(assaultSound);
                break;

            case 1:
                audioSource.PlayOneShot(artillerySound);
                break;

            case 2:
                audioSource.PlayOneShot(sniperSound);
                break;
        }
    }

    private IEnumerator AnimateSlot(Image slot, bool selected)
    {
        Transform slotTransform = slot.transform;

        // Change colour
        slot.color = selected
            ? selectedColor
            : normalColor;

        if (!selected)
        {
            // Return inactive characters to normal size
            slotTransform.localScale =
                Vector3.one * normalScale;

            yield break;
        }

        // Selected character starts slightly smaller
        slotTransform.localScale =
            Vector3.one * normalScale;

        // Grow
        yield return ScaleTo(
            slotTransform,
            1.25f,
            0.08f
        );

        // Shrink slightly
        yield return ScaleTo(
            slotTransform,
            1.12f,
            0.06f
        );

        // Settle
        yield return ScaleTo(
            slotTransform,
            selectedScale,
            0.08f
        );
    }

    private IEnumerator ScaleTo(
        Transform target,
        float targetScale,
        float duration
    )
    {
        Vector3 startScale = target.localScale;

        Vector3 endScale =
            Vector3.one * targetScale;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float progress =
                Mathf.Clamp01(time / duration);

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
