using UnityEngine;
using UnityEngine.SceneManagement;
 
public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu Panel")]
    public GameObject pausePanel;
 
    [Header("Settings Panel (assign to block Esc when settings is open)")]
    public GameObject settingsPanel;
 
    [Header("Scenes")]
    public string mainMenuSceneName;
 
    public AudioSource audioSource;
    public AudioClip pauseOpenSound;
 
    private bool isPaused = false;
 
    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }
 
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If settings panel is open, let GameSettingsLoader handle Esc — do nothing here
            if (settingsPanel != null && settingsPanel.activeSelf)
                return;
 
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }
 
    void Pause()
    {
        isPaused = true;
        if (audioSource != null && pauseOpenSound != null)
            audioSource.PlayOneShot(pauseOpenSound);
        Time.timeScale = 0f;
        if (pausePanel != null)
            pausePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
 
    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
 
    public void Restart()
    {
        Time.timeScale = 1f;
        ClearAllData();
        SceneManager.LoadScene(mainMenuSceneName);
    }
 
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Game quit");
    }
 
    void ClearAllData()
    {
        if (PlayerRoleRandomizer.Instance != null)
            Destroy(PlayerRoleRandomizer.Instance.gameObject);
        if (GameManager.Instance != null)
            Destroy(GameManager.Instance.gameObject);
    }
}