using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VotePhaseManager : MonoBehaviour
{
    public enum Phase
    {
        Discussion,
        Voting,
        Resolved
    }

    [Header("Timers")]
    public float discussionDuration = 30f;
    public float votingDuration = 20f;
    public float resultDelay = 3f;

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
    // Value = amount of votes.
    // Target -1 means the player.
    private readonly Dictionary<int, int> voteTally =
        new Dictionary<int, int>();

    private Button[] voteButtons;

    private void Start()
    {
        if (discussionManager == null)
            discussionManager = GetComponent<DiscussionManager>();

        SetupVoteButtons();
        ShowVoteMarker(-1);
        ClearVoteLog();

        StartCoroutine(RunPhases());
    }

    private void SetupVoteButtons()
    {
        if (voteButtonsContainer == null)
            return;

        // Keep the container visible.
        voteButtonsContainer.SetActive(true);

        voteButtons = voteButtonsContainer.GetComponentsInChildren<Button>(
            true
        );

        // Keep buttons visually enabled from the start.
        foreach (Button button in voteButtons)
        {
            if (button != null)
                button.interactable = true;
        }
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
        // Buttons remain clickable, but votes only count
        // during the voting phase.
        if (currentPhase != Phase.Voting)
        {
            Debug.Log(
                "Vote ignored because voting has not started."
            );

            ClearButtonSelection();
            return;
        }

    private bool IsValidNPCIndex(int npcIndex)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null.");
            return false;
        }

        if (GameManager.Instance.npcAlive == null)
        {
            Debug.LogError("npcAlive is null.");
            return false;
        }

        if (npcIndex < 0 || npcIndex >= GameManager.Instance.npcAlive.Count)
        {
            Debug.LogError(
                $"NPC index {npcIndex} is invalid. " +
                $"npcAlive contains {GameManager.Instance.npcAlive.Count} NPCs."
            );

            return false;
        }

        return true;
    }

        // Clicking the same suspect again changes nothing.
        if (playerVoteIndex == npcIndex)
        {
            ClearButtonSelection();
            return;
        }

        // Remove the player's old vote.
        if (playerVoteIndex >= 0)
            RemoveVote(playerVoteIndex);

        playerVoteIndex = npcIndex;

        AddVote(playerVoteIndex);
        ShowVoteMarker(playerVoteIndex);

        SetVoteLog(
            $"You voted for NPC {playerVoteIndex}.\n" +
            "You can change your vote before time runs out."
        );

        ClearButtonSelection();

        Debug.Log(
            $"Player voted for NPC {playerVoteIndex}"
        );
    }

    private void RegisterNPCVote(int voterIndex, int targetIndex)
    {
        // Ignore delayed NPC votes after voting ends.
        if (currentPhase != Phase.Voting || votesResolved)
            return;

        // -1 represents voting for the player.
        if (targetIndex != -1 && !IsValidNPCIndex(targetIndex))
        {
            Debug.LogWarning(
                $"NPC {voterIndex} chose invalid target {targetIndex}"
            );

            return;
        }

        AddVote(targetIndex);

        string targetName = targetIndex == -1
            ? "the player"
            : $"NPC {targetIndex}";

        AddVoteLog(
            $"NPC {voterIndex} voted for {targetName}."
        );

        Debug.Log(
            $"NPC {voterIndex} voted for {targetName}"
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

            StartCoroutine(LoadSceneAfterDelay(daySceneName));
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

            StartCoroutine(LoadSceneAfterDelay(daySceneName));
            return;
        }

        int executedIndex = topCandidates[0];

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
                $"Cannot execute invalid NPC index {npcIndex}"
            );

            StartCoroutine(LoadSceneAfterDelay(daySceneName));
            return;
        }

        GameManager.Instance.npcAlive[npcIndex] = false;

        SetPhaseText($"NPC {npcIndex} was voted out");

        Debug.Log(
            $"NPC {npcIndex} was voted out. " +
            $"Role: {GameManager.Instance.savedNPCRoles[npcIndex]}"
        );

        ResolveOutcome();
    }

    private void ResolveOutcome()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null.");
            StartCoroutine(LoadSceneAfterDelay(daySceneName));
            return;
        }

        int result = GameManager.Instance.CheckWinCondition();

        if (result == 1)
        {
            string sceneName =
                GameManager.Instance.playerRole ==
                PlayerRole.Role.Arsonist
                    ? arsonistWinSceneName
                    : villagerWinSceneName;

            StartCoroutine(LoadSceneAfterDelay(sceneName));
        }
        else if (result == 2)
        {
            StartCoroutine(
                LoadSceneAfterDelay(werewolfWinSceneName)
            );
        }
        else
        {
            StartCoroutine(LoadSceneAfterDelay(daySceneName));
        }
    }

    private bool IsValidNPCIndex(int npcIndex)
    {
        return GameManager.Instance != null &&
               GameManager.Instance.npcAlive != null &&
               npcIndex >= 0 &&
               npcIndex < GameManager.Instance.npcAlive.Count;
    }

    private void ShowVoteMarker(int selectedIndex)
    {
        if (voteMarkers == null)
            return;

        for (int i = 0; i < voteMarkers.Length; i++)
        {
            if (voteMarkers[i] != null)
                voteMarkers[i].SetActive(i == selectedIndex);
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
                : $"NPC {vote.Key}";

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