using UnityEngine;

public class PlayerRole : MonoBehaviour
{
    public enum Role
    {
        Villager,
        Seer,
        Werewolf
    }

    [Header("Role")]
    public Role currentRole =
        Role.Villager;

    [Header("Settings")]
    public bool isPlayer;

    [Header("NPC")]
    public int npcIndex;

    [HideInInspector]
    public bool isDead = false;

    void Start()
    {
        // PLAYER
        if (isPlayer)
        {
            if (PlayerRoleRandomizer.Instance != null)
            {
                currentRole =
                    (Role)
                    PlayerRoleRandomizer
                    .Instance.currentRole;
            }
        }

        // NPC
        else
        {
            if (GameManager.Instance != null)
            {
                if (npcIndex >= 0 &&
                    npcIndex <
                    GameManager.Instance
                    .savedNPCRoles.Count)
                {
                    currentRole =
                        GameManager.Instance
                        .savedNPCRoles[npcIndex];
                }
            }
        }
    }

    public bool HasNightAbility()
    {
        return currentRole ==
               Role.Seer ||

               currentRole ==
               Role.Werewolf;
    }
}