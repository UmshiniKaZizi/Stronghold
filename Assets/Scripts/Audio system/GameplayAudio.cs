using UnityEngine;

public class GameplayAudio : MonoBehaviour
{
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource announcementSource;

    [Header("Gameplay Ambience")]
    [SerializeField] private AudioClip gameplayAmbience;

    [Header("Wave Announcements")]
    [SerializeField] private AudioClip waveIncomingClip;
    [SerializeField] private AudioClip waveCompleteClip;

    private void Start()
    {
        // Start the background ambience.
        if (ambienceSource != null && gameplayAmbience != null)
        {
            ambienceSource.clip = gameplayAmbience;
            ambienceSource.loop = true;
            ambienceSource.Play();
        }
    }

    public void PlayWaveIncoming()
    {
        PlayAnnouncement(waveIncomingClip);
    }

    public void PlayWaveComplete()
    {
        PlayAnnouncement(waveCompleteClip);
    }

    private void PlayAnnouncement(AudioClip clip)
    {
        if (announcementSource == null || clip == null)
            return;

        announcementSource.PlayOneShot(clip);
    }
}

