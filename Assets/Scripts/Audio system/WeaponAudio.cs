using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class WeaponAudio : MonoBehaviour
{
    
    [Header("Shooting Audio")]
    [SerializeField] private AudioClip[] shootingClips;

    [Header("Audio Settings")]
    [SerializeField, Range(0f, 1f)]
    private float volume = 0.8f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayShootSound()
    {
        if (shootingClips == null || shootingClips.Length == 0)
            return;

        AudioClip clip = shootingClips[
            Random.Range(0, shootingClips.Length)
        ];

        if (clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}
