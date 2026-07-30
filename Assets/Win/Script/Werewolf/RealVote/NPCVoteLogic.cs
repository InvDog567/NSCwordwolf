using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCVoteLogic : MonoBehaviour
{
    public static NPCVoteLogic Instance;

    private Dictionary<int, Dictionary<int, float>> suspicionMap =
        new Dictionary<int, Dictionary<int, float>>();

    void Awake()
    {
        Instance = this;
        Debug.Log("NPCVoteLogic Instance set");
    }

    public IEnumerator RunNPCVotes(float votingDuration,
        System.Action<int, int> onVoteCast)
    {
        List<int> aliveNPCs = new List<int>();
        for (int i = 0; i < GameManager.Instance.npcAlive.Count; i++)
        {
            if (GameManager.Instance.npcAlive[i])
                aliveNPCs.Add(i);
        }

        Debug.Log("Alive NPCs for voting: " + aliveNPCs.Count);

        BuildSuspicion(aliveNPCs);

        float safeWindow = votingDuration * 0.8f;

        foreach (int npcIndex in aliveNPCs)
        {
            float delay = Random.Range(1f, safeWindow);
            Debug.Log("NPC " + npcIndex + " will vote in " + delay + " seconds");
            StartCoroutine(CastVoteAfterDelay(npcIndex, delay, onVoteCast));
        }

        yield return null;
    }

    IEnumerator CastVoteAfterDelay(int npcIndex, float delay,
        System.Action<int, int> onVoteCast)
    {
        yield return new WaitForSeconds(delay);

        if (!GameManager.Instance.npcAlive[npcIndex])
        {
            Debug.Log("NPC " + npcIndex + " died before voting, skipping");
            yield break;
        }

        int target = PickVoteTarget(npcIndex);
        Debug.Log("NPC " + npcIndex + " picked target: " + target);

        if (target != -2)
            onVoteCast?.Invoke(npcIndex, target);
        else
            Debug.LogWarning("NPC " + npcIndex + " had no valid target");
    }

    void BuildSuspicion(List<int> aliveNPCs)
    {
        suspicionMap.Clear();

        foreach (int voter in aliveNPCs)
        {
            Dictionary<int, float> scores = new Dictionary<int, float>();

            foreach (int target in aliveNPCs)
            {
                if (target == voter) continue;

                float baseScore = Random.Range(0f, 1f);

                PlayerRole.Role voterRole =
                    GameManager.Instance.savedNPCRoles[voter];

                if (voterRole == PlayerRole.Role.Seer &&
                    GameManager.Instance.savedNPCRoles[target] ==
                    PlayerRole.Role.Werewolf)
                {
                    baseScore += 5f;
                }

                if (target < GameManager.Instance.npcDoused.Count &&
                    GameManager.Instance.npcDoused[target])
                {
                    baseScore += 1f;
                }

                if (GameManager.Instance.witnessedMurderers.Contains(target))
                {
                    baseScore += 5f;
                    Debug.Log($"[VOTE LOGIC] Voter {voter} knows {target} is a murderer!");
                }

                // Apply dynamic suspicion modifiers from the discussion phase
                if (DiscussionManager.DynamicSuspicionModifiers.TryGetValue(target, out float discussionMod))
                {
                    baseScore += discussionMod;
                    Debug.Log($"[VOTE LOGIC] Discussion modifier of {discussionMod:+0.0;-0.0} applied to target {target} by voter {voter}");
                }

                scores[target] = baseScore;
            }

            if (!GameManager.Instance.playerIsJailed)
            {
                float playerScore = Random.Range(0f, 1f);

                PlayerRole.Role voterRole =
                    GameManager.Instance.savedNPCRoles[voter];

                if (voterRole == PlayerRole.Role.Seer &&
                    GameManager.Instance.playerRole ==
                    PlayerRole.Role.Werewolf)
                {
                    playerScore += 5f;
                }

                if (GameManager.Instance.playerDoused)
                    playerScore += 1f;

                if (GameManager.Instance.witnessedMurderers.Contains(-1))
                {
                    playerScore += 5f;
                    Debug.Log($"[VOTE LOGIC] Voter {voter} knows PLAYER is a murderer!");
                }

                // Apply dynamic suspicion modifiers from the discussion phase
                if (DiscussionManager.DynamicSuspicionModifiers.TryGetValue(-1, out float playerDiscussionMod))
                {
                    playerScore += playerDiscussionMod;
                    Debug.Log($"[VOTE LOGIC] Discussion modifier of {playerDiscussionMod:+0.0;-0.0} applied to Player by voter {voter}");
                }

                scores[-1] = playerScore;
            }

            suspicionMap[voter] = scores;
        }

        Debug.Log("Suspicion map built for " + suspicionMap.Count + " voters");
    }

    int PickVoteTarget(int voterIndex)
    {
        if (!suspicionMap.ContainsKey(voterIndex))
        {
            Debug.LogWarning("No suspicion data for voter " + voterIndex);
            return -2;
        }

        Dictionary<int, float> scores = suspicionMap[voterIndex];
        if (scores.Count == 0)
        {
            Debug.LogWarning("Voter " + voterIndex + " has no scored targets");
            return -2;
        }

        int bestTarget = -2;
        float bestScore = -1f;

        foreach (var kvp in scores)
        {
            if (kvp.Value > bestScore)
            {
                bestScore = kvp.Value;
                bestTarget = kvp.Key;
            }
        }

        return bestTarget;
    }
}