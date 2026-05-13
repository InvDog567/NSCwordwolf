using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class VoteManager : MonoBehaviour
{
    [Header("NPC Vote Buttons (index 0-11)")]
    public List<Button> npcButtons =
        new List<Button>();

    [Header("Scenes")]
    public string villagersWinScene;
    public string werewolfWinScene;
    public string daySceneName;
    public string loseScene;

    void Start()
    {
        if (GameManager.Instance == null) return;

        // If player was killed by wolf at night
        if (GameManager.Instance.playerKilledByWolf)
        {
            SceneManager.LoadScene(loseScene);
            return;
        }

        // Hide buttons for dead NPCs
        for (int i = 0; i < npcButtons.Count; i++)
        {
            if (npcButtons[i] == null) continue;

            if (i >= GameManager.Instance.npcAlive.Count ||
                !GameManager.Instance.npcAlive[i])
            {
                npcButtons[i].gameObject.SetActive(false);
            }
        }

        // Check win condition before voting starts
        int result = GameManager.Instance.CheckWinCondition();
        if (result == 1)
        {
            SceneManager.LoadScene(villagersWinScene);
            return;
        }
        if (result == 2)
        {
            SceneManager.LoadScene(werewolfWinScene);
            return;
        }
    }

    public void VoteNPC(int npcIndex)
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.npcAlive[npcIndex] = false;

        Debug.Log("Voted out NPC " + npcIndex +
                  " who was " +
                  GameManager.Instance.savedNPCRoles[npcIndex]);

        int result = GameManager.Instance.CheckWinCondition();

        if (result == 1)
        {
            SceneManager.LoadScene(villagersWinScene);
        }
        else if (result == 2)
        {
            SceneManager.LoadScene(werewolfWinScene);
        }
        else
        {
            // Game continues — back to day
            SceneManager.LoadScene(daySceneName);
        }
    }
}