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

    private void ApplyJobConfigurations(PlayerJobManager.Job currentJob)
    {
        foreach (var config in jobConfigs)
        {
            bool isActiveJob = (config.job == currentJob);

            if (config.componentsToEnable != null)
            {
                foreach (var comp in config.componentsToEnable)
                {
                    if (comp != null)
                        comp.enabled = isActiveJob;
                }
            }

            if (config.gameObjectsToEnable != null)
            {
                foreach (var go in config.gameObjectsToEnable)
                {
                    if (go != null)
                        go.SetActive(isActiveJob);
                }
            }
        }

        Debug.Log($"[JobSceneSetup] Applied configurations for job: {currentJob}");
    }
}
