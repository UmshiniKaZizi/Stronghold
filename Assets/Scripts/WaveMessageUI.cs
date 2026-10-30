using UnityEngine;
using System.Collections;

public class WaveMessageUI : MonoBehaviour
{
    [Header("Wave Messages")]
    [SerializeField] private GameObject waveIncomingMessage;
    [SerializeField] private GameObject waveCompleteMessage;

    [Header("Timing")]
    [SerializeField] private float messageDuration = 2f;

    private Coroutine currentMessage;

    private void Start()
    {
        if (waveIncomingMessage != null)
            waveIncomingMessage.SetActive(false);

        if (waveCompleteMessage != null)
            waveCompleteMessage.SetActive(false);
    }

    public void ShowWaveIncoming()
    {
        ShowMessage(waveIncomingMessage);
    }

    public void ShowWaveComplete()
    {
        ShowMessage(waveCompleteMessage);
    }

    private void ShowMessage(GameObject message)
    {
        if (message == null)
            return;

        if (currentMessage != null)
        {
            StopCoroutine(currentMessage);
        }

        if (waveIncomingMessage != null)
            waveIncomingMessage.SetActive(false);

        if (waveCompleteMessage != null)
            waveCompleteMessage.SetActive(false);

        currentMessage = StartCoroutine(
            DisplayMessage(message)
        );
    }

    private IEnumerator DisplayMessage(GameObject message)
    {
        message.SetActive(true);

        yield return new WaitForSeconds(messageDuration);

        message.SetActive(false);

        currentMessage = null;
    }
}