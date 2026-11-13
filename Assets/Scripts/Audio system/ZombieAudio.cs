using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]


public class ZombieAudio : MonoBehaviour
{
    
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Zombie Growling")]
    [SerializeField] private AudioClip[] growlClips;
    [SerializeField] private float minGrowlInterval = 4f;
    [SerializeField] private float maxGrowlInterval = 9f;

    [Header("Zombie Hit")]
    [SerializeField] private AudioClip[] hitClips;

    [Header("Zombie Death")]
    [SerializeField] private AudioClip[] deathClips;

    private bool isDead;
    private Coroutine growlCoroutine;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        growlCoroutine = StartCoroutine(GrowlRoutine());
    }

    // ==========================================
    // RANDOM ZOMBIE GROWLING
    // ==========================================

    private IEnumerator GrowlRoutine()
    {
        while (!isDead)
        {
            float waitTime = Random.Range(
                minGrowlInterval,
                maxGrowlInterval
            );

            yield return new WaitForSeconds(waitTime);

            if (isDead)
                yield break;

            PlayRandomClip(growlClips);
        }
    }

    // ==========================================
    // ZOMBIE HIT SOUND
    // ==========================================

    public void PlayHitSound()
    {
        if (isDead)
            return;

        PlayRandomClip(hitClips);
    }

    // ==========================================
    // ZOMBIE DEATH SOUND
    // ==========================================

    public void PlayDeathSound()
    {
        if (isDead)
            return;

        isDead = true;

        if (growlCoroutine != null)
        {
            StopCoroutine(growlCoroutine);
        }

        // Stop any existing growling or hit sound.
        audioSource.Stop();

        AudioClip deathClip = GetRandomClip(deathClips);

        if (deathClip != null)
        {
            audioSource.PlayOneShot(deathClip);
        }
    }

    // ==========================================
    // AUDIO HELPERS
    // ==========================================

    private void PlayRandomClip(AudioClip[] clips)
    {
        if (isDead || clips == null || clips.Length == 0)
            return;

        AudioClip clip = GetRandomClip(clips);

        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return null;

        return clips[Random.Range(0, clips.Length)];
    }

    private void OnDisable()
    {
        if (growlCoroutine != null)
            StopCoroutine(growlCoroutine);
    }
}


