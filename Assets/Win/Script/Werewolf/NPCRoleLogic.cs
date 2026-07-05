using UnityEngine;
using System.Collections.Generic;

public class NPCRoleLogic : MonoBehaviour
{
    public static NPCRoleLogic Instance;

    void Awake()
    {
        Instance = this;
    }

    // Call this at the start of each night
    // after player has had their turn
    public void RunNightActions()
    {
        if (GameManager.Instance == null) return;

        for (int i = 0; i < GameManager.Instance.savedNPCRoles.Count; i++)
        {
            if (!GameManager.Instance.npcAlive[i]) continue;

            PlayerRole.Role role = GameManager.Instance.savedNPCRoles[i];

            switch (role)
            {
                case PlayerRole.Role.Seer:
                    NPCSeer(i);
                    break;
                case PlayerRole.Role.Doctor:
                    NPCDoctor(i);
                    break;
                case PlayerRole.Role.Jailer:
                    NPCJailer(i);
                    break;
                case PlayerRole.Role.Arsonist:
                    NPCArsonist(i);
                    break;
                case PlayerRole.Role.Witch:
                    NPCWitch(i);
                    break;
            }
        }
    }

    // Call this at the start of each day
    public void RunDayActions()
    {
        if (GameManager.Instance == null) return;

        for (int i = 0; i < GameManager.Instance.savedNPCRoles.Count; i++)
        {
            if (!GameManager.Instance.npcAlive[i]) continue;

            PlayerRole.Role role = GameManager.Instance.savedNPCRoles[i];

            switch (role)
            {
                case PlayerRole.Role.Vigilante:
                    NPCVigilante(i);
                    break;
                case PlayerRole.Role.Gunner:
                    NPCGunner(i);
                    break;
            }
        }
    }

    // SEER — reveals a random unknown NPC role
    void NPCSeer(int seerIndex)
    {
        List<int> unknowns = new List<int>();
        for (int i = 0; i < GameManager.Instance.savedNPCRoles.Count; i++)
        {
            if (i == seerIndex) continue;
            if (!GameManager.Instance.npcAlive[i]) continue;
            unknowns.Add(i);
        }

        if (unknowns.Count == 0) return;

        int target = unknowns[Random.Range(0, unknowns.Count)];
        PlayerRole.Role revealed =
            GameManager.Instance.savedNPCRoles[target];

        Debug.Log("[SEER NPC " + seerIndex + "] saw NPC " +
                  target + " is " + revealed);
    }

    // DOCTOR — protect a random living non-wolf NPC
    void NPCDoctor(int doctorIndex)
    {
        // Only protect if not already protecting someone
        if (GameManager.Instance.doctorProtectedIndex != -1) return;

        List<int> targets = new List<int>();
        for (int i = 0; i < GameManager.Instance.savedNPCRoles.Count; i++)
        {
            if (i == doctorIndex) continue;
            if (!GameManager.Instance.npcAlive[i]) continue;
            if (GameManager.Instance.savedNPCRoles[i] ==
                PlayerRole.Role.Werewolf) continue;
            targets.Add(i);
        }

        // Also consider protecting player
        bool protectPlayer = Random.Range(0, targets.Count + 1) == targets.Count;

        if (protectPlayer)
        {
            GameManager.Instance.doctorProtectedPlayer = true;
            Debug.Log("[DOCTOR NPC " + doctorIndex + "] protecting PLAYER");
        }
        else if (targets.Count > 0)
        {
            int target = targets[Random.Range(0, targets.Count)];
            GameManager.Instance.doctorProtectedIndex = target;
            Debug.Log("[DOCTOR NPC " + doctorIndex +
                      "] protecting NPC " + target);
        }
    }

    // JAILER — jail a random NPC (not player, not wolf ally)
    void NPCJailer(int jailerIndex)
    {
        // Already jailed someone this cycle
        if (GameManager.Instance.jailedNPCIndex != -1) return;

        List<int> targets = new List<int>();
        for (int i = 0; i < GameManager.Instance.savedNPCRoles.Count; i++)
        {
            if (i == jailerIndex) continue;
            if (!GameManager.Instance.npcAlive[i]) continue;
            if (!GameManager.Instance.CanBeJailed(i)) continue;
            targets.Add(i);
        }

        if (targets.Count == 0) return;

        int target = targets[Random.Range(0, targets.Count)];
        GameManager.Instance.SetJailTarget(target);
        Debug.Log("[JAILER NPC " + jailerIndex +
                  "] jailed NPC " + target);
    }

    // ARSONIST — 75% douse random NPC, 25% ignite all doused
    void NPCArsonist(int arsonistIndex)
    {
        bool anyDoused = false;
        for (int i = 0; i < GameManager.Instance.npcDoused.Count; i++)
        {
            if (GameManager.Instance.npcDoused[i])
            {
                anyDoused = true;
                break;
            }
        }

        // If no one doused yet, always douse
        // If someone doused, 75% douse 25% ignite
        float roll = Random.Range(0f, 1f);
        bool ignite = anyDoused && roll > 0.75f;

        if (ignite)
        {
            GameManager.Instance.IgniteAllDoused();
            Debug.Log("[ARSONIST NPC " + arsonistIndex + "] IGNITED all doused");
        }
        else
        {
            List<int> targets = new List<int>();
            for (int i = 0; i < GameManager.Instance.savedNPCRoles.Count; i++)
            {
                if (i == arsonistIndex) continue;
                if (!GameManager.Instance.npcAlive[i]) continue;
                if (GameManager.Instance.npcDoused[i]) continue;
                targets.Add(i);
            }

            // Also consider dousing player
            bool dousePlayer = !GameManager.Instance.playerDoused &&
                               Random.Range(0, targets.Count + 1) == targets.Count;

            if (dousePlayer)
            {
                GameManager.Instance.playerDoused = true;
                Debug.Log("[ARSONIST NPC " + arsonistIndex +
                          "] doused PLAYER");
            }
            else if (targets.Count > 0)
            {
                int target = targets[Random.Range(0, targets.Count)];
                GameManager.Instance.npcDoused[target] = true;
                Debug.Log("[ARSONIST NPC " + arsonistIndex +
                          "] doused NPC " + target);
            }
        }
    }

    // WITCH — protect a random NPC each night until potion is used
    void NPCWitch(int witchIndex)
    {
        if (GameManager.Instance.witchUsedProtect) return;

        List<int> targets = new List<int>();
        for (int i = 0; i < GameManager.Instance.savedNPCRoles.Count; i++)
        {
            if (i == witchIndex) continue;
            if (!GameManager.Instance.npcAlive[i]) continue;
            targets.Add(i);
        }

        if (targets.Count == 0) return;

        int target = targets[Random.Range(0, targets.Count)];

        // Set protection — if this NPC gets attacked tonight
        // the protection triggers and potion is consumed
        GameManager.Instance.doctorProtectedIndex = target;

        Debug.Log("[WITCH NPC " + witchIndex +
                  "] protecting NPC " + target + " tonight");
    }

    // VIGILANTE — scan a random NPC each day
    // shoots only if target is wolf or arsonist
    void NPCVigilante(int vigilanteIndex)
    {
        if (GameManager.Instance.vigilanteUsedShoot) return;

        List<int> targets = new List<int>();
        for (int i = 0; i < GameManager.Instance.savedNPCRoles.Count; i++)
        {
            if (i == vigilanteIndex) continue;
            if (!GameManager.Instance.npcAlive[i]) continue;
            targets.Add(i);
        }

        if (targets.Count == 0) return;

        int target = targets[Random.Range(0, targets.Count)];
        PlayerRole.Role scanned =
            GameManager.Instance.savedNPCRoles[target];

        Debug.Log("[VIGILANTE NPC " + vigilanteIndex +
                  "] scanned NPC " + target + " = " + scanned);

        // Shoot if wolf or arsonist
        bool isThreat = scanned == PlayerRole.Role.Werewolf ||
                        scanned == PlayerRole.Role.Arsonist;

        if (isThreat)
        {
            GameManager.Instance.npcAlive[target] = false;
            GameManager.Instance.vigilanteUsedShoot = true;
            Debug.Log("[VIGILANTE NPC " + vigilanteIndex +
                      "] SHOT NPC " + target + " (" + scanned + ")");
        }
    }

    // GUNNER — suggestion: hold off until round 3+
    // then shoot the most suspicious NPC
    // for now shoots a random non-villager if they have info
    // AI can override this later
    void NPCGunner(int gunnerIndex)
    {
        if (GameManager.Instance.gunnerBulletsLeft <= 0) return;
        if (GameManager.Instance.gunnerShotThisDay) return;

        // For now gunner does nothing without AI
        // AI will decide who to shoot based on chat
        Debug.Log("[GUNNER NPC " + gunnerIndex +
                  "] waiting for AI to decide who to shoot");
    }
}