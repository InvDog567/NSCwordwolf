using UnityEngine;

/// <summary>
/// Attach this to the fishing spot GameObject (with a Collider).
/// The player's camera must have a tag "MainCamera" or be assigned via FishingController.
/// </summary>
public class FishingTriggerZone : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How far the player can be from the zone to interact")]
    public float interactRange = 3f;

    [Header("References")]
    public FishingController fishingController;

    private bool _playerLooking = false;

    void Update()
    {
        if (fishingController == null || fishingController.IsFishing) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // Raycast from center of screen
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, interactRange);

        bool lookingAtThis = hit && hitInfo.collider.gameObject == gameObject;

        if (lookingAtThis != _playerLooking)
        {
            _playerLooking = lookingAtThis;
            fishingController.SetPromptVisible(_playerLooking);
        }

        if (_playerLooking && Input.GetKeyDown(KeyCode.E))
        {
            if (PlayerJobManager.Instance != null && !PlayerJobManager.Instance.CanPlayMinigame(PlayerJobManager.Job.Fishing))
            {
                Debug.Log("[FishingTriggerZone] Player cannot play fishing minigame (either not job or not daytime).");
                return;
            }
            fishingController.StartFishing();
        }
    }

    void OnDisable()
    {
        if (fishingController != null)
            fishingController.SetPromptVisible(false);
    }
}