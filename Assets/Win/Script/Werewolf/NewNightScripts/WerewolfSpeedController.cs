using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Handles increasing speed for Werewolves during the night,
/// and dynamically normalizing NPC Werewolf speed when near the player.
/// </summary>
public class WerewolfSpeedController : MonoBehaviour
{
    [Header("Speed Settings")]
    public float werewolfSpeedMultiplier = 2f;
    public float normalizeDistance = 20f;
    public float speedTransitionRate = 2f;

    private Player playerController;
    private float originalPlayerSpeed;
    private bool playerIsWerewolf;

    // Track NPC agents and their original night speeds
    private class NpcSpeedData
    {
        public NavMeshAgent agent;
        public float baseNightSpeed;
        public float targetSpeed;
    }
    
    private List<NpcSpeedData> werewolfNpcs = new List<NpcSpeedData>();
    private Transform playerTransform;

    private void Start()
    {
        if (GameManager.Instance == null) return;

        // Find Player
        PlayerRole[] roles = FindObjectsOfType<PlayerRole>();
        foreach (var pr in roles)
        {
            if (pr.npcIndex == -1) // Player
            {
                playerController = pr.GetComponent<Player>();
                playerTransform = pr.transform;
                playerIsWerewolf = GameManager.Instance.playerRole == PlayerRole.Role.Werewolf;

                if (playerController != null)
                {
                    originalPlayerSpeed = playerController.speed;
                    if (playerIsWerewolf)
                    {
                        playerController.speed *= werewolfSpeedMultiplier;
                    }
                }
            }
            else if (GameManager.Instance.savedNPCRoles[pr.npcIndex] == PlayerRole.Role.Werewolf)
            {
                // NPC Werewolf
                NavMeshAgent agent = pr.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    werewolfNpcs.Add(new NpcSpeedData
                    {
                        agent = agent,
                        // We assume NpcNightBehavior has already set its nightWalkSpeed by now
                        baseNightSpeed = agent.speed, 
                        targetSpeed = agent.speed * werewolfSpeedMultiplier
                    });
                }
            }
        }
    }

    private void Update()
    {
        if (playerTransform == null || werewolfNpcs.Count == 0) return;

        foreach (var data in werewolfNpcs)
        {
            if (data.agent == null) continue;

            var nightBehavior = data.agent.GetComponent<NpcNightBehavior>();
            if (nightBehavior == null) continue;

            if (nightBehavior.forceSafeDestinationsOnly)
            {
                // Post-kill safe roaming: standard walking speed
                data.targetSpeed = data.baseNightSpeed;
            }
            else if (!nightBehavior.enabled)
            {
                // Active hunter:
                float distToPlayer = Vector3.Distance(data.agent.transform.position, playerTransform.position);
                if (distToPlayer <= normalizeDistance)
                {
                    // Near player: slightly faster than normal (1.3x) to catch victim without looking crazy fast
                    data.targetSpeed = data.baseNightSpeed * 1.3f;
                }
                else
                {
                    // Far from player: full speed multiplier
                    data.targetSpeed = data.baseNightSpeed * werewolfSpeedMultiplier;
                }
            }
            else
            {
                // Normal werewolf roaming: just standard walking speed
                data.targetSpeed = data.baseNightSpeed;
            }

            // Smooth transition
            data.agent.speed = Mathf.Lerp(data.agent.speed, data.targetSpeed, Time.deltaTime * speedTransitionRate);
        }
    }

    private void OnDisable()
    {
        // Restore player speed
        if (playerController != null)
        {
            playerController.speed = originalPlayerSpeed;
        }

        // Restore NPC speeds
        foreach (var data in werewolfNpcs)
        {
            if (data.agent != null)
            {
                data.agent.speed = data.baseNightSpeed;
            }
        }
    }
}
