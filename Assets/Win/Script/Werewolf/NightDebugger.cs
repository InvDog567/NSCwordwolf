using UnityEngine;

public class NightDebugger : MonoBehaviour
{
    [Header("Debug Target NPC Index")]
    public int debugKillNPCIndex = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (GameManager.Instance == null)
            {
                Debug.Log("GameManager NULL");
                return;
            }

            Debug.Log("=== NIGHT DEBUG ===");
            Debug.Log("Player role: " + GameManager.Instance.playerRole);

            for (int i = 0; i < GameManager.Instance.savedNPCRoles.Count; i++)
            {
                Debug.Log("NPC " + i +
                    " | Role: " + GameManager.Instance.savedNPCRoles[i] +
                    " | Alive: " + GameManager.Instance.npcAlive[i] +
                    " | Doused: " + GameManager.Instance.npcDoused[i]);
            }

            Debug.Log("Doctor protected index: " +
                GameManager.Instance.doctorProtectedIndex);
            Debug.Log("Doctor protected player: " +
                GameManager.Instance.doctorProtectedPlayer);
            Debug.Log("Witch kill used: " +
                GameManager.Instance.witchUsedKill);
            Debug.Log("Witch protect used: " +
                GameManager.Instance.witchUsedProtect);
            Debug.Log("Jailed NPC: " +
                GameManager.Instance.jailedNPCIndex);
            Debug.Log("debugForceKillIndex: " +
                GameManager.Instance.debugForceKillIndex);
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            if (GameManager.Instance == null) return;
            Debug.Log("=== FORCING NPCWerewolfKill (random) ===");
            GameManager.Instance.NPCWerewolfKill();
            Debug.Log("playerKilledByWolf: " +
                GameManager.Instance.playerKilledByWolf);
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            if (GameManager.Instance == null)
            {
                Debug.Log("GameManager NULL");
                return;
            }

            int idx = debugKillNPCIndex;

            if (idx < 0 || idx >= GameManager.Instance.npcAlive.Count)
            {
                Debug.Log("Invalid NPC index: " + idx);
                return;
            }

            if (!GameManager.Instance.npcAlive[idx])
            {
                Debug.Log("NPC " + idx + " is already dead");
                return;
            }

            if (idx == GameManager.Instance.doctorProtectedIndex)
            {
                Debug.Log("NPC " + idx +
                    " targeted but DOCTOR protected them!");
                return;
            }

            GameManager.Instance.npcAlive[idx] = false;
            Debug.Log("DEBUG force killed NPC " + idx +
                " | Role: " +
                GameManager.Instance.savedNPCRoles[idx]);
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (GameManager.Instance == null) return;
            int idx = debugKillNPCIndex;
            GameManager.Instance.doctorProtectedIndex = idx;
            Debug.Log("Doctor protecting NPC " + idx +
                      " | Press F9 to test wolf kill");
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            if (GameManager.Instance == null) return;
            int idx = debugKillNPCIndex;

            if (idx == GameManager.Instance.doctorProtectedIndex)
            {
                Debug.Log("Witch targeted NPC " + idx +
                    " but DOCTOR protected them!");
                return;
            }

            GameManager.Instance.npcAlive[idx] = false;
            GameManager.Instance.witchUsedKill = true;
            Debug.Log("Witch killed NPC " + idx);
        }

        if (Input.GetKeyDown(KeyCode.F7))
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.doctorProtectedPlayer = true;
            Debug.Log("Doctor protecting PLAYER | Press F8 to test");
        }

        if (Input.GetKeyDown(KeyCode.F8))
        {
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.doctorProtectedPlayer)
            {
                Debug.Log("Wolf targeted PLAYER but DOCTOR protected them!");
                return;
            }

            GameManager.Instance.playerKilledByWolf = true;
            Debug.Log("Wolf killed the PLAYER");
        }

       if (Input.GetKeyDown(KeyCode.F9))
{
    if (GameManager.Instance == null)
    {
        Debug.Log("GameManager NULL");
        return;
    }

    Debug.Log("Setting wolf target to NPC " + debugKillNPCIndex);
    GameManager.Instance.debugForceKillIndex = debugKillNPCIndex;
    GameManager.Instance.NPCWerewolfKill();
}
    }
}