using UnityEngine;

public class JobSpawnManager : MonoBehaviour
{
    [Header("Job Spawn Points")]
    public Transform clerkSpawnPoint;
    public Transform herbalistSpawnPoint;
    public Transform farmingSpawnPoint;
    public Transform doctorSpawnPoint;
    public Transform carpenterSpawnPoint;
    public Transform fishingSpawnPoint;
    public Transform woodcutterSpawnPoint;
    public Transform blacksmithSpawnPoint;
    public Transform bartenderSpawnPoint;

    private void Start()
    {
        if (PlayerJobManager.Instance == null)
        {
            Debug.LogWarning("PlayerJobManager Instance not found. Spawning will not be applied.");
            return;
        }

        // NOTE: Enabling/disabling of minigame objects is handled by JobSceneSetup on this same object
        // (or another GameObject in this scene). See JobSceneSetup.cs.

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player GameObject with tag 'Player' not found in scene!");
            return;
        }

        Transform targetSpawn = null;
        switch (PlayerJobManager.Instance.currentJob)
        {
            case PlayerJobManager.Job.Clerk:
                targetSpawn = clerkSpawnPoint;
                break;
            case PlayerJobManager.Job.Herbalist:
                targetSpawn = herbalistSpawnPoint;
                break;
            case PlayerJobManager.Job.Farming:
                targetSpawn = farmingSpawnPoint;
                break;
            case PlayerJobManager.Job.Doctor:
                targetSpawn = doctorSpawnPoint;
                break;
            case PlayerJobManager.Job.Carpenter:
                targetSpawn = carpenterSpawnPoint;
                break;
            case PlayerJobManager.Job.Fishing:
                targetSpawn = fishingSpawnPoint;
                break;
            case PlayerJobManager.Job.Woodcutter:
                targetSpawn = woodcutterSpawnPoint;
                break;
            case PlayerJobManager.Job.Blacksmith:
                targetSpawn = blacksmithSpawnPoint;
                break;
            case PlayerJobManager.Job.Bartender:
                targetSpawn = bartenderSpawnPoint;
                break;
        }

        if (targetSpawn != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = targetSpawn.position;
            player.transform.rotation = targetSpawn.rotation;

            if (cc != null) cc.enabled = true;

            Debug.Log("Teleported player to job spawn point for job: " + PlayerJobManager.Instance.currentJob);
        }
        else
        {
            Debug.LogWarning("No spawn point defined for job: " + PlayerJobManager.Instance.currentJob);
        }
    }
}
