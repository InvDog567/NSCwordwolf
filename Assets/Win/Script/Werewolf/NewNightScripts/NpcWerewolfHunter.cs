using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Makes NPC Werewolves physically hunt a target when the player is not a Werewolf.
/// Replaces the instant background kill.
/// </summary>
public class NpcWerewolfHunter : MonoBehaviour
{
    [Header("Hunt Settings")]
    public float killRange = 2f;
    public float huntCheckInterval = 0.2f;

    private NavMeshAgent hunterAgent;
    private Transform victimTransform;
    private int victimIndex = -1;
    private bool killExecuted = false;

    private Coroutine huntRoutine;

    private void Start()
    {
        if (GameManager.Instance == null) return;

        // If player is a Werewolf, NPCs do NOT hunt
        if (GameManager.Instance.playerRole == PlayerRole.Role.Werewolf)
        {
            enabled = false;
            return;
        }

        // 1. Immediately disable default background random kill
        GameManager.Instance.wolfKillDoneThisNight = true;

        SelectVictim();

        if (victimIndex != -1 && victimTransform != null)
        {
            // Logically kill them at the start of the night so they die in the vote scene regardless of being touched
            GameManager.Instance.npcAlive[victimIndex] = false;

            FindActiveHunter();
            if (hunterAgent != null)
            {
                huntRoutine = StartCoroutine(HuntRoutine());
            }
        }
    }

    private void SelectVictim()
    {
        List<int> validTargets = new List<int>();
        for (int i = 0; i < GameManager.Instance.savedNPCRoles.Count; i++)
        {
            // Do not target dead NPCs, jailed NPCs, or werewolves
            if (GameManager.Instance.npcAlive[i] && 
                GameManager.Instance.savedNPCRoles[i] != PlayerRole.Role.Werewolf &&
                i != GameManager.Instance.jailedNPCIndex)
            {
                validTargets.Add(i);
            }
        }

        if (validTargets.Count > 0)
        {
            victimIndex = validTargets[Random.Range(0, validTargets.Count)];
            
            // Find victim's transform in the scene
            PlayerRole[] roles = FindObjectsOfType<PlayerRole>();
            foreach (var pr in roles)
            {
                if (pr.npcIndex == victimIndex && pr.gameObject.activeInHierarchy)
                {
                    victimTransform = pr.transform;
                    break;
                }
            }
        }
    }

    private void FindActiveHunter()
    {
        List<NavMeshAgent> werewolfAgents = new List<NavMeshAgent>();
        PlayerRole[] roles = FindObjectsOfType<PlayerRole>();
        foreach (var pr in roles)
        {
            if (pr.gameObject.activeInHierarchy && pr.npcIndex != -1 && GameManager.Instance.savedNPCRoles[pr.npcIndex] == PlayerRole.Role.Werewolf)
            {
                NavMeshAgent agent = pr.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    werewolfAgents.Add(agent);
                }
            }
        }

        // Only choose ONE werewolf to hunt, others roam normally
        if (werewolfAgents.Count > 0)
        {
            hunterAgent = werewolfAgents[Random.Range(0, werewolfAgents.Count)];
            
            // Disable normal night roaming for the selected hunter
            var nightBehavior = hunterAgent.GetComponent<NpcNightBehavior>();
            if (nightBehavior != null) nightBehavior.enabled = false;
        }
    }

    private IEnumerator HuntRoutine()
    {
        while (!killExecuted && victimTransform != null && hunterAgent != null && hunterAgent.isOnNavMesh)
        {
            // Move toward victim at normal walk speed (no speed boost)
            hunterAgent.SetDestination(victimTransform.position);

            // Kill instantly when within range — no stopping, no waiting
            if (Vector3.Distance(hunterAgent.transform.position, victimTransform.position) <= killRange)
            {
                ExecuteKill();
                yield break;
            }
            yield return new WaitForSeconds(huntCheckInterval);
        }
    }

    private void ExecuteKill()
    {
        if (killExecuted) return;
        killExecuted = true;

        if (GameManager.Instance != null && victimIndex != -1)
        {
            // 1. Check protections
            bool isJailed = (victimIndex == GameManager.Instance.jailedNPCIndex);
            bool isProtectedByDoctor = (victimIndex == GameManager.Instance.doctorProtectedIndex);
            bool isProtectedByWitch = false;

            if (victimIndex == GameManager.Instance.doctorProtectedIndex)
            {
                // Verify if Witch protection is active and consume the potion
                for (int w = 0; w < GameManager.Instance.savedNPCRoles.Count; w++)
                {
                    if (GameManager.Instance.savedNPCRoles[w] == PlayerRole.Role.Witch &&
                        GameManager.Instance.npcAlive[w] &&
                        !GameManager.Instance.witchUsedProtect)
                    {
                        isProtectedByWitch = true;
                        GameManager.Instance.witchUsedProtect = true;
                        Debug.Log($"[WITCH POTION] Witch NPC {w} saved NPC {victimIndex} from physical attack!");
                        break;
                    }
                }
            }

            if (isJailed)
            {
                Debug.Log($"[PHYSICAL ATTACK] Attack failed: NPC {victimIndex} is in Jail!");
            }
            else if (isProtectedByDoctor || isProtectedByWitch)
            {
                Debug.Log($"[PHYSICAL ATTACK] Attack blocked: NPC {victimIndex} was protected!");
            }
            else
            {
                // Kill target logically (handled in vote scene later)
                GameManager.Instance.npcAlive[victimIndex] = false;

                Debug.Log($"[PHYSICAL ATTACK] Success: NPC {victimIndex} marked as killed for vote scene!");
                
                // --- WITNESS CHECK ---
                int killerIndex = hunterAgent.GetComponent<PlayerRole>().npcIndex;
                if (killerIndex != -1)
                {
                    PlayerRole[] allRoles = FindObjectsOfType<PlayerRole>();
                    foreach (var pr in allRoles)
                    {
                        if (pr.npcIndex == killerIndex || pr.npcIndex == victimIndex || pr.isDead || pr.npcIndex == -1) continue;
                        
                        var nightBehavior = pr.GetComponent<NpcNightBehavior>();
                        if (nightBehavior != null && pr.gameObject.activeInHierarchy)
                        {
                            float dist = Vector3.Distance(pr.transform.position, victimTransform.position);
                            if (dist <= nightBehavior.detectionRadius)
                            {
                                // Simple line of sight check
                                bool canSee = true;
                                if (nightBehavior.requireLineOfSight)
                                {
                                    Vector3 dir = (victimTransform.position - pr.transform.position).normalized;
                                    if (Physics.Raycast(pr.transform.position + Vector3.up, dir, dist, nightBehavior.sightObstacles))
                                    {
                                        canSee = false;
                                    }
                                }
                                
                                if (canSee)
                                {
                                    if (!GameManager.Instance.witnessedMurderers.Contains(killerIndex))
                                    {
                                        GameManager.Instance.witnessedMurderers.Add(killerIndex);
                                    }
                                    Debug.Log($"[WITNESS] NPC {pr.npcIndex} witnessed NPC {killerIndex} murdering {victimIndex}!");
                                }
                            }
                        }
                    }
                }
                // ---------------------
            }
        }

        // Return hunter back to normal night behavior, but restricted to safe zones
        if (hunterAgent != null)
        {
            hunterAgent.isStopped = false;
            var nightBehavior = hunterAgent.GetComponent<NpcNightBehavior>();
            if (nightBehavior != null)
            {
                nightBehavior.forceSafeDestinationsOnly = true;
                nightBehavior.enabled = true; // Resume normal roaming
            }
        }
    }

    private void OnDisable()
    {
        if (huntRoutine != null) StopCoroutine(huntRoutine);
    }
}
