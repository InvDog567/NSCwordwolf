using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Place this script in the DAY SCENE only.
/// It reads the assigned job from the DontDestroyOnLoad PlayerJobManager
/// and enables/disables the correct GameObjects and components in THIS scene.
/// </summary>
public class JobSceneSetup : MonoBehaviour
{
    [System.Serializable]
    public class JobMinigameConfig
    {
        public PlayerJobManager.Job job;

        [Tooltip("MonoBehaviour components on the player (e.g. FarmingController) to enable only for this job.")]
        public MonoBehaviour[] componentsToEnable;

        [Tooltip("For scripts on the persistent Player, enter their class names here (for example: FarmingController). These are found at runtime, so no cross-scene reference is needed.")]
        public string[] persistentPlayerScriptNamesToEnable;

        [Tooltip("GameObjects in the Day scene (e.g. FarmingTriggerZone) to activate only for this job.")]
        public GameObject[] gameObjectsToEnable;
    }

    [Header("Job Configurations — drag Day scene objects here")]
    public List<JobMinigameConfig> jobConfigs = new List<JobMinigameConfig>();

    private void Start()
    {
        if (PlayerJobManager.Instance == null)
        {
            Debug.LogWarning("[JobSceneSetup] PlayerJobManager not found. Cannot apply job configurations.");
            return;
        }

        ApplyJobConfigurations(PlayerJobManager.Instance.currentJob);
    }

    private static readonly HashSet<string> CorePlayerScripts = new HashSet<string>
    {
        "Player", "CharacterController", "PlayerRole", "DayAbility", "NightAbility"
    };

    private void ApplyJobConfigurations(PlayerJobManager.Job currentJob)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            Debug.LogWarning("[JobSceneSetup] Player tagged Player was not found. Persistent player scripts cannot be configured.");

        foreach (var config in jobConfigs)
        {
            bool isActiveJob = (config.job == currentJob);

            if (config.componentsToEnable != null)
            {
                foreach (var comp in config.componentsToEnable)
                {
                    if (comp != null)
                    {
                        if (!isActiveJob && CorePlayerScripts.Contains(comp.GetType().Name))
                        {
                            Debug.LogWarning($"[JobSceneSetup] Prevented disabling core script '{comp.GetType().Name}' for non-active job '{config.job}'.");
                            continue;
                        }
                        comp.enabled = isActiveJob;
                        Debug.Log($"[JobSceneSetup] Component '{comp.GetType().Name}' set to enabled={isActiveJob} for job {config.job}");
                    }
                }
            }

            SetPersistentPlayerScripts(player, config.persistentPlayerScriptNamesToEnable, isActiveJob, config.job);

            if (config.gameObjectsToEnable != null)
            {
                foreach (var go in config.gameObjectsToEnable)
                {
                    if (go != null)
                    {
                        go.SetActive(isActiveJob);
                        Debug.Log($"[JobSceneSetup] GameObject '{go.name}' set active={isActiveJob} for job {config.job}");
                    }
                }
            }
        }

        Debug.Log($"[JobSceneSetup] Applied configurations for job: {currentJob}");
    }

    private void SetPersistentPlayerScripts(GameObject player, string[] scriptNames, bool enabled, PlayerJobManager.Job job)
    {
        if (player == null || scriptNames == null)
            return;

        MonoBehaviour[] playerComponents = player.GetComponents<MonoBehaviour>();
        foreach (string scriptName in scriptNames)
        {
            if (string.IsNullOrWhiteSpace(scriptName))
                continue;

            string cleanName = scriptName.Trim();

            if (!enabled && CorePlayerScripts.Contains(cleanName))
            {
                Debug.LogWarning($"[JobSceneSetup] Refusing to disable core player script '{cleanName}' specified under job config '{job}'.");
                continue;
            }

            bool found = false;
            foreach (MonoBehaviour component in playerComponents)
            {
                if (component == null || component.GetType().Name != cleanName)
                    continue;

                component.enabled = enabled;
                Debug.Log($"[JobSceneSetup] Player script '{cleanName}' set to enabled={enabled} for job {job}");
                found = true;
            }

            if (!found)
                Debug.LogWarning($"[JobSceneSetup] Player script '{cleanName}' was not found on the Player GameObject.");
        }
    }
}
