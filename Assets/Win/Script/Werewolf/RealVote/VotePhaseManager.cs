using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class VotePhaseManager : MonoBehaviour
{
    public enum Phase
    {
        Discussion,
        Voting,
        Resolved
    }

    [Header("Phase Timers")]
    [Min(0f)] public float discussionDuration = 30f;
    [Min(0f)] public float votingDuration = 20f;
    [Min(0f)] public float resultDelay = 3f;

    [Header("UI")]
    public GameObject voteButtonsContainer;
    public TMP_Text phaseText;
    public TMP_Text timerText;
    public TMP_Text voteLogText;

    [Header("Vote Markers")]
    [Tooltip("Element 0 = NPC 0, Element 1 = NPC 1, etc.")]
    public GameObject[] voteMarkers;

    [Header("Managers")]
    public DiscussionManager discussionManager;

    [Header("API Key")]
    [Tooltip("The JSON file containing { \"api_key\": \"...\" }. This lets the Vote scene start discussion even when no OpenAI Manager is already loaded.")]
    public TextAsset apiKeyFile;

    [Header("Current State")]
    public Phase currentPhase = Phase.Discussion;

    [Header("Scenes")]
    public string daySceneName;
    public string voteExecutedSceneName;
    public string wolfKilledSceneName;
    public string arsonistKilledSceneName;
    public string villagerWinSceneName;
    public string werewolfWinSceneName;
    public string arsonistWinSceneName;

    private float timer;
    private int playerVoteIndex = -1;
    private bool votesResolved;

    // Key = target index.
    // Value = number of votes.
    // Target -1 represents the player.
    private readonly Dictionary<int, int> voteTally =
        new Dictionary<int, int>();

    private void Start()
    {
        // Time.timeScale persists between scenes. A previously closed settings
        // panel must not leave the vote timer frozen in the discussion phase.
        Time.timeScale = 1f;

        if (apiKeyFile != null)
            OpenAIManager.EnsureInstance(apiKeyFile);
        else if (OpenAIManager.Instance == null)
            Debug.LogError("[VotePhaseManager] API key file is not assigned. Assign secret.json to Api Key File.");

        if (discussionManager == null)
            discussionManager = GetComponent<DiscussionManager>();

        // Make sure all suspects have an npcAlive entry.
        SynchronizeNPCAliveList();

        // Keep the voting UI visible.
        if (voteButtonsContainer != null)
            voteButtonsContainer.SetActive(true);

        ShowVoteMarker(-1);
        ClearVoteLog();

        StartCoroutine(RunPhases());
    }

    private void SynchronizeNPCAliveList()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "GameManager.Instance is null. " +
                "VotePhaseManager cannot prepare NPC data."
            );

            return;
        }

        if (GameManager.Instance.npcAlive == null)
        {
            Debug.LogError(
                "GameManager npcAlive list is null. " +
                "Initialize it inside GameManager."
            );

            return;
        }

        if (GameManager.Instance.savedNPCRoles == null)
        {
            Debug.LogError(
                "GameManager savedNPCRoles list is null."
            );

            return;
        }

        int requiredNPCCount =
            GameManager.Instance.savedNPCRoles.Count;

        // Add alive entries for NPCs that are missing.
        while (GameManager.Instance.npcAlive.Count < requiredNPCCount)
        {
            GameManager.Instance.npcAlive.Add(true);
        }

        Debug.Log(
            $"NPC data prepared. Roles: {requiredNPCCount}, " +
            $"Alive entries: {GameManager.Instance.npcAlive.Count}"
        );
    }

    private IEnumerator RunPhases()
    {
        yield return RunDiscussionPhase();
        yield return RunVotingPhase();
    }

    private IEnumerator RunDiscussionPhase()
    {
        currentPhase = Phase.Discussion;
        timer = discussionDuration;

        SetPhaseText("Discussion Phase");
        ShowVoteMarker(-1);

        if (discussionManager != null)
            discussionManager.BeginDiscussion();

        yield return RunTimer();

        if (discussionManager != null)
            discussionManager.StopDiscussion();
    }

    private IEnumerator RunVotingPhase()
    {
        currentPhase = Phase.Voting;
        timer = votingDuration;
        votesResolved = false;

        voteTally.Clear();
        playerVoteIndex = -1;

        ShowVoteMarker(-1);
        ClearVoteLog();

        SetPhaseText("Voting Phase");
        AddVoteLog("Choose a suspect.");

        if (NPCVoteLogic.Instance != null)
        {
            StartCoroutine(
                NPCVoteLogic.Instance.RunNPCVotes(
                    votingDuration,
                    RegisterNPCVote
                )
            );
        }
        else
        {
            Debug.LogWarning(
                "NPCVoteLogic.Instance is null. NPCs will not vote."
            );
        }

        yield return RunTimer();

        ResolveVotes();
    }

    private IEnumerator RunTimer()
    {
        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(
                    Mathf.Max(0f, timer)
                ).ToString();
            }

            yield return null;
        }

        timer = 0f;

        if (timerText != null)
            timerText.text = "0";
    }

    public void PlayerVote(int npcIndex)
    {
        if (currentPhase != Phase.Voting)
        {
            Debug.Log("Voting is not currently active.");
            ClearButtonSelection();
            return;
        }

        if (!IsValidNPCIndex(npcIndex))
        {
            Debug.LogError(
                $"Invalid NPC vote index: {npcIndex}. " +
                $"npcAlive count: {GetNPCCount()}"
            );

            ClearButtonSelection();
            return;
        }

        if (!GameManager.Instance.npcAlive[npcIndex])
        {
            Debug.LogWarning(
                $"NPC {npcIndex} is already eliminated."
            );

            ClearButtonSelection();
            return;
        }

        // Clicking the current selection again changes nothing.
        if (playerVoteIndex == npcIndex)
        {
            ClearButtonSelection();
            return;
        }

        // Remove the player's previous vote.
        if (playerVoteIndex >= 0)
            RemoveVote(playerVoteIndex);

        // Add the new vote.
        playerVoteIndex = npcIndex;

        AddVote(playerVoteIndex);
        ShowVoteMarker(playerVoteIndex);

        SetVoteLog(
            $"You voted for {GetVoteTargetName(playerVoteIndex)}.\n" +
            "You can change your vote before time runs out."
        );

        Debug.Log(
            $"Player voted for {GetVoteTargetName(playerVoteIndex)}"
        );

        ClearButtonSelection();
    }

    private void RegisterNPCVote(int voterIndex, int targetIndex)
    {
        // Ignore NPC callbacks after voting ends.
        if (currentPhase != Phase.Voting || votesResolved)
            return;

        // -1 represents voting for the player.
        if (targetIndex != -1)
        {
            if (!IsValidNPCIndex(targetIndex))
            {
                Debug.LogWarning(
                    $"NPC {voterIndex} selected invalid target " +
                    $"{targetIndex}."
                );

                return;
            }

            if (!GameManager.Instance.npcAlive[targetIndex])
            {
                Debug.LogWarning(
                    $"NPC {voterIndex} tried to vote for eliminated " +
                    $"NPC {targetIndex}."
                );

                return;
            }
        }

        AddVote(targetIndex);

        string voterName = GetVoteTargetName(voterIndex);
        string targetName = GetVoteTargetName(targetIndex);

        AddVoteLog(
            $"{voterName} voted for {targetName}."
        );

        Debug.Log(
            $"{voterName} voted for {targetName}"
        );
    }

    private void AddVote(int targetIndex)
    {
        if (!voteTally.ContainsKey(targetIndex))
            voteTally[targetIndex] = 0;

        voteTally[targetIndex]++;
    }

    private void RemoveVote(int targetIndex)
    {
        if (!voteTally.ContainsKey(targetIndex))
            return;

        voteTally[targetIndex]--;

        if (voteTally[targetIndex] <= 0)
            voteTally.Remove(targetIndex);
    }

    private void ResolveVotes()
    {
        if (votesResolved)
            return;

        votesResolved = true;
        currentPhase = Phase.Resolved;

        if (voteTally.Count == 0)
        {
            SetPhaseText("No votes cast");
            AddVoteLog("No one was executed.");

            StartCoroutine(
                LoadSceneAfterDelay(daySceneName)
            );

            return;
        }

        int highestVotes = 0;
        List<int> topCandidates = new List<int>();

        foreach (KeyValuePair<int, int> vote in voteTally)
        {
            if (vote.Value > highestVotes)
                highestVotes = vote.Value;
        }

        foreach (KeyValuePair<int, int> vote in voteTally)
        {
            if (vote.Value == highestVotes)
                topCandidates.Add(vote.Key);
        }

        PrintVoteResults();

        if (topCandidates.Count > 1)
        {
            SetPhaseText("Vote tied");
            AddVoteLog("No one was executed.");

            StartCoroutine(
                LoadSceneAfterDelay(daySceneName)
            );

            return;
        }

        int executedIndex = topCandidates[0];

        // -1 means the player received the most votes.
        if (executedIndex == -1)
        {
            SetPhaseText("You were voted out");

            StartCoroutine(
                LoadSceneAfterDelay(voteExecutedSceneName)
            );

            return;
        }

        ExecuteNPC(executedIndex);
    }

    private void ExecuteNPC(int npcIndex)
    {
        if (!IsValidNPCIndex(npcIndex))
        {
            Debug.LogError(
                $"Cannot execute invalid NPC index {npcIndex}."
            );

            StartCoroutine(
                LoadSceneAfterDelay(daySceneName)
            );

            return;
        }

        GameManager.Instance.npcAlive[npcIndex] = false;

        SetPhaseText(
            $"{GetVoteTargetName(npcIndex)} was voted out"
        );

        string roleName = "Unknown";

        if (GameManager.Instance.savedNPCRoles != null &&
            npcIndex < GameManager.Instance.savedNPCRoles.Count)
        {
            roleName =
                GameManager.Instance.savedNPCRoles[npcIndex].ToString();
        }

        Debug.Log(
            $"{GetVoteTargetName(npcIndex)} was voted out. Role: {roleName}"
        );

        ResolveOutcome();
    }

    private void ResolveOutcome()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null.");

            StartCoroutine(
                LoadSceneAfterDelay(daySceneName)
            );

            return;
        }

        int result = GameManager.Instance.CheckWinCondition();

        if (result == 1)
        {
            bool playerIsArsonist =
                GameManager.Instance.playerRole ==
                PlayerRole.Role.Arsonist;

            string winScene = playerIsArsonist
                ? arsonistWinSceneName
                : villagerWinSceneName;

            StartCoroutine(
                LoadSceneAfterDelay(winScene)
            );
        }
        else if (result == 2)
        {
            StartCoroutine(
                LoadSceneAfterDelay(werewolfWinSceneName)
            );
        }
        else
        {
            StartCoroutine(
                LoadSceneAfterDelay(daySceneName)
            );
        }
    }

    private bool IsValidNPCIndex(int npcIndex)
    {
        return GameManager.Instance != null &&
               GameManager.Instance.npcAlive != null &&
               npcIndex >= 0 &&
               npcIndex < GameManager.Instance.npcAlive.Count;
    }

    private int GetNPCCount()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.npcAlive == null)
        {
            return 0;
        }

        return GameManager.Instance.npcAlive.Count;
    }

    private static string GetVoteTargetName(int npcIndex)
    {
        if (npcIndex == -1)
            return "Player";

        return DiscussionRoster.GetFixedNpcName(npcIndex);
    }

    private void ShowVoteMarker(int selectedIndex)
    {
        if (voteMarkers == null)
            return;

        for (int i = 0; i < voteMarkers.Length; i++)
        {
            if (voteMarkers[i] != null)
            {
                voteMarkers[i].SetActive(
                    i == selectedIndex
                );
            }
        }
    }

    private void ClearButtonSelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void SetPhaseText(string message)
    {
        if (phaseText != null)
            phaseText.text = message;
    }

    private void ClearVoteLog()
    {
        if (voteLogText != null)
            voteLogText.text = "";
    }

    private void SetVoteLog(string message)
    {
        if (voteLogText != null)
            voteLogText.text = message;
    }

    private void AddVoteLog(string message)
    {
        if (voteLogText == null)
            return;

        if (string.IsNullOrEmpty(voteLogText.text))
            voteLogText.text = message;
        else
            voteLogText.text += "\n" + message;
    }

    private void PrintVoteResults()
    {
        Debug.Log("=== FINAL VOTE RESULTS ===");

        foreach (KeyValuePair<int, int> vote in voteTally)
        {
            string targetName = vote.Key == -1
                ? "Player"
                : GetVoteTargetName(vote.Key);

            Debug.Log(
                $"{targetName}: {vote.Value} vote(s)"
            );
        }
    }

    private IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(resultDelay);

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError(
                "A scene name is missing in VotePhaseManager."
            );

            yield break;
        }

        SceneManager.LoadScene(sceneName);
    }
}
