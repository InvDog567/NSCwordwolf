// Assets/kin/OpenAI/Scripts/Voting/SimpleVoteManager.cs

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SimpleVoteManager : MonoBehaviour
{
    [Header("Vote Data")]
    [SerializeField] private List<SimpleVoteCandidate> candidates = new List<SimpleVoteCandidate>();

    [Header("Random NPC Votes")]
    [SerializeField] private bool includeRandomNpcVotes = true;
    [SerializeField] private bool playerVoteCounts = true;

    [Header("UI")]
    [SerializeField] private GameObject votePanel;
    [SerializeField] private TMP_Text resultText;

    [Header("Optional Scenes")]
    [SerializeField] private bool loadSceneAfterVote;
    [SerializeField] private string winSceneName;
    [SerializeField] private string loseSceneName;

    [Header("Events")]
    public UnityEvent<SimpleVoteCandidate> OnCorrectVote;
    public UnityEvent<SimpleVoteCandidate> OnWrongVote;

    private bool _hasVoted;

    public IReadOnlyList<SimpleVoteCandidate> Candidates => candidates;
    public bool HasVoted => _hasVoted;

    private void Start()
    {
        if (votePanel != null)
            votePanel.SetActive(false);
    }

    [ContextMenu("Auto Find Candidates")]
    public void AutoFindCandidates()
    {
        candidates.Clear();
        candidates.AddRange(FindObjectsOfType<SimpleVoteCandidate>(true));
    }

    public void ConfigureUI(GameObject panel, TMP_Text voteResultText)
    {
        votePanel = panel;
        resultText = voteResultText;
    }

    public void OpenVotePanel()
    {
        if (votePanel != null)
            votePanel.SetActive(true);

        if (resultText != null)
            resultText.text = "Choose who you think is the werewolf.";
    }

    public void CloseVotePanel()
    {
        if (votePanel != null)
            votePanel.SetActive(false);
    }

    public void SubmitVote(SimpleVoteCandidate playerChoice)
    {
        if (_hasVoted)
            return;

        if (playerChoice == null)
        {
            Debug.LogError("[SimpleVoteManager] Player vote candidate is missing.");
            return;
        }

        if (playerChoice.IsEliminated)
        {
            Debug.LogWarning($"[SimpleVoteManager] {playerChoice.CandidateName} is already eliminated.");
            return;
        }

        _hasVoted = true;

        SimpleVoteCandidate eliminatedCandidate = includeRandomNpcVotes
            ? RunVoteRoundWithRandomNpcVotes(playerChoice)
            : playerChoice;

        if (eliminatedCandidate == null)
        {
            Debug.LogWarning("[SimpleVoteManager] No candidate could be eliminated.");
            return;
        }

        if (eliminatedCandidate.IsWerewolf)
            HandleCorrectVote(eliminatedCandidate);
        else
            HandleWrongVote(eliminatedCandidate);
    }

    private SimpleVoteCandidate RunVoteRoundWithRandomNpcVotes(SimpleVoteCandidate playerChoice)
    {
        List<SimpleVoteCandidate> aliveCandidates = GetAliveCandidates();
        if (aliveCandidates.Count == 0)
            return null;

        Dictionary<SimpleVoteCandidate, int> voteCounts = new Dictionary<SimpleVoteCandidate, int>();
        foreach (SimpleVoteCandidate candidate in aliveCandidates)
            voteCounts[candidate] = 0;

        if (playerVoteCounts && voteCounts.ContainsKey(playerChoice))
        {
            voteCounts[playerChoice]++;
            Debug.Log($"[SimpleVoteManager] Player voted for {playerChoice.CandidateName}");
        }

        foreach (SimpleVoteCandidate npcVoter in aliveCandidates)
        {
            List<SimpleVoteCandidate> possibleTargets = new List<SimpleVoteCandidate>(aliveCandidates);
            possibleTargets.Remove(npcVoter);

            if (possibleTargets.Count == 0)
                continue;

            SimpleVoteCandidate randomTarget = possibleTargets[Random.Range(0, possibleTargets.Count)];
            voteCounts[randomTarget]++;

            Debug.Log($"[SimpleVoteManager] {npcVoter.CandidateName} randomly voted for {randomTarget.CandidateName}");
        }

        SimpleVoteCandidate topCandidate = PickHighestVoteCandidate(voteCounts);
        DebugVoteResult(voteCounts, topCandidate);
        return topCandidate;
    }

    private List<SimpleVoteCandidate> GetAliveCandidates()
    {
        List<SimpleVoteCandidate> aliveCandidates = new List<SimpleVoteCandidate>();

        foreach (SimpleVoteCandidate candidate in candidates)
        {
            if (candidate == null || candidate.IsEliminated)
                continue;

            aliveCandidates.Add(candidate);
        }

        return aliveCandidates;
    }

    private SimpleVoteCandidate PickHighestVoteCandidate(Dictionary<SimpleVoteCandidate, int> voteCounts)
    {
        int highestVotes = -1;
        List<SimpleVoteCandidate> tiedCandidates = new List<SimpleVoteCandidate>();

        foreach (KeyValuePair<SimpleVoteCandidate, int> pair in voteCounts)
        {
            if (pair.Value > highestVotes)
            {
                highestVotes = pair.Value;
                tiedCandidates.Clear();
                tiedCandidates.Add(pair.Key);
            }
            else if (pair.Value == highestVotes)
            {
                tiedCandidates.Add(pair.Key);
            }
        }

        if (tiedCandidates.Count == 0)
            return null;

        return tiedCandidates[Random.Range(0, tiedCandidates.Count)];
    }

    private void DebugVoteResult(Dictionary<SimpleVoteCandidate, int> voteCounts, SimpleVoteCandidate eliminatedCandidate)
    {
        string summary = "Vote result:";
        foreach (KeyValuePair<SimpleVoteCandidate, int> pair in voteCounts)
            summary += $"\n- {pair.Key.CandidateName}: {pair.Value}";

        if (eliminatedCandidate != null)
            summary += $"\nEliminated: {eliminatedCandidate.CandidateName}";

        Debug.Log($"[SimpleVoteManager] {summary}");

        if (resultText != null)
            resultText.text = summary;
    }

    private void HandleCorrectVote(SimpleVoteCandidate candidate)
    {
        Debug.Log($"[SimpleVoteManager] Correct vote result: {candidate.CandidateName} was the werewolf.");

        if (resultText != null)
            resultText.text += $"\nCorrect! {candidate.CandidateName} was the werewolf.";

        OnCorrectVote?.Invoke(candidate);

        if (loadSceneAfterVote && !string.IsNullOrWhiteSpace(winSceneName))
            SceneManager.LoadScene(winSceneName);
    }

    private void HandleWrongVote(SimpleVoteCandidate candidate)
    {
        Debug.Log($"[SimpleVoteManager] Wrong vote result: {candidate.CandidateName} was not the werewolf.");

        candidate.Eliminate();

        if (resultText != null)
            resultText.text += $"\nWrong. {candidate.CandidateName} was not the werewolf.";

        OnWrongVote?.Invoke(candidate);

        if (loadSceneAfterVote && !string.IsNullOrWhiteSpace(loseSceneName))
            SceneManager.LoadScene(loseSceneName);
    }

    public void ResetVoteForTesting()
    {
        _hasVoted = false;

        if (resultText != null)
            resultText.text = string.Empty;
    }
}
