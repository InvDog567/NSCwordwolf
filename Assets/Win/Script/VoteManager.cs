using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VoteManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button npc1Button;
    public Button npc2Button;
    public Button npc3Button;

    [Header("Scenes")]
    public string villagersWinScene;
    public string werewolfWinScene;
    public string loseScene;

    void Start()
    {
        if (GameManager.Instance == null) return;

        // Hide dead NPC buttons
        if (!GameManager.Instance.npcAlive[0])
            npc1Button.gameObject.SetActive(false);

        if (!GameManager.Instance.npcAlive[1])
            npc2Button.gameObject.SetActive(false);

        if (!GameManager.Instance.npcAlive[2])
            npc3Button.gameObject.SetActive(false);
    }

    public void VoteNPC(int npcIndex)
    {
        if (GameManager.Instance == null) return;

        // Mark voted NPC as dead
        GameManager.Instance.npcAlive[npcIndex] = false;

        // Get voted role
        PlayerRole.Role votedRole =
            GameManager.Instance.savedNPCRoles[npcIndex];

        // Get player role safely
        PlayerRole.Role playerRole =
            (PlayerRole.Role)PlayerRoleRandomizer.Instance.currentRole;

        // =========================
        // IF PLAYER IS WEREWOLF
        // =========================
        if (playerRole == PlayerRole.Role.Werewolf)
        {
            SceneManager.LoadScene(werewolfWinScene);
            return;
        }

        // =========================
        // IF PLAYER IS VILLAGER / SEER
        // =========================
        if (votedRole == PlayerRole.Role.Werewolf)
        {
            // Correct vote → WIN
            SceneManager.LoadScene(villagersWinScene);
        }
        else
        {
            // Wrong vote → LOSE
            SceneManager.LoadScene(loseScene);
        }
    }
}