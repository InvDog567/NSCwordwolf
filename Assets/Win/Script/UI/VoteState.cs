using UnityEngine;
using UnityEngine.UI;

public class VoteState : MonoBehaviour
{
    public int npcIndex;
    public Button voteButton;
    public GameObject xMarkOverlay;
    public VotePhaseManager votePhaseManager;

    void Update()
    {
        if (GameManager.Instance == null) return;

        bool alive = GameManager.Instance.npcAlive[npcIndex];
        bool votingPhase = votePhaseManager != null &&
            votePhaseManager.currentPhase == VotePhaseManager.Phase.Voting;

        voteButton.interactable = alive && votingPhase;

        if (xMarkOverlay != null)
            xMarkOverlay.SetActive(!alive);
    }
}