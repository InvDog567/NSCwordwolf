using UnityEngine;

public class PlayerRole : MonoBehaviour
{
    public enum Role
    {
        Villager,
        Seer,
        Werewolf,
        Gunner,
        Doctor,
        Jailer
    }

    [Header("Role")]
    public Role currentRole = Role.Villager;

    [Header("Player")]
    public bool isPlayer;

    [Header("NPC Index (set manually in Inspector)")]
    public int npcIndex = -1;

    [HideInInspector]
    public bool isDead = false;

    void Start()
    {
        if (isPlayer)
        {
            if (PlayerRoleRandomizer.Instance != null)
            {
                currentRole =
                    (Role)PlayerRoleRandomizer.Instance.currentRole;
            }
        }
        else
        {
            if (GameManager.Instance != null &&
                npcIndex >= 0 &&
                npcIndex < GameManager.Instance.savedNPCRoles.Count)
            {
                currentRole =
                    GameManager.Instance.savedNPCRoles[npcIndex];

                isDead =
                    !GameManager.Instance.npcAlive[npcIndex];

                if (isDead)
                    gameObject.SetActive(false);
            }
        }
    }

    public bool HasNightAbility()
    {
        return currentRole == Role.Seer ||
               currentRole == Role.Werewolf ||
               currentRole == Role.Doctor ||
               currentRole == Role.Jailer;
    }
}