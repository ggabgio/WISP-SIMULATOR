using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("SFX Clips")]

    [Header("Audio Mixer")]
    public AudioMixerGroup sfxMixerGroup;

    public AudioClip pickUpClip;
    public AudioClip dropClip;
    public AudioClip placeClip;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = sfxMixerGroup;

        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        
        PlaySFXInternal(clip, 0);
    }
    
    private void PlaySFXInternal(AudioClip clip, int retryCount)
    {
        // Safety check to prevent infinite recursion
        if (retryCount > 2)
        {
            Debug.LogError("AudioManager: Failed to play SFX after multiple retries. AudioSource may be continuously destroyed.");
            return;
        }
        
        // Ensure AudioSource exists - check if null or destroyed and reinitialize if needed
        if (audioSource == null || !audioSource)
        {
            // Try to get existing AudioSource component first
            audioSource = gameObject.GetComponent<AudioSource>();
            
            // If still null, create a new one
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                if (sfxMixerGroup != null)
                {
                    audioSource.outputAudioMixerGroup = sfxMixerGroup;
                }
            }
        }
        
        // Try to play the sound - wrap in try-catch to handle destroyed objects
        try
        {
            if (audioSource != null && audioSource)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        catch (System.Exception e)
        {
            // AudioSource was destroyed - reinitialize it and try again
            Debug.LogWarning($"AudioManager: AudioSource was destroyed, reinitializing (retry {retryCount + 1}/3). Error: {e.Message}");
            audioSource = null; // Clear the reference
            PlaySFXInternal(clip, retryCount + 1); // Try again with incremented retry count
        }
    }

    public void PlayPickUp() => PlaySFX(pickUpClip);
    public void PlayDrop() => PlaySFX(dropClip);
    public void PlayPlace() => PlaySFX(placeClip);

    public void SetSFXVolume(float volume) // Volume controller
    {
        // Ensure audioSource exists before setting volume
        if (audioSource == null || !audioSource)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                if (sfxMixerGroup != null)
                {
                    audioSource.outputAudioMixerGroup = sfxMixerGroup;
                }
            }
        }
        
        if (audioSource != null && audioSource)
        {
            audioSource.volume = Mathf.Clamp01(volume); // ensures volume is between 0 and 1
        }
    }
}
