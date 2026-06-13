using UnityEngine;
 
public class SettingsMenu : MonoBehaviour
{
    [Header("Setting Panels")]
    public GameObject videoPanel;
    public GameObject audioPanel;
    public GameObject gameplayPanel;
    public GameObject controlsPanel;
 
    void Start()
    {
        CloseAllPanels();
    }
 
    public void OpenVideo()
    {
        CloseAllPanels();
        videoPanel.SetActive(true);
    }
 
    public void OpenAudio()
    {
        CloseAllPanels();
        audioPanel.SetActive(true);
    }
 
    public void OpenGameplay()
    {
        CloseAllPanels();
        gameplayPanel.SetActive(true);
    }
 
    public void OpenControls()
    {
        CloseAllPanels();
        controlsPanel.SetActive(true);
    }
 
    void CloseAllPanels()
    {
        videoPanel.SetActive(false);
        audioPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }
}