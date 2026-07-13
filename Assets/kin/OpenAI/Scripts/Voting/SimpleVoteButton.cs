// Assets/kin/OpenAI/Scripts/Voting/SimpleVoteButton.cs

using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SimpleVoteButton : MonoBehaviour
{
    [SerializeField] private SimpleVoteManager voteManager;
    [SerializeField] private SimpleVoteCandidate candidate;
    [SerializeField] private TMP_Text label;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(SubmitVote);
    }

    private void Start()
    {
        RefreshLabel();
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(SubmitVote);
    }

    public void Setup(SimpleVoteManager manager, SimpleVoteCandidate voteCandidate)
    {
        voteManager = manager;
        candidate = voteCandidate;
        RefreshLabel();
    }

    private void SubmitVote()
    {
        if (voteManager == null)
        {
            Debug.LogError("[SimpleVoteButton] Vote Manager is missing.");
            return;
        }

        if (candidate == null)
        {
            Debug.LogError("[SimpleVoteButton] Candidate is missing.");
            return;
        }

        voteManager.SubmitVote(candidate);
    }

    private void RefreshLabel()
    {
        if (label != null && candidate != null)
            label.text = candidate.CandidateName;
    }
}
