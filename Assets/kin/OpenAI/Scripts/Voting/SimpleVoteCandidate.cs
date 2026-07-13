// Assets/kin/OpenAI/Scripts/Voting/SimpleVoteCandidate.cs

using UnityEngine;

public class SimpleVoteCandidate : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private string candidateName = "NPC";

    [Header("Role Source")]
    [Tooltip("If assigned, this script reads Werewolf/Villager from PlayerRole automatically.")]
    [SerializeField] private PlayerRole playerRole;

    [Tooltip("Used only when PlayerRole is not assigned.")]
    [SerializeField] private bool isWerewolf;

    [Header("State")]
    [SerializeField] private bool isEliminated;

    public string CandidateName => candidateName;
    public bool IsEliminated => isEliminated;

    public bool IsWerewolf
    {
        get
        {
            if (playerRole != null)
                return playerRole.currentRole == PlayerRole.Role.Werewolf;

            return isWerewolf;
        }
    }

    private void Reset()
    {
        candidateName = gameObject.name;
        playerRole = GetComponent<PlayerRole>();
    }

    private void Awake()
    {
        if (playerRole == null)
            playerRole = GetComponent<PlayerRole>();
    }

    public void Configure(string displayName, PlayerRole roleSource)
    {
        candidateName = string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName.Trim();
        playerRole = roleSource;
    }

    public void Eliminate()
    {
        isEliminated = true;
        gameObject.SetActive(false);
    }
}
