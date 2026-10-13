using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class StrongholdAudio : MonoBehaviour
{
    [Header("Alert Sounds")]
   // [SerializeField] private AudioClip threatenedSound;
    [SerializeField] private AudioClip criticalBeep;

    [Header("Critical Alert Settings")]
    [SerializeField, Min(0.2f)]
    private float beepInterval = 1.2f;

    private AudioSource audioSource;
    private Coroutine criticalRoutine;
    private StrongholdStatus previousStatus;
    private bool hasPreviousStatus;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // HUD alerts should not come from a 3D position.
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    // Called whenever the HUD receives a stronghold status.
    public void UpdateAlert(StrongholdStatus newStatus)
    {
        // Ignore repeated updates for the same status.
        if (hasPreviousStatus && newStatus == previousStatus)
            return;

        previousStatus = newStatus;
        hasPreviousStatus = true;

        // Stop any alert belonging to the previous state.
        StopCriticalAlert();
        audioSource.Stop();

        switch (newStatus)
        {
            case StrongholdStatus.Secure:
                // No sound while secure.
                break;

            //case StrongholdStatus.Threatened:
                // Play a warning cue once when entering this state.
                //if (threatenedSound != null)
                //    audioSource.PlayOneShot(threatenedSound);
                //break;

            case StrongholdStatus.Critical:
                // Repeat the beep while the stronghold remains critical.
                if (criticalBeep != null)
                {
                    criticalRoutine =
                        StartCoroutine(CriticalBeepRoutine());
                }
                break;

            case StrongholdStatus.Destroyed:
                // Stop alerts. No repeating sound when destroyed.
                break;
        }
    }

    private IEnumerator CriticalBeepRoutine()
    {
        while (true)
        {
            audioSource.PlayOneShot(criticalBeep);
            yield return new WaitForSeconds(beepInterval);
        }
    }

    private void StopCriticalAlert()
    {
        if (criticalRoutine != null)
        {
            StopCoroutine(criticalRoutine);
            criticalRoutine = null;
        }
    }

    private void OnDisable()
    {
        StopCriticalAlert();

        if (audioSource != null)
            audioSource.Stop();
    }
}

