using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class VotePhaseManager : MonoBehaviour
{
    public enum Phase { Discussion, Voting, Resolved }

    [Header("Phase Timers")]
    public float discussionDuration = 30f;
    public float votingDuration = 20f;

    [Header("UI")]
    public GameObject voteButtonsContainer;
    public TMP_Text phaseText;
    public TMP_Text timerText;
    public TMP_Text voteLogText;

    [Header("Current State")]
    public Phase currentPhase = Phase.Discussion;

    private float timer;
    private bool playerHasVoted = false;
    private int playerVoteIndex = -1;
    private Dictionary<int, int> voteTally = new Dictionary<int, int>();

    [Header("Scenes")]
public string daySceneName;
public string voteExecutedSceneName;   // player voted out
public string wolfKilledSceneName;     // wolf killed player
public string arsonistKilledSceneName; // arsonist ignited player
public string villagerWinSceneName;    // player was villager-aligned, wolves eliminated
public string werewolfWinSceneName;    // player was werewolf, wolves won
public string arsonistWinSceneName;    // player was arsonist, villagers won (arsonist side wins alongside villagers)

    void Start()
    {
        if (voteButtonsContainer != null)
            voteButtonsContainer.SetActive(true);

        StartCoroutine(RunDiscussionPhase());
    }

    IEnumerator RunDiscussionPhase()
    {
        currentPhase = Phase.Discussion;
        timer = discussionDuration;

        Debug.Log("=== DISCUSSION PHASE STARTED === duration: " + discussionDuration);

        if (phaseText != null)
            phaseText.text = "Discussion Phase";

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(timer).ToString();
            yield return null;
        }

        Debug.Log("=== DISCUSSION PHASE ENDED ===");

        StartCoroutine(RunVotingPhase());
    }

    IEnumerator RunVotingPhase()
    {
        currentPhase = Phase.Voting;
        timer = votingDuration;

        Debug.Log("=== VOTING PHASE STARTED === duration: " + votingDuration);

        if (phaseText != null)
            phaseText.text = "Voting Phase";


        voteTally.Clear();
        playerHasVoted = false;
        playerVoteIndex = -1;

        if (NPCVoteLogic.Instance != null)
        {
            Debug.Log("Starting NPC votes...");
            StartCoroutine(NPCVoteLogic.Instance.RunNPCVotes(
                votingDuration, RegisterNPCVote));
        }
        else
        {
            Debug.LogWarning("NPCVoteLogic.Instance is NULL - is the script attached?");
        }

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(timer).ToString();
            yield return null;
        }

        Debug.Log("=== VOTING PHASE ENDED === Tally count: " + voteTally.Count);

        ResolveVotes();
    }

    public void PlayerVote(int npcIndex)
    {
        Debug.Log("PlayerVote called with index: " + npcIndex +
                  " | Current phase: " + currentPhase +
                  " | Already voted: " + playerHasVoted);

        if (currentPhase != Phase.Voting)
        {
            Debug.LogWarning("Vote blocked - not in Voting phase (current: " + currentPhase + ")");
            return;
        }

        if (playerHasVoted)
        {
            Debug.LogWarning("Vote blocked - player already voted");
            return;
        }

        playerHasVoted = true;
        playerVoteIndex = npcIndex;
        AddVote(npcIndex);

        Debug.Log("Player vote registered for NPC " + npcIndex);

        if (voteLogText != null)
            voteLogText.text += "\nYou voted for NPC " + npcIndex;
    }

    void RegisterNPCVote(int voterIndex, int targetIndex)
    {
        Debug.Log("NPC " + voterIndex + " voting for target " + targetIndex);
        AddVote(targetIndex);

        if (voteLogText != null)
            voteLogText.text += "\nNPC " + voterIndex +
                " voted for NPC " + targetIndex;
    }

    void AddVote(int targetIndex)
    {
        if (!voteTally.ContainsKey(targetIndex))
            voteTally[targetIndex] = 0;
        voteTally[targetIndex]++;

        Debug.Log("Vote added for " + targetIndex +
                  " | Current tally for this target: " + voteTally[targetIndex]);
    }

    void ResolveVotes()
{
    currentPhase = Phase.Resolved;

    if (voteTally.Count == 0)
    {
        if (phaseText != null)
            phaseText.text = "No votes cast, no one executed";
        StartCoroutine(GoToDayAfterDelay());
        return;
    }

    int topVotes = -1;
    foreach (var kvp in voteTally)
    {
        if (kvp.Value > topVotes)
            topVotes = kvp.Value;
    }

    List<int> topCandidates = new List<int>();
    foreach (var kvp in voteTally)
    {
        if (kvp.Value == topVotes)
            topCandidates.Add(kvp.Key);
    }

    if (topCandidates.Count > 1)
    {
        Debug.Log("Tie vote between: " + string.Join(", ", topCandidates));
        if (phaseText != null)
            phaseText.text = "Vote tied, no one executed";
        StartCoroutine(GoToDayAfterDelay());
        return;
    }

    int topIndex = topCandidates[0];

    if (topIndex == -1)
    {
        // Player was voted out
        if (phaseText != null)
            phaseText.text = "You were voted out";

        StartCoroutine(GoToSceneAfterDelay(voteExecutedSceneName));
        return;
    }

    if (phaseText != null)
        phaseText.text = "NPC " + topIndex + " was voted out";

    GameManager.Instance.npcAlive[topIndex] = false;
    Debug.Log("Voted out NPC " + topIndex +
        " | Role: " + GameManager.Instance.savedNPCRoles[topIndex]);

    ResolveOutcome();
}

void ResolveOutcome()
{
    int result = GameManager.Instance.CheckWinCondition();

    if (result == 1)
    {
        // Villagers side won
        if (GameManager.Instance.playerRole == PlayerRole.Role.Arsonist)
            StartCoroutine(GoToSceneAfterDelay(arsonistWinSceneName));
        else
            StartCoroutine(GoToSceneAfterDelay(villagerWinSceneName));
    }
    else if (result == 2)
    {
        // Wolves side won
        StartCoroutine(GoToSceneAfterDelay(werewolfWinSceneName));
    }
    else
    {
        StartCoroutine(GoToDayAfterDelay());
    }
}
IEnumerator GoToDayAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(daySceneName);
    }

    IEnumerator GoToSceneAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(sceneName);
    }
}
