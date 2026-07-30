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
        if (voteButton == null)
            return;

        bool hasValidNpc = GameManager.Instance != null &&
            GameManager.Instance.npcAlive != null &&
            npcIndex >= 0 &&
            npcIndex < GameManager.Instance.npcAlive.Count;

        bool alive = hasValidNpc && GameManager.Instance.npcAlive[npcIndex];
        bool votingPhase = votePhaseManager != null &&
            votePhaseManager.currentPhase == VotePhaseManager.Phase.Voting;

        voteButton.interactable = alive && votingPhase;

        if (xMarkOverlay != null)
            xMarkOverlay.SetActive(!alive);
    }
}
