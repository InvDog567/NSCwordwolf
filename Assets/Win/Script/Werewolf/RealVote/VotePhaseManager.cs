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

    [Header("Scenes")]
    public string daySceneName;
    public string loseSceneName;
    public string winSceneName;

    [Header("Current State")]
    public Phase currentPhase = Phase.Discussion;

    private float timer;
    private bool playerHasVoted = false;
    private int playerVoteIndex = -1;
    private Dictionary<int, int> voteTally = new Dictionary<int, int>();

    void Start()
    {
        if (voteButtonsContainer != null)
            voteButtonsContainer.SetActive(false);

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

        if (voteButtonsContainer != null)
        {
            voteButtonsContainer.SetActive(true);
            Debug.Log("Vote buttons container activated: " + voteButtonsContainer.activeSelf);
        }
        else
        {
            Debug.LogWarning("voteButtonsContainer is NULL - assign it in Inspector!");
        }

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
        Debug.Log("ResolveVotes called | tally count: " + voteTally.Count);

        currentPhase = Phase.Resolved;

        if (voteButtonsContainer != null)
            voteButtonsContainer.SetActive(false);

        if (voteTally.Count == 0)
        {
            Debug.Log("No votes were cast at all - nobody dies");
            if (phaseText != null)
                phaseText.text = "No votes cast, no one executed";
            StartCoroutine(GoToDayAfterDelay());
            return;
        }

        int topIndex = -1;
        int topVotes = -1;
        foreach (var kvp in voteTally)
        {
            Debug.Log("Tally entry -> NPC/Player " + kvp.Key + " : " + kvp.Value + " votes");
            if (kvp.Value > topVotes)
            {
                topVotes = kvp.Value;
                topIndex = kvp.Key;
            }
        }

        Debug.Log("Top voted index: " + topIndex + " with " + topVotes + " votes");

        if (topIndex == -1)
        {
            Debug.LogWarning("Player was voted out - not yet handled by this script!");
            StartCoroutine(GoToDayAfterDelay());
            return;
        }

        if (phaseText != null)
            phaseText.text = "NPC " + topIndex + " was voted out";

        if (topIndex < 0 || topIndex >= GameManager.Instance.npcAlive.Count)
        {
            Debug.LogError("topIndex " + topIndex + " is out of range for npcAlive list!");
            StartCoroutine(GoToDayAfterDelay());
            return;
        }

        GameManager.Instance.npcAlive[topIndex] = false;
        Debug.Log("Voted out NPC " + topIndex +
            " | Role: " + GameManager.Instance.savedNPCRoles[topIndex]);

        int result = GameManager.Instance.CheckWinCondition();
        Debug.Log("Win condition result: " + result);

        if (result == 1)
        {
            StartCoroutine(GoToSceneAfterDelay(winSceneName));
        }
        else if (result == 2)
        {
            StartCoroutine(GoToSceneAfterDelay(loseSceneName));
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