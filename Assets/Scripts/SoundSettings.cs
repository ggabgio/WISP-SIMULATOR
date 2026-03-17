using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundSettings : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer masterMixer;

    [Header("Exposed Parameter Names (must match AudioMixer)")]
    [SerializeField] private string musicParam = "MusicVolume";
    [SerializeField] private string sfxParam = "SFXVolume";

    // PlayerPrefs keys
    private const string MUSIC_PREF = "SavedMusicVolume";
    private const string SFX_PREF = "SavedSFXVolume";

    private void Start()
    {
        // Load saved values (default 100)
        float savedMusic = PlayerPrefs.GetFloat(MUSIC_PREF, 100f);
        float savedSfx = PlayerPrefs.GetFloat(SFX_PREF, 100f);

        // Apply them (this also updates sliders and mixer)
        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSfx);

        // Optional: make sliders call the setter in the editor via OnValueChanged
        // (if you prefer wiring in script, uncomment below)
        // musicSlider.onValueChanged.AddListener(SetMusicVolume);
        // sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    // Public - hook these to the slider OnValueChanged(float)
    public void SetMusicVolume(float value) => ApplyVolume(value, MUSIC_PREF, musicSlider, musicParam, "Music");
    public void SetSFXVolume(float value) => ApplyVolume(value, SFX_PREF, sfxSlider, sfxParam, "SFX");

    // Helper that does the conversion, clamping, PlayerPrefs and calls mixer
    private void ApplyVolume(float value, string prefKey, Slider slider, string paramName, string debugTag)
    {
        // Accept 0..100 from slider. Avoid log10(0) by clamping minimal non-zero.
        float clamped = Mathf.Clamp(value, 0f, 100f);

        // Update slider visually (if called from code)
        if (slider != null && slider.value != clamped)
            slider.value = clamped;

        // Save preference
        PlayerPrefs.SetFloat(prefKey, clamped);
        PlayerPrefs.Save();

        // Convert 0..100 to decibels for mixer (0 => -80dB (mute), 100 => 0dB)
        float mixerDb;
        if (clamped <= 0.001f) // treat as mute
        {
            mixerDb = -80f; // a safe "mute" dB value
        }
        else
        {
            // Convert percentage to linear (0..1), then to dB
            float linear = clamped / 100f;
            mixerDb = Mathf.Log10(linear) * 20f;
        }

        // Apply to mixer - make sure 'paramName' matches the exposed parameter
        bool ok = masterMixer.SetFloat(paramName, mixerDb);
        Debug.Log($"[{debugTag}] value={clamped} -> dB={mixerDb} applied to '{paramName}' (SetFloat result: {ok})");
    }

    // Convenience methods if you want to call slider input methods specifically:
    public void SetMusicVolumeFromSlider() { if (musicSlider != null) SetMusicVolume(musicSlider.value); }
    public void SetSFXVolumeFromSlider() { if (sfxSlider != null) SetSFXVolume(sfxSlider.value); }
}
