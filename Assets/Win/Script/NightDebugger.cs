using UnityEngine;

public class NightDebugger : MonoBehaviour
{
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
                    " | Alive: " + GameManager.Instance.npcAlive[i]);
            }
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("=== FORCING NPCWerewolfKill ===");
            GameManager.Instance.NPCWerewolfKill();
            Debug.Log("playerKilledByWolf: " +
                GameManager.Instance.playerKilledByWolf);
        }
    }
}