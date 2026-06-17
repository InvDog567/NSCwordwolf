using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages all fishing-related HUD elements.
/// Wire up the references in the Inspector.
/// </summary>
public class FishingUI : MonoBehaviour
{
    [Header("Interaction Prompt")]
    public GameObject promptPanel;          // "Press E to Fish"
    public TextMeshProUGUI promptText;      // optional: customize the label

    [Header("Waiting UI")]
    public GameObject waitingPanel;         // bobber / "waiting..." indicator
    public TextMeshProUGUI waitingText;     // e.g., "Waiting for a bite..."

    [Header("Bite Alert")]
    public GameObject biteAlertPanel;       // "!" exclamation or "Fish On!" banner — set text in Editor

    [Header("Result Panel")]
    public GameObject resultPanel;          // set text in Editor; shown on any result


    void Awake()
    {
        // Ensure everything starts hidden
        SetActive(promptPanel, false);
        SetActive(waitingPanel, false);
        SetActive(biteAlertPanel, false);
        SetActive(resultPanel, false);
    }

    // Prompt

    public void ShowPrompt(bool visible)
    {
        if (promptText != null && visible)
            promptText.text = "[E]  Fish";
        SetActive(promptPanel, visible);
    }

    // Waiting

    public void ShowWaiting(bool visible)
    {
        if (waitingText != null && visible)
            waitingText.text = "Waiting for a bite...";
        SetActive(waitingPanel, visible);
    }

    // Bite Alert

    public void ShowBiteAlert(bool visible)
    {
        // Panel text is set up in the Editor — just toggle visibility
        SetActive(biteAlertPanel, visible);
    }

    // Result

    public void ShowResult(bool caught)
    {
        // Panel text is set up in the Editor — just toggle visibility
        // (wire up separate caught/lost panels if you need different visuals)
        SetActive(resultPanel, true);
    }

    public void HideResult()
    {
        SetActive(resultPanel, false);
    }

    // Utility

    private void SetActive(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }
}