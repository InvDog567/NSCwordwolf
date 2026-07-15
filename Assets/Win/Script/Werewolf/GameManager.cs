using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Role Counts (total including player)")]
    public int villagerCount = 4;
    public int seerCount = 1;
    public int werewolfCount = 2;
    public int gunnerCount = 1;
    public int doctorCount = 1;
    public int jailerCount = 1;
    public int arsonistCount = 1;
    public int witchCount = 1;
    public int vigilanteCount = 1;

    [HideInInspector] public PlayerRole.Role playerRole;
    [HideInInspector] public List<PlayerRole.Role> savedNPCRoles = new List<PlayerRole.Role>();
    [HideInInspector] public List<bool> npcAlive = new List<bool>();
    [HideInInspector] public List<bool> npcDoused = new List<bool>();
    [HideInInspector] public bool playerKilledByWolf = false;
    [HideInInspector] public bool playerDoused = false;
    [HideInInspector] public int doctorProtectedIndex = -1;
    [HideInInspector] public bool doctorProtectedPlayer = false;

    [HideInInspector] public int jailedNPCIndex = -1;
    [HideInInspector] public bool playerIsJailed = false;
    [HideInInspector] public bool jailerUsedBullet = false;
    [HideInInspector] public int lastJailedNPCIndex = -1;
    [HideInInspector] public int jailCooldownNightsLeft = 0;

    [HideInInspector] public int gunnerBulletsLeft = 2;
    [HideInInspector] public bool gunnerRevealed = false;

    // ADDED
    [HideInInspector] public bool gunnerShotThisDay = false;

    [HideInInspector] public bool witchUsedKill = false;
    [HideInInspector] public bool witchUsedProtect = false;
    [HideInInspector] public bool isFirstNight = true;

    [HideInInspector] public bool vigilanteUsedShoot = false;
    [HideInInspector] public bool vigilanteUsedReveal = false;
    [HideInInspector] public bool vigilanteActedThisDay = false;

    [HideInInspector] public int debugForceKillIndex = -1;
    [HideInInspector] public bool wolfKillDoneThisNight = false;
    [HideInInspector] public bool playerKilledByArsonist = false;
    [HideInInspector] public List<int> witnessedMurderers = new List<int>();

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
            return;
        }
    }

    public void AssignRoles()
    {
        if (PlayerRoleRandomizer.Instance == null)
        {
            Debug.LogError("PlayerRoleRandomizer missing!");
            return;
        }

        if (!PlayerRoleRandomizer.Instance.roleReady)
        {
            Debug.LogError("AssignRoles called before role was ready!");
            return;
        }

        playerRole =
            (PlayerRole.Role)PlayerRoleRandomizer.Instance.currentRole;

        List<PlayerRole.Role> allRoles = new List<PlayerRole.Role>();

        for (int i = 0; i < villagerCount; i++)
            allRoles.Add(PlayerRole.Role.Villager);
        for (int i = 0; i < seerCount; i++)
            allRoles.Add(PlayerRole.Role.Seer);
        for (int i = 0; i < werewolfCount; i++)
            allRoles.Add(PlayerRole.Role.Werewolf);
        for (int i = 0; i < gunnerCount; i++)
            allRoles.Add(PlayerRole.Role.Gunner);
        for (int i = 0; i < doctorCount; i++)
            allRoles.Add(PlayerRole.Role.Doctor);
        for (int i = 0; i < jailerCount; i++)
            allRoles.Add(PlayerRole.Role.Jailer);
        for (int i = 0; i < arsonistCount; i++)
            allRoles.Add(PlayerRole.Role.Arsonist);
        for (int i = 0; i < witchCount; i++)
            allRoles.Add(PlayerRole.Role.Witch);
        for (int i = 0; i < vigilanteCount; i++)
            allRoles.Add(PlayerRole.Role.Vigilante);

        allRoles.Remove(playerRole);

        for (int i = 0; i < allRoles.Count; i++)
        {
            int randomIndex = Random.Range(i, allRoles.Count);
            PlayerRole.Role temp = allRoles[i];
            allRoles[i] = allRoles[randomIndex];
            allRoles[randomIndex] = temp;
        }

        savedNPCRoles.Clear();
        npcAlive.Clear();
        npcDoused.Clear();
        playerKilledByWolf = false;
        playerDoused = false;
        playerKilledByArsonist = false;
        doctorProtectedIndex = -1;
        doctorProtectedPlayer = false;
        jailedNPCIndex = -1;
        playerIsJailed = false;
        jailerUsedBullet = false;
        lastJailedNPCIndex = -1;
        jailCooldownNightsLeft = 0;
        gunnerBulletsLeft = 2;
        gunnerRevealed = false;

        // ADDED
        gunnerShotThisDay = false;

        witchUsedKill = false;
        witchUsedProtect = false;
        isFirstNight = true;
        vigilanteUsedShoot = false;
        vigilanteUsedReveal = false;
        vigilanteActedThisDay = false;
        debugForceKillIndex = -1;
        wolfKillDoneThisNight = false;

        for (int i = 0; i < allRoles.Count; i++)
        {
            savedNPCRoles.Add(allRoles[i]);
            npcAlive.Add(true);
            npcDoused.Add(false);
            Debug.Log("NPC " + i + " | Role: " + allRoles[i]);
        }

        Debug.Log("Player role: " + playerRole);
        Debug.Log("Total NPCs: " + savedNPCRoles.Count);

        SyncLoadedNPCChatRoles();
    }

    private void SyncLoadedNPCChatRoles()
    {
        NPCChatController[] chatControllers = FindObjectsOfType<NPCChatController>(true);

        foreach (NPCChatController chatController in chatControllers)
            chatController.SyncAssignedRoleFromGameManager();
    }

        public void ResetNightState()
    {
        doctorProtectedIndex = -1;
        doctorProtectedPlayer = false;
        playerKilledByWolf = false;
        wolfKillDoneThisNight = false;

        if (jailCooldownNightsLeft > 0)
        {
            jailCooldownNightsLeft--;
            Debug.Log("Jail cooldown remaining: " + jailCooldownNightsLeft);
        }

        Debug.Log("Night started | jailedNPCIndex: " + jailedNPCIndex);
    }

    public void ResetDayState()
    {
        jailedNPCIndex = -1;
        playerIsJailed = false;
        vigilanteActedThisDay = false;
        isFirstNight = false;

        // ADDED
        gunnerShotThisDay = false;

        Debug.Log("Day started, jail slot cleared");
    }

    public bool CanBeJailed(int npcIndex)
    {
        if (lastJailedNPCIndex == npcIndex &&
            jailCooldownNightsLeft > 0)
            return false;

        return true;
    }

    public void SetJailTarget(int npcIndex)
    {
        jailedNPCIndex = npcIndex;
        lastJailedNPCIndex = npcIndex;
        jailCooldownNightsLeft = 2;
        Debug.Log("Jailed NPC " + npcIndex +
                  " | Cooldown: " + jailCooldownNightsLeft);
    }

    public void IgniteAllDoused()
{
    for (int i = 0; i < npcDoused.Count; i++)
    {
        if (npcDoused[i] && npcAlive[i])
        {
            npcAlive[i] = false;
            Debug.Log("Arsonist ignited NPC " + i);
        }
    }

    if (playerDoused)
    {
        playerKilledByArsonist = true;
        Debug.Log("Arsonist ignited the PLAYER");
    }
}

    public void NPCWerewolfKill()
    {
        if (playerRole == PlayerRole.Role.Werewolf)
        {
            Debug.Log("Player is werewolf, NPC wolves follow player");
            return;
        }

        if (wolfKillDoneThisNight)
        {
            Debug.Log("Wolf already killed this night, skipping");
            return;
        }

        bool anyWolfAlive = false;
        for (int i = 0; i < npcAlive.Count; i++)
        {
            if (npcAlive[i] &&
                savedNPCRoles[i] == PlayerRole.Role.Werewolf)
            {
                anyWolfAlive = true;
                break;
            }
        }

        if (!anyWolfAlive)
        {
            Debug.Log("No NPC werewolves alive");
            return;
        }

        // Debug force kill
        if (debugForceKillIndex >= 0)
        {
            int idx = debugForceKillIndex;
            debugForceKillIndex = -1;

            Debug.Log("=== NPC Wolf force targeting NPC " + idx + " ===");

            if (idx >= npcAlive.Count || !npcAlive[idx])
            {
                Debug.Log("Invalid or dead target");
                return;
            }

            if (savedNPCRoles[idx] == PlayerRole.Role.Werewolf)
            {
                Debug.Log("Cannot kill wolf ally");
                return;
            }

            if (idx == jailedNPCIndex)
            {
                Debug.Log("Target is jailed, wolf cannot kill");
                return;
            }

            if (idx == doctorProtectedIndex)
            {
                Debug.Log("NPC " + idx +
                    " attacked by wolf but DOCTOR saved them!");
                wolfKillDoneThisNight = true;
                return;
            }

            npcAlive[idx] = false;
            wolfKillDoneThisNight = true;
            Debug.Log("NPC Wolf killed NPC " + idx +
                      " | Role: " + savedNPCRoles[idx]);
            return;
        }

        // Normal random kill
        List<int> npcVictims = new List<int>();
        for (int i = 0; i < npcAlive.Count; i++)
        {
            if (!npcAlive[i]) continue;
            if (savedNPCRoles[i] == PlayerRole.Role.Werewolf) continue;
            if (i == jailedNPCIndex) continue;
            npcVictims.Add(i);
        }

        bool playerCanBeVictim = !playerIsJailed;
        int totalPool = npcVictims.Count + (playerCanBeVictim ? 1 : 0);

        if (totalPool == 0)
        {
            Debug.Log("No victims available");
            return;
        }

        int roll = Random.Range(0, totalPool);

        if (playerCanBeVictim && roll == totalPool - 1)
        {
            if (doctorProtectedPlayer)
            {
                Debug.Log("Player attacked but Doctor saved them!");
                wolfKillDoneThisNight = true;
            }
            else
            {
                playerKilledByWolf = true;
                wolfKillDoneThisNight = true;
                Debug.Log("NPC Werewolf killed the PLAYER");
            }
        }
        else
        {
            int victimIndex = npcVictims[roll];

            if (victimIndex == doctorProtectedIndex)
            {
                Debug.Log("NPC " + victimIndex + " attacked but saved!");

                // If protection came from witch, consume potion
                // Check by seeing if witch NPC is alive and hasnt used protect
                for (int w = 0; w < GameManager.Instance.savedNPCRoles.Count; w++)
                {
                    if (GameManager.Instance.savedNPCRoles[w] ==
                        PlayerRole.Role.Witch &&
                        GameManager.Instance.npcAlive[w] &&
                        !GameManager.Instance.witchUsedProtect)
                    {
                        GameManager.Instance.witchUsedProtect = true;
                        Debug.Log("Witch potion consumed protecting NPC " + victimIndex);
                        break;
                    }
                }

                wolfKillDoneThisNight = true;
            }
            else
            {
                npcAlive[victimIndex] = false;
                wolfKillDoneThisNight = true;
                Debug.Log("NPC Werewolf killed NPC " + victimIndex +
                          " | Role: " + savedNPCRoles[victimIndex]);
            }
        }
    }

    public int CheckWinCondition()
    {
        int aliveWolves = 0;
        int aliveVillagers = 0;

        for (int i = 0; i < npcAlive.Count; i++)
        {
            if (!npcAlive[i]) continue;

            if (savedNPCRoles[i] == PlayerRole.Role.Werewolf)
                aliveWolves++;
            else
                aliveVillagers++;
        }

        if (playerRole == PlayerRole.Role.Werewolf)
            aliveWolves++;
        else
            aliveVillagers++;

        Debug.Log("Alive wolves: " + aliveWolves +
                  " | Alive villagers: " + aliveVillagers);

        if (aliveWolves == 0) return 1;
        if (aliveWolves >= aliveVillagers) return 2;
        return 0;
    }
}
