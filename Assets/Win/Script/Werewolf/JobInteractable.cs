using UnityEngine;

public class JobInteractable : MonoBehaviour
{
    [Header("Job Requirement")]
    public PlayerJobManager.Job requiredJob;

    [Header("Minigame UI/Canvas Panel to open")]
    public GameObject minigameUIPanel;

    [Header("Interaction Settings")]
    public float interactRange = 3f;
    public KeyCode interactKey = KeyCode.E;
    public string promptMessage = "Press E to start minigame";

    [Header("Player Settings to Lock during minigame")]
    public MonoBehaviour playerMovement;
    public MonoBehaviour cameraLook;

    private bool _playerLooking = false;

    private void Update()
    {
        if (minigameUIPanel != null && minigameUIPanel.activeSelf) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, interactRange);

        bool lookingAtThis = hit && (hitInfo.collider.gameObject == gameObject || hitInfo.collider.transform.IsChildOf(transform));

        if (lookingAtThis != _playerLooking)
        {
            _playerLooking = lookingAtThis;
            if (InteractPromptUI.Instance != null)
            {
                if (_playerLooking)
                    InteractPromptUI.Instance.Show(promptMessage);
                else
                    InteractPromptUI.Instance.Hide();
            }
        }

        if (_playerLooking && Input.GetKeyDown(interactKey))
        {
            if (PlayerJobManager.Instance != null && PlayerJobManager.Instance.CanPlayMinigame(requiredJob))
            {
                OpenMinigame();
            }
            else
            {
                Debug.Log($"[JobInteractable] Cannot play minigame for job {requiredJob}. Incorrect job or not daytime.");
            }
        }
    }

    private void OpenMinigame()
    {
        if (InteractPromptUI.Instance != null)
            InteractPromptUI.Instance.Hide();

        if (minigameUIPanel != null)
            minigameUIPanel.SetActive(true);

        if (playerMovement != null) playerMovement.enabled = false;
        if (cameraLook != null) cameraLook.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseMinigame()
    {
        if (minigameUIPanel != null)
            minigameUIPanel.SetActive(false);

        if (playerMovement != null) playerMovement.enabled = true;
        if (cameraLook != null) cameraLook.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        if (_playerLooking && InteractPromptUI.Instance != null)
            InteractPromptUI.Instance.Hide();
        _playerLooking = false;
    }
}
