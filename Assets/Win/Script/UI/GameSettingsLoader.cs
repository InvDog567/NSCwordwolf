using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
 
// Attach this to your Settings Prefab root GameObject.
// It loads and applies all settings in any scene — no StartManager needed.
public class GameSettingsLoader : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
 
    [Header("Audio Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeLabel;
 
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TextMeshProUGUI musicVolumeLabel;
 
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI sfxVolumeLabel;
 
    [SerializeField] private Slider dialogVolumeSlider;
    [SerializeField] private TextMeshProUGUI dialogVolumeLabel;
 
    [Header("Controls Sliders")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityLabel;
 
    // ========== SENSITIVITY REMAP ==========
    // Slider 0-1 maps to real sensitivity 0.1-5.0
    
    
 
    private void Update()
    {
        // Intercept Esc while settings is open — closes settings without touching pause menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
        }
    }
 
    private void OnEnable()
    {
        // Reload every time the panel is opened so values are always fresh
        GameSettings.LoadFromPrefs();
        ApplyAudioMixer();
        LoadIntoUI();
        HookUpListeners();
    }
 
    private void OnDisable()
    {
        RemoveListeners();
    }
 
    // ========== AUDIO MIXER ==========
 
    private float ToDecibels(float value)
    {
        return value > 0.001f ? Mathf.Log10(value) * 20f : -80f;
    }
 
    private void ApplyAudioMixer()
    {
        if (audioMixer == null) return;
        audioMixer.SetFloat("MusicVolume", ToDecibels(GameSettings.MusicVolume));
        audioMixer.SetFloat("SFXVolume",   ToDecibels(GameSettings.SFXVolume));
        AudioListener.volume = GameSettings.MasterVolume;
    }
 
    // ========== LOAD INTO UI ==========
 
    private void LoadIntoUI()
    {
        if (masterVolumeSlider != null)  masterVolumeSlider.value = GameSettings.MasterVolume;
        if (masterVolumeLabel != null)   masterVolumeLabel.text = $"Master Volume: {GameSettings.MasterVolume * 100f:F0}%";
 
        if (musicVolumeSlider != null)   musicVolumeSlider.value = GameSettings.MusicVolume;
        if (musicVolumeLabel != null)    musicVolumeLabel.text = $"Music Volume: {GameSettings.MusicVolume * 100f:F0}%";
 
        if (sfxVolumeSlider != null)     sfxVolumeSlider.value = GameSettings.SFXVolume;
        if (sfxVolumeLabel != null)      sfxVolumeLabel.text = $"SFX Volume: {GameSettings.SFXVolume * 100f:F0}%";
 
        if (dialogVolumeSlider != null)  dialogVolumeSlider.value = GameSettings.DialogVolume;
        if (dialogVolumeLabel != null)   dialogVolumeLabel.text = $"Dialog Volume: {GameSettings.DialogVolume * 100f:F0}%";
 
        if (sensitivitySlider != null)   sensitivitySlider.value = GameSettings.SensitivityX;
        if (sensitivityLabel != null)    sensitivityLabel.text = $"Camera Sensitivity: {GameSettings.SensitivityX:F1}";
    }
 
    // ========== LISTENERS ==========
 
    private void HookUpListeners()
    {
        if (masterVolumeSlider != null)  masterVolumeSlider.onValueChanged.AddListener(ChangeMasterVolume);
        if (musicVolumeSlider != null)   musicVolumeSlider.onValueChanged.AddListener(ChangeMusicVolume);
        if (sfxVolumeSlider != null)     sfxVolumeSlider.onValueChanged.AddListener(ChangeSFXVolume);
        if (dialogVolumeSlider != null)  dialogVolumeSlider.onValueChanged.AddListener(UpdateDialogLabel);
        if (sensitivitySlider != null)   sensitivitySlider.onValueChanged.AddListener(ChangeSensitivity);
    }
 
    private void RemoveListeners()
    {
        if (masterVolumeSlider != null)  masterVolumeSlider.onValueChanged.RemoveListener(ChangeMasterVolume);
        if (musicVolumeSlider != null)   musicVolumeSlider.onValueChanged.RemoveListener(ChangeMusicVolume);
        if (sfxVolumeSlider != null)     sfxVolumeSlider.onValueChanged.RemoveListener(ChangeSFXVolume);
        if (dialogVolumeSlider != null)  dialogVolumeSlider.onValueChanged.RemoveListener(UpdateDialogLabel);
        if (sensitivitySlider != null)   sensitivitySlider.onValueChanged.RemoveListener(ChangeSensitivity);
    }
 
    // ========== CHANGE FUNCTIONS ==========
 
    private void ChangeMasterVolume(float value)
    {
        GameSettings.MasterVolume = value;
        AudioListener.volume = value;
        if (masterVolumeLabel != null)
            masterVolumeLabel.text = $"Master Volume: {value * 100f:F0}%";
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }
 
    private void ChangeMusicVolume(float value)
    {
        GameSettings.MusicVolume = value;
        if (audioMixer != null)
            audioMixer.SetFloat("MusicVolume", ToDecibels(value));
        if (musicVolumeLabel != null)
            musicVolumeLabel.text = $"Music Volume: {value * 100f:F0}%";
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }
 
    private void ChangeSFXVolume(float value)
    {
        GameSettings.SFXVolume = value;
        if (audioMixer != null)
            audioMixer.SetFloat("SFXVolume", ToDecibels(value));
        if (sfxVolumeLabel != null)
            sfxVolumeLabel.text = $"SFX Volume: {value * 100f:F0}%";
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }
 
    private void UpdateDialogLabel(float value)
    {
        if (dialogVolumeLabel != null)
            dialogVolumeLabel.text = $"Dialog Volume: {value * 100f:F0}%";
    }
 
    private void ChangeSensitivity(float sliderValue)
    {
        
        GameSettings.SensitivityX = sliderValue;
        GameSettings.SensitivityY = sliderValue;
        if (sensitivityLabel != null)
            sensitivityLabel.text = $"Camera Sensitivity: {sliderValue:F1}";
        PlayerPrefs.SetFloat("SensitivityX", sliderValue);
        PlayerPrefs.SetFloat("SensitivityY", sliderValue);
        PlayerPrefs.Save();
    }
}