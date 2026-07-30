using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach this script to a GameObject in the Night Scene.
/// Allows triggering a shortcut key (e.g. T or any assignable KeyCode) or UI Button click
/// to set the remaining NightTimer value to 3 seconds, letting it finish naturally.
/// </summary>
public class NightTimerShortcutButton : MonoBehaviour
{
    [Header("Shortcut Key")]
    public KeyCode shortcutKey = KeyCode.T;

    [Header("Optional UI Button Reference")]
    public Button shortcutButton;

    [Header("References")]
    public NightTimer nightTimer;

    [Header("Shortcut Settings")]
    [Tooltip("Target remaining time in seconds when shortcut key or button is triggered.")]
    public float targetRemainingTime = 3f;

    private void Awake()
    {
        if (shortcutButton == null)
            shortcutButton = GetComponent<Button>();

        if (shortcutButton != null)
        {
            shortcutButton.onClick.AddListener(TriggerShortcut);
        }
    }

    private void Start()
    {
        if (nightTimer == null)
        {
            nightTimer = FindFirstObjectByType<NightTimer>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(shortcutKey))
        {
            TriggerShortcut();
        }
    }

    public void TriggerShortcut()
    {
        if (nightTimer == null)
        {
            nightTimer = FindFirstObjectByType<NightTimer>();
        }

        if (nightTimer != null)
        {
            if (nightTimer.timer > targetRemainingTime)
            {
                nightTimer.timer = targetRemainingTime;
                Debug.Log($"[NightTimerShortcut] Timer shortened to {targetRemainingTime} seconds via key/button.");
            }
        }
        else
        {
            Debug.LogError("[NightTimerShortcut] NightTimer reference is missing in the scene!");
        }
    }

    private void OnDestroy()
    {
        if (shortcutButton != null)
        {
            shortcutButton.onClick.RemoveListener(TriggerShortcut);
        }
    }
}
