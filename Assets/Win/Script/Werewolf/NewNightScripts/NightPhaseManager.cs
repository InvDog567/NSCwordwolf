using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central orchestrator for the Night Phase overhaul.
/// Placed in the Night scene to manage all night-specific logic.
/// </summary>
public class NightPhaseManager : MonoBehaviour
{
    [Header("Feature Toggles")]
    public bool enableFog = true;
    public bool enableNpcNightMovement = true;
    public bool enableNpcWerewolfHunting = true;
    public bool enableWerewolfSpeedBoost = true;

    [Header("Managers (Auto-Generated)")]
    [Tooltip("These will be created automatically. You do not need to assign prefabs.")]
    // Removed prefab fields for easier setup

    private List<GameObject> activeSubSystems = new List<GameObject>();
    private List<NpcNightBehavior> spawnedNpcBehaviors = new List<NpcNightBehavior>();

    private void Start()
    {
        InitializeNightPhase();
    }

    private void InitializeNightPhase()
    {
        // 1. Ensure Memory Bank exists
        if (NightMemoryBank.Instance == null)
        {
            GameObject bankObj = new GameObject("NightMemoryBank");
            bankObj.AddComponent<NightMemoryBank>();
        }

        // 2. Ensure SP Tracker exists
        if (SuspicionPointTracker.Instance == null)
        {
            GameObject spObj = new GameObject("SuspicionPointTracker");
            spObj.AddComponent<SuspicionPointTracker>();
        }

        // 2.5 Ensure Destination Registry exists
        if (NightDestinationRegistry.Instance == null)
        {
            GameObject regObj = new GameObject("NightDestinationRegistry");
            regObj.AddComponent<NightDestinationRegistry>();
        }

        // Clear memories at the start of the night
        if (NightMemoryBank.Instance != null)
        {
            NightMemoryBank.Instance.Clear();
        }

        // 3. Fog
        if (enableFog)
        {
            GameObject fogObj = new GameObject("NightFogController");
            fogObj.transform.SetParent(transform);
            fogObj.AddComponent<NightFogController>();
            activeSubSystems.Add(fogObj);
        }

        // 4. NPC Night Movement
        if (enableNpcNightMovement)
        {
            PlayerRole[] roles = FindObjectsOfType<PlayerRole>();
            foreach (var pr in roles)
            {
                if (pr.npcIndex != -1) // It's an NPC
                {
                    // Add Night Behavior dynamically so we don't break Day prefabs
                    NpcNightBehavior behavior = pr.gameObject.AddComponent<NpcNightBehavior>();
                    spawnedNpcBehaviors.Add(behavior);
                }
            }
        }

        // 5. NPC Werewolf Hunting
        if (enableNpcWerewolfHunting)
        {
            GameObject hunterObj = new GameObject("NpcWerewolfHunter");
            hunterObj.transform.SetParent(transform);
            hunterObj.AddComponent<NpcWerewolfHunter>();
            activeSubSystems.Add(hunterObj);
        }

        // 6. Werewolf Speed Boost
        if (enableWerewolfSpeedBoost)
        {
            GameObject speedObj = new GameObject("WerewolfSpeedController");
            speedObj.transform.SetParent(transform);
            speedObj.AddComponent<WerewolfSpeedController>();
            activeSubSystems.Add(speedObj);
        }
    }

    private void OnDestroy()
    {
        // Clean up components added to NPCs when the scene ends/manager is destroyed
        foreach (var behavior in spawnedNpcBehaviors)
        {
            if (behavior != null)
            {
                Destroy(behavior);
            }
        }
        spawnedNpcBehaviors.Clear();

        // The activeSubSystems are parented to this, so they destroy automatically
    }
}
