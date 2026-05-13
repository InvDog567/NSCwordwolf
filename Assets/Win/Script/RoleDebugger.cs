using UnityEngine;

public class RoleDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (GameManager.Instance == null)
            {
                Debug.Log("GameManager is NULL");
                return;
            }

            Debug.Log("=== ROLE DEBUG ===");
            Debug.Log("Player Role: " + GameManager.Instance.playerRole);
            Debug.Log("savedNPCRoles count: " + GameManager.Instance.savedNPCRoles.Count);
            Debug.Log("npcAlive count: " + GameManager.Instance.npcAlive.Count);

            for (int i = 0; i < GameManager.Instance.savedNPCRoles.Count; i++)
            {
                Debug.Log("NPC " + i + " | Role: " + GameManager.Instance.savedNPCRoles[i] + " | Alive: " + GameManager.Instance.npcAlive[i]);
            }
        }
    }
}