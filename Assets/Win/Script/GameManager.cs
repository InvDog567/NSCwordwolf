using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("NPC Characters")]
    public List<PlayerRole> npcs;

    // PLAYER ROLE ONLY
    [HideInInspector]
    public PlayerRole.Role playerRole;

    // SAVED NPC ROLES
    [HideInInspector]
    public List<PlayerRole.Role> savedNPCRoles =
        new List<PlayerRole.Role>();

    // NPC ALIVE STATES
    [HideInInspector]
    public List<bool> npcAlive =
        new List<bool>() { true, true, true };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (savedNPCRoles.Count == 0)
        {
            AssignRoles();
        }
    }

    void AssignRoles()
    {
        // CHECK RANDOMIZER
        if (PlayerRoleRandomizer.Instance == null)
        {
            Debug.LogError(
                "PlayerRoleRandomizer is NULL!");

            return;
        }

        // SAVE PLAYER ROLE
        playerRole =
            (PlayerRole.Role)
            PlayerRoleRandomizer.Instance.currentRole;

        // REMAINING ROLES
        List<PlayerRole.Role> remainingRoles =
            new List<PlayerRole.Role>()
        {
            PlayerRole.Role.Werewolf,
            PlayerRole.Role.Seer,
            PlayerRole.Role.Villager,
            PlayerRole.Role.Villager
        };

        // REMOVE PLAYER ROLE
        remainingRoles.Remove(playerRole);

        // SHUFFLE
        for (int i = 0; i < remainingRoles.Count; i++)
        {
            PlayerRole.Role temp =
                remainingRoles[i];

            int randomIndex =
                Random.Range(
                    i,
                    remainingRoles.Count);

            remainingRoles[i] =
                remainingRoles[randomIndex];

            remainingRoles[randomIndex] =
                temp;
        }

        savedNPCRoles.Clear();

        // ASSIGN NPC ROLES
        for (int i = 0; i < npcs.Count; i++)
        {
            // NULL CHECK
            if (npcs[i] == null)
            {
                Debug.LogError(
                    "NPC at index "
                    + i +
                    " is NULL!");

                continue;
            }

            npcs[i].currentRole =
                remainingRoles[i];

            savedNPCRoles.Add(
                remainingRoles[i]);

            Debug.Log(
                npcs[i].name +
                " is " +
                remainingRoles[i]);
        }
    }

    // NPC WEREWOLF AUTO KILL
    public void NPCWerewolfKill()
    {
        // IF PLAYER IS WEREWOLF,
        // DON'T AUTO KILL
        if (playerRole ==
            PlayerRole.Role.Werewolf)
        {
            return;
        }

        int wolfIndex = -1;

        // FIND NPC WEREWOLF
        for (int i = 0; i < savedNPCRoles.Count; i++)
        {
            if (savedNPCRoles[i] ==
                PlayerRole.Role.Werewolf)
            {
                wolfIndex = i;

                break;
            }
        }

        // NO WEREWOLF FOUND
        if (wolfIndex == -1)
        {
            Debug.LogError(
                "No NPC Werewolf found!");

            return;
        }

        // POSSIBLE VICTIMS
        List<int> victims =
            new List<int>();

        for (int i = 0; i < npcAlive.Count; i++)
        {
            if (i != wolfIndex &&
                npcAlive[i])
            {
                victims.Add(i);
            }
        }

        // KILL RANDOM NPC
        if (victims.Count > 0)
        {
            int victim =
                victims[
                    Random.Range(
                        0,
                        victims.Count)];

            npcAlive[victim] = false;

            Debug.Log(
                "NPC Werewolf killed NPC "
                + victim);
        }
    }
}