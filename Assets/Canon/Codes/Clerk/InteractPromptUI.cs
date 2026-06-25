using UnityEngine;
using TMPro;

public class InteractPromptUI : MonoBehaviour
{
    public static InteractPromptUI Instance { get; private set; }

    [Header("References")]
    public GameObject panel;      // the whole prompt panel
    public TMP_Text promptText; // the text inside it

    void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(string text)
    {
        promptText.text = text;
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}