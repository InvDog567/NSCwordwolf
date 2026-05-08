using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // ========== MAIN MENU BUTTONS ==========
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    // ========== GAME SCENE BUTTONS ==========
    [SerializeField] private Button backToMenuButton;

    // ========== SETTINGS PANEL ==========
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button backButton;

    // ========== SETTINGS CONTROLS ==========
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeLabel;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    // ========== SCENE NAME ==========
    [SerializeField] private string gameSceneName = "GameScene"; // เปลี่ยนชื่อได้ที่นี่

    // ========== PRIVATE VARIABLES ==========
    private AudioListener audioListener;
    private Resolution[] resolutions;

    private void Start()
    {
        // หาตัว AudioListener
        audioListener = FindObjectOfType<AudioListener>();
        if (audioListener == null)
        {
            Debug.LogWarning("ไม่พบ AudioListener ในเซน");
        }

        // เช็คว่าปุ่มเหล่านี้มีหรือไม่
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        if (backButton != null)
            backButton.onClick.AddListener(HideSettings);

        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(BackToMenu);

        // ตั้งค่า Slider
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(ChangeVolume);

        // ตั้งค่า Dropdown
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(ChangeResolution);

        // ตั้งค่า Toggle
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(ChangeFullscreen);

        // โหลดค่า Settings ที่บันทึกไว้
        LoadSettings();

        // ตั้งค่า Resolution Dropdown
        if (resolutionDropdown != null)
            SetupResolutions();
    }

    // ========== MAIN MENU FUNCTIONS ==========

    private void PlayGame()
    {
        Debug.Log($"เข้าเกม: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
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
        Debug.Log("ออกจากเกม!");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // ========== GAME SCENE FUNCTIONS ==========

    private void BackToMenu()
    {
        Debug.Log("กลับไปเมนู!");
        SceneManager.LoadScene("MainMenu");
    }

    // ========== SETTINGS FUNCTIONS ==========

    private void ChangeVolume(float volume)
    {
        // ปรับระดับเสียง (0-100)
        float volumePercent = volume * 100f;
        AudioListener.volume = volume;

        // อัปเดต UI
        if (volumeLabel != null)
            volumeLabel.text = $"Volume: {volumePercent:F0}%";

        // บันทึก Setting
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.Save();

        Debug.Log($"เปลี่ยนเสียง: {volumePercent:F0}%");
    }

    private void SetupResolutions()
    {
        // หาความละเอียดจอที่รองรับทั้งหมด
        resolutions = Screen.resolutions;

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();

            int currentResolutionIndex = 0;

            for (int i = 0; i < resolutions.Length; i++)
            {
                string resolutionOption = resolutions[i].width + " x " + resolutions[i].height;
                resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(resolutionOption));

                // เช็คความละเอียดปัจจุบัน
                if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }
    }

    private void ChangeResolution(int resolutionIndex)
    {
        if (resolutions != null && resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

            // บันทึก Setting
            PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
            PlayerPrefs.Save();

            Debug.Log($"เปลี่ยนความละเอียด: {resolution.width} x {resolution.height}");
        }
    }

    private void ChangeFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;

        // บันทึก Setting
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"เต็มจอ: {isFullscreen}");
    }

    // ========== LOAD/SAVE SETTINGS ==========

    private void LoadSettings()
    {
        // โหลด Volume
        float savedVolume = PlayerPrefs.GetFloat("Volume", 0.5f);
        if (volumeSlider != null)
            volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;

        // โหลด Resolution
        int resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        if (resolutionDropdown != null)
            resolutionDropdown.value = resolutionIndex;

        // โหลด Fullscreen
        int fullscreen = PlayerPrefs.GetInt("Fullscreen", 1);
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = fullscreen == 1;
        Screen.fullScreen = fullscreen == 1;

        Debug.Log("โหลด Settings สำเร็จ");
    }
}
