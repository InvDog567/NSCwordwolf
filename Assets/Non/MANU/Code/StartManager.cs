using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartManager : MonoBehaviour
{
    // ========== MAIN MENU BUTTONS ==========

    [Header("Main Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    // ========== SETTINGS PANEL ==========

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;

    // Optional button for closing the settings panel.
    [SerializeField] private Button closeSettingsButton;

    // ========== LOADING SCREEN ==========

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private Image loadingProgressBar;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Min(0f)]
    [SerializeField] private float fadeDuration = 0.5f;

    [Min(0f)]
    [SerializeField] private float minimumLoadingScreenTime = 1f;

    [SerializeField] private bool useProgressBar;

    // ========== FAKE LOADING ==========

    [Header("Fake Loading")]
    [Tooltip("Makes the loading screen last for a chosen duration.")]
    [SerializeField] private bool useFakeLoading;

    [Min(0.1f)]
    [SerializeField] private float fakeLoadingDuration = 3f;

    // ========== AUDIO MIXER ==========

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Tooltip("The exposed Audio Mixer parameter for music.")]
    [SerializeField] private string musicMixerParameter = "MusicVolume";

    [Tooltip("The exposed Audio Mixer parameter for sound effects.")]
    [SerializeField] private string sfxMixerParameter = "SFXVolume";

    [Tooltip("Optional exposed Audio Mixer parameter for dialogue.")]
    [SerializeField] private string dialogMixerParameter = "DialogVolume";

    // ========== VIDEO SETTINGS ==========

    [Header("Video Settings")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    // ========== AUDIO SETTINGS ==========

    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeLabel;

    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TextMeshProUGUI musicVolumeLabel;

    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI sfxVolumeLabel;

    [SerializeField] private Slider dialogVolumeSlider;
    [SerializeField] private TextMeshProUGUI dialogVolumeLabel;

    // ========== CONTROLS SETTINGS ==========

    [Header("Controls Settings")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityLabel;

    // ========== SCENE LOADING ==========

    [Header("Scene Loading")]
    [SerializeField] private string gameSceneName = "GameScene";

    // ========== PRIVATE VARIABLES ==========

    private Resolution[] resolutions;
    private bool isLoading;

    private void Awake()
    {
        GameSettings.LoadFromPrefs();

        ApplySavedSettings();
        PrepareLoadingScreen();
    }

    private void Start()
    {
        SetupResolutions();
        LoadSettingsIntoUI();
        AddListeners();
    }

    private void OnDestroy()
    {
        RemoveListeners();
    }

    // ========== LISTENERS ==========

    private void AddListeners()
    {
        RemoveListeners();

        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(HideSettings);

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(ChangeMasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(ChangeMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(ChangeSFXVolume);

        if (dialogVolumeSlider != null)
            dialogVolumeSlider.onValueChanged.AddListener(ChangeDialogVolume);

        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(ChangeSensitivity);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(ChangeResolution);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(ChangeFullscreen);
    }

    private void RemoveListeners()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(PlayGame);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(ShowSettings);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitGame);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.RemoveListener(HideSettings);

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(ChangeMasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(ChangeMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(ChangeSFXVolume);

        if (dialogVolumeSlider != null)
            dialogVolumeSlider.onValueChanged.RemoveListener(ChangeDialogVolume);

        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.RemoveListener(ChangeSensitivity);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveListener(ChangeResolution);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(ChangeFullscreen);
    }

    // ========== MAIN MENU ==========

    private void PlayGame()
    {
        if (isLoading)
            return;

        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("The game scene name has not been assigned.");
            return;
        }

        isLoading = true;
        SetMainMenuButtonsInteractable(false);

        StartCoroutine(LoadSceneRoutine());
    }

    private void ShowSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    private void HideSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetMainMenuButtonsInteractable(bool interactable)
    {
        if (playButton != null)
            playButton.interactable = interactable;

        if (settingsButton != null)
            settingsButton.interactable = interactable;

        if (exitButton != null)
            exitButton.interactable = interactable;
    }

    // ========== LOADING SCREEN ==========

    private void PrepareLoadingScreen()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.interactable = false;
            loadingCanvasGroup.blocksRaycasts = false;
        }

        if (loadingProgressBar != null)
            loadingProgressBar.fillAmount = 0f;
    }

    private IEnumerator LoadSceneRoutine()
    {
        ShowLoadingScreen();

        yield return FadeLoadingScreen(0f, 1f);

        // Give Unity one frame to draw the loading screen.
        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(gameSceneName);

        if (operation == null)
        {
            Debug.LogError($"Could not load scene '{gameSceneName}'.");

            isLoading = false;
            SetMainMenuButtonsInteractable(true);
            HideLoadingScreenImmediately();

            yield break;
        }

        // Unity normally activates an asynchronously loaded scene when progress
        // reaches 0.9. This keeps the loading screen visible until we are ready.
        operation.allowSceneActivation = false;

        if (useFakeLoading)
        {
            yield return FakeLoadingRoutine(operation);
        }
        else
        {
            yield return RealLoadingRoutine(operation);
        }

        if (useProgressBar && loadingProgressBar != null)
            loadingProgressBar.fillAmount = 1f;

        operation.allowSceneActivation = true;

        // Wait until the scene switch has completed.
        while (!operation.isDone)
            yield return null;
    }

    private void ShowLoadingScreen()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.interactable = true;
            loadingCanvasGroup.blocksRaycasts = true;
        }

        if (loadingProgressBar != null)
            loadingProgressBar.fillAmount = 0f;

        if (loadingText != null)
            loadingText.text = "Loading";
    }

    private void HideLoadingScreenImmediately()
    {
        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.interactable = false;
            loadingCanvasGroup.blocksRaycasts = false;
        }

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    // ========== FAKE LOADING ==========

    private IEnumerator FakeLoadingRoutine(AsyncOperation operation)
    {
        float timer = 0f;
        float duration = Mathf.Max(0.1f, fakeLoadingDuration);

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float fakeProgress = Mathf.Clamp01(timer / duration);

            UpdateLoadingUI(fakeProgress);

            yield return null;
        }

        // The fake timer may finish before Unity has loaded the scene.
        while (operation.progress < 0.9f)
        {
            UpdateLoadingUI(1f);
            yield return null;
        }

        UpdateLoadingUI(1f);
    }

    // ========== REAL LOADING ==========

    private IEnumerator RealLoadingRoutine(AsyncOperation operation)
    {
        float timer = 0f;
        float minimumTime = Mathf.Max(0f, minimumLoadingScreenTime);

        while (operation.progress < 0.9f || timer < minimumTime)
        {
            timer += Time.unscaledDeltaTime;

            // Unity reports scene-loading progress from 0 to 0.9 before activation.
            float sceneProgress = Mathf.Clamp01(operation.progress / 0.9f);

            float timeProgress = minimumTime > 0f
                ? Mathf.Clamp01(timer / minimumTime)
                : 1f;

            // Do not display 100% until both requirements are complete.
            float displayedProgress = Mathf.Min(sceneProgress, timeProgress);

            UpdateLoadingUI(displayedProgress);

            yield return null;
        }

        UpdateLoadingUI(1f);
    }

    private void UpdateLoadingUI(float progress)
    {
        if (useProgressBar && loadingProgressBar != null)
            loadingProgressBar.fillAmount = Mathf.Clamp01(progress);

        if (loadingText != null)
            loadingText.text = GetAnimatedLoadingText();
    }

    private IEnumerator FadeLoadingScreen(float from, float to)
    {
        if (loadingCanvasGroup == null)
            yield break;

        if (fadeDuration <= 0f)
        {
            loadingCanvasGroup.alpha = to;
            yield break;
        }

        float timer = 0f;
        loadingCanvasGroup.alpha = from;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(timer / fadeDuration);
            loadingCanvasGroup.alpha = Mathf.Lerp(from, to, progress);

            yield return null;
        }

        loadingCanvasGroup.alpha = to;
    }

    private string GetAnimatedLoadingText()
    {
        int dotCount = Mathf.FloorToInt(Time.unscaledTime * 3f) % 4;

        return dotCount switch
        {
            1 => "Loading.",
            2 => "Loading..",
            3 => "Loading...",
            _ => "Loading"
        };
    }

    // ========== AUDIO ==========

    private void ApplySavedSettings()
    {
        AudioListener.volume = Mathf.Clamp01(GameSettings.MasterVolume);

        ApplyAudioMixer();

        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = fullscreen;
    }

    private void ApplyAudioMixer()
    {
        if (audioMixer == null)
            return;

        SetMixerVolume(musicMixerParameter, GameSettings.MusicVolume);
        SetMixerVolume(sfxMixerParameter, GameSettings.SFXVolume);
        SetMixerVolume(dialogMixerParameter, GameSettings.DialogVolume);
    }

    private void SetMixerVolume(string parameterName, float value)
    {
        if (audioMixer == null || string.IsNullOrWhiteSpace(parameterName))
            return;

        audioMixer.SetFloat(parameterName, ToDecibels(value));
    }

    private float ToDecibels(float value)
    {
        value = Mathf.Clamp01(value);

        return value > 0.0001f
            ? Mathf.Log10(value) * 20f
            : -80f;
    }

    private void ChangeMasterVolume(float value)
    {
        value = Mathf.Clamp01(value);

        GameSettings.MasterVolume = value;
        AudioListener.volume = value;

        UpdateVolumeLabel(masterVolumeLabel, "Master Volume", value);

        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    private void ChangeMusicVolume(float value)
    {
        value = Mathf.Clamp01(value);

        GameSettings.MusicVolume = value;
        SetMixerVolume(musicMixerParameter, value);

        UpdateVolumeLabel(musicVolumeLabel, "Music Volume", value);

        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    private void ChangeSFXVolume(float value)
    {
        value = Mathf.Clamp01(value);

        GameSettings.SFXVolume = value;
        SetMixerVolume(sfxMixerParameter, value);

        UpdateVolumeLabel(sfxVolumeLabel, "SFX Volume", value);

        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    private void ChangeDialogVolume(float value)
    {
        value = Mathf.Clamp01(value);

        GameSettings.DialogVolume = value;
        SetMixerVolume(dialogMixerParameter, value);

        UpdateVolumeLabel(dialogVolumeLabel, "Dialog Volume", value);

        PlayerPrefs.SetFloat("DialogVolume", value);
        PlayerPrefs.Save();
    }

    private void UpdateVolumeLabel(
        TextMeshProUGUI label,
        string labelName,
        float value)
    {
        if (label != null)
            label.text = $"{labelName}: {value * 100f:F0}%";
    }

    // ========== VIDEO ==========

    private void SetupResolutions()
    {
        if (resolutionDropdown == null)
            return;

        Resolution[] availableResolutions = Screen.resolutions;

        if (availableResolutions == null || availableResolutions.Length == 0)
        {
            Debug.LogWarning("No screen resolutions were returned by Unity.");
            resolutionDropdown.interactable = false;
            return;
        }

        // Remove duplicate width/height entries that only differ by refresh rate.
        List<Resolution> uniqueResolutions = new List<Resolution>();
        HashSet<string> addedResolutions = new HashSet<string>();

        foreach (Resolution resolution in availableResolutions)
        {
            string key = $"{resolution.width}x{resolution.height}";

            if (addedResolutions.Add(key))
                uniqueResolutions.Add(resolution);
        }

        resolutions = uniqueResolutions.ToArray();

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution resolution = resolutions[i];

            options.Add($"{resolution.width} x {resolution.height}");

            if (resolution.width == Screen.width &&
                resolution.height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        int savedResolutionIndex = PlayerPrefs.GetInt(
            "ResolutionIndex",
            currentResolutionIndex);

        savedResolutionIndex = Mathf.Clamp(
            savedResolutionIndex,
            0,
            resolutions.Length - 1);

        resolutionDropdown.SetValueWithoutNotify(savedResolutionIndex);
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.interactable = true;
    }

    private void ChangeResolution(int index)
    {
        if (resolutions == null ||
            resolutions.Length == 0 ||
            index < 0 ||
            index >= resolutions.Length)
        {
            return;
        }

        Resolution resolution = resolutions[index];

        Screen.SetResolution(
            resolution.width,
            resolution.height,
            Screen.fullScreen);

        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
    }

    private void ChangeFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;

        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ========== CONTROLS ==========

    private void ChangeSensitivity(float sliderValue)
    {
        GameSettings.SensitivityX = sliderValue;
        GameSettings.SensitivityY = sliderValue;

        if (sensitivityLabel != null)
        {
            sensitivityLabel.text =
                $"Camera Sensitivity: {sliderValue:F1}";
        }

        PlayerPrefs.SetFloat("SensitivityX", sliderValue);
        PlayerPrefs.SetFloat("SensitivityY", sliderValue);
        PlayerPrefs.Save();
    }

    // ========== LOAD SETTINGS INTO UI ==========

    private void LoadSettingsIntoUI()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(
                GameSettings.MasterVolume);
        }

        UpdateVolumeLabel(
            masterVolumeLabel,
            "Master Volume",
            GameSettings.MasterVolume);

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(
                GameSettings.MusicVolume);
        }

        UpdateVolumeLabel(
            musicVolumeLabel,
            "Music Volume",
            GameSettings.MusicVolume);

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(
                GameSettings.SFXVolume);
        }

        UpdateVolumeLabel(
            sfxVolumeLabel,
            "SFX Volume",
            GameSettings.SFXVolume);

        if (dialogVolumeSlider != null)
        {
            dialogVolumeSlider.SetValueWithoutNotify(
                GameSettings.DialogVolume);
        }

        UpdateVolumeLabel(
            dialogVolumeLabel,
            "Dialog Volume",
            GameSettings.DialogVolume);

        if (sensitivitySlider != null)
        {
            sensitivitySlider.SetValueWithoutNotify(
                GameSettings.SensitivityX);
        }

        if (sensitivityLabel != null)
        {
            sensitivityLabel.text =
                $"Camera Sensitivity: {GameSettings.SensitivityX:F1}";
        }

        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(fullscreen);

        AudioListener.volume = Mathf.Clamp01(GameSettings.MasterVolume);
        Screen.fullScreen = fullscreen;

        Debug.Log("Settings loaded successfully.");
    }
}