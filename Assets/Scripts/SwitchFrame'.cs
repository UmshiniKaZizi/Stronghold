using UnityEngine;
using System.Collections;

public class SwitchFrameUI : MonoBehaviour
{
    [Header("Character Frames")]
    [SerializeField] private GameObject assaultFrame;
    [SerializeField] private GameObject artilleryFrame;
    [SerializeField] private GameObject sniperFrame;

    [Header("Flash Settings")]
    [SerializeField] private int flashCount = 3;
    [SerializeField] private float flashOnTime = 0.12f;
    [SerializeField] private float flashOffTime = 0.08f;

    private Coroutine flashCoroutine;

    private void Start()
    {
        HideAllFrames();
    }

    public void FlashCharacterFrame(int characterIndex)
    {
        GameObject frame = GetFrame(characterIndex);

        if (frame == null)
            return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        HideAllFrames();

        flashCoroutine = StartCoroutine(
            FlashFrame(frame)
        );
    }

    private IEnumerator FlashFrame(GameObject frame)
    {
        for (int i = 0; i < flashCount; i++)
        {
            frame.SetActive(true);

            yield return new WaitForSecondsRealtime(
                flashOnTime
            );

            frame.SetActive(false);

            yield return new WaitForSecondsRealtime(
                flashOffTime
            );
        }

        frame.SetActive(false);

        flashCoroutine = null;
    }

    private GameObject GetFrame(int characterIndex)
    {
        switch (characterIndex)
        {
            case 0:
                return assaultFrame;

            case 1:
                return artilleryFrame;

            case 2:
                return sniperFrame;
        }

        return null;
    }

    private void HideAllFrames()
    {
        if (assaultFrame != null)
            assaultFrame.SetActive(false);

        if (artilleryFrame != null)
            artilleryFrame.SetActive(false);

        if (sniperFrame != null)
            sniperFrame.SetActive(false);
    }
}