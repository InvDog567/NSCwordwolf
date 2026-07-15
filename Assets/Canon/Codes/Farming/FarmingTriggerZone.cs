using UnityEngine;

/// <summary>
/// Place a trigger collider at the edge of the field — walking in starts the minigame.
///
/// SETUP REQUIREMENTS:
/// - This GameObject needs a Collider (set to Is Trigger = true)
/// - The player needs either: a Rigidbody, OR this object needs a Rigidbody (add a kinematic one here if needed)
/// - Player GameObject must have the tag "Player"
/// </summary>
[RequireComponent(typeof(Collider))]
public class FarmingTriggerZone : MonoBehaviour
{
    void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        // CharacterController players don't have a Rigidbody, so Unity won't fire
        // OnTriggerEnter unless at least one side has a Rigidbody.
        // Add a kinematic one to this zone automatically if neither side has one.
        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"ENTERED TRIGGER: {other.name}, tag: {other.tag}", other);
        TryStart(other);
    }

    // Fallback: catches the case where the player was already inside when Play started
    void OnTriggerStay(Collider other)
    {
        TryStart(other);
    }

    private void TryStart(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (PlayerJobManager.Instance != null && !PlayerJobManager.Instance.CanPlayMinigame(PlayerJobManager.Job.Farming))
        {
            Debug.Log("[FarmingTriggerZone] Player cannot play farming minigame (either not job or not daytime).");
            return;
        }

        FarmingController fc = FarmingController.Instance;
        if (fc == null)
        {
            Debug.LogError("[FarmingTriggerZone] FarmingController.Instance is null!", this);
            return;
        }

        if (!fc.IsMinigameActive)
        {
            Debug.Log("[FarmingTriggerZone] Starting farming minigame.");
            fc.StartMinigame();
        }
    }
}