using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Collections;
 
public class StartManager : MonoBehaviour
{
    // ========== MAIN MENU BUTTONS ==========
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
 
    // ========== SETTINGS PANEL ==========
    [SerializeField] private GameObject settingsPanel;
 
    // ========== AUDIO MIXER ==========
    [SerializeField] private AudioMixer audioMixer;
 
    // ========== VIDEO SETTINGS ==========
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
 
    // ========== AUDIO SETTINGS ==========
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeLabel;
 
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TextMeshProUGUI musicVolumeLabel;
 
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI sfxVolumeLabel;
 
    [SerializeField] private Slider dialogVolumeSlider;
    [SerializeField] private TextMeshProUGUI dialogVolumeLabel;
 
    // ========== CONTROLS SETTINGS ==========
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityLabel;
 
    // ========== SCENE NAME ==========
    [SerializeField] private string gameSceneName = "GameScene";
 
    // ========== DELAY ==========
    [SerializeField] private float sceneLoadDelay = 1f;
 
    // ========== PRIVATE ==========
    private Resolution[] resolutions;
 
    private void Awake()
    {
        GameSettings.LoadFromPrefs();
        ApplyAudioMixer();
    }
 
    private void Start()
    {
        if (playButton != null)   playButton.onClick.AddListener(PlayGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(ShowSettings);
        if (exitButton != null)   exitButton.onClick.AddListener(ExitGame);
 
        if (masterVolumeSlider != null)  masterVolumeSlider.onValueChanged.AddListener(ChangeMasterVolume);
        if (musicVolumeSlider != null)   musicVolumeSlider.onValueChanged.AddListener(ChangeMusicVolume);
        if (sfxVolumeSlider != null)     sfxVolumeSlider.onValueChanged.AddListener(ChangeSFXVolume);
        if (dialogVolumeSlider != null)  dialogVolumeSlider.onValueChanged.AddListener(UpdateDialogLabel);
        if (sensitivitySlider != null)   sensitivitySlider.onValueChanged.AddListener(ChangeSensitivity);
 
        if (resolutionDropdown != null)  resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
        if (fullscreenToggle != null)    fullscreenToggle.onValueChanged.AddListener(ChangeFullscreen);
 
        LoadSettingsIntoUI();
 
        if (resolutionDropdown != null)
            SetupResolutions();
    }
 
    // ========== MAIN MENU ==========
 
    private void PlayGame()
    {
        playButton.interactable = false;
        StartCoroutine(LoadAfterDelay());
    }
 
    private IEnumerator LoadAfterDelay()
    {
        yield return new WaitForSeconds(sceneLoadDelay);
        SceneManager.LoadScene(gameSceneName);
    }
 
    private void ShowSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }
 
    private void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
 
    // ========== AUDIO ==========
 
    // Converts 0-1 slider value to decibels and applies to mixer
    // AudioMixer uses dB: 0dB = full, -80dB = silent
    private float ToDecibels(float value)
    {
        return value > 0.001f ? Mathf.Log10(value) * 20f : -80f;
    }
 
    private void ApplyAudioMixer()
    {
        if (audioMixer == null) return;
        audioMixer.SetFloat("MusicVolume", ToDecibels(GameSettings.MusicVolume));
        audioMixer.SetFloat("SFXVolume",   ToDecibels(GameSettings.SFXVolume));
    }
 
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
 
    // Dialog: UI only for now
    private void UpdateDialogLabel(float value)
    {
        if (dialogVolumeLabel != null)
            dialogVolumeLabel.text = $"Dialog Volume: {value * 100f:F0}%";
    }
 
    // ========== VIDEO ==========
 
    private void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        int currentResolutionIndex = 0;
 
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(option));
 
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                currentResolutionIndex = i;
        }
 
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
 
    private void ChangeResolution(int index)
    {
        if (resolutions != null && index < resolutions.Length)
        {
            Resolution res = resolutions[index];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            PlayerPrefs.SetInt("ResolutionIndex", index);
            PlayerPrefs.Save();
        }
    }
 
    private void ChangeFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
 
    // ========== CONTROLS ==========
 
    // Slider in Inspector: Min=0, Max=1
    // Remaps to real sensitivity: 0=0.1 (very slow), 0.5=2.0 (normal), 1.0=5.0 (fast)
    
    
 
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
 
    // ========== LOAD INTO UI ==========
 
    private void LoadSettingsIntoUI()
    {
        // Remove listeners first to avoid double-firing
        if (masterVolumeSlider != null)  masterVolumeSlider.onValueChanged.RemoveListener(ChangeMasterVolume);
        if (musicVolumeSlider != null)   musicVolumeSlider.onValueChanged.RemoveListener(ChangeMusicVolume);
        if (sfxVolumeSlider != null)     sfxVolumeSlider.onValueChanged.RemoveListener(ChangeSFXVolume);
        if (dialogVolumeSlider != null)  dialogVolumeSlider.onValueChanged.RemoveListener(UpdateDialogLabel);
        if (sensitivitySlider != null)   sensitivitySlider.onValueChanged.RemoveListener(ChangeSensitivity);
 
        if (masterVolumeSlider != null)  masterVolumeSlider.value = GameSettings.MasterVolume;
        if (masterVolumeLabel != null)   masterVolumeLabel.text = $"Master Volume: {GameSettings.MasterVolume * 100f:F0}%";
        AudioListener.volume = GameSettings.MasterVolume;
 
        if (musicVolumeSlider != null)   musicVolumeSlider.value = GameSettings.MusicVolume;
        if (musicVolumeLabel != null)    musicVolumeLabel.text = $"Music Volume: {GameSettings.MusicVolume * 100f:F0}%";
 
        if (sfxVolumeSlider != null)     sfxVolumeSlider.value = GameSettings.SFXVolume;
        if (sfxVolumeLabel != null)      sfxVolumeLabel.text = $"SFX Volume: {GameSettings.SFXVolume * 100f:F0}%";
 
        if (dialogVolumeSlider != null)  dialogVolumeSlider.value = GameSettings.DialogVolume;
        if (dialogVolumeLabel != null)   dialogVolumeLabel.text = $"Dialog Volume: {GameSettings.DialogVolume * 100f:F0}%";
 
        if (sensitivitySlider != null)   sensitivitySlider.value = GameSettings.SensitivityX;
        if (sensitivityLabel != null)    sensitivityLabel.text = $"Camera Sensitivity: {GameSettings.SensitivityX:F1}";
 
 
 
        int resIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        if (resolutionDropdown != null)  resolutionDropdown.value = resIndex;
 
        int fullscreen = PlayerPrefs.GetInt("Fullscreen", 1);
        if (fullscreenToggle != null)    fullscreenToggle.isOn = fullscreen == 1;
        Screen.fullScreen = fullscreen == 1;
 
        // Re-add listeners
        if (masterVolumeSlider != null)  masterVolumeSlider.onValueChanged.AddListener(ChangeMasterVolume);
        if (musicVolumeSlider != null)   musicVolumeSlider.onValueChanged.AddListener(ChangeMusicVolume);
        if (sfxVolumeSlider != null)     sfxVolumeSlider.onValueChanged.AddListener(ChangeSFXVolume);
        if (dialogVolumeSlider != null)  dialogVolumeSlider.onValueChanged.AddListener(UpdateDialogLabel);
        if (sensitivitySlider != null)   sensitivitySlider.onValueChanged.AddListener(ChangeSensitivity);
 
        Debug.Log("โหลด Settings สำเร็จ");
    }
}