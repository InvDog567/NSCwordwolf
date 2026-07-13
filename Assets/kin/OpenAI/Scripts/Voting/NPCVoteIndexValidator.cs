// Assets/kin/OpenAI/Scripts/Voting/NPCVoteIndexValidator.cs

using System.Collections.Generic;
using UnityEngine;

public class NPCVoteIndexValidator : MonoBehaviour
{
    [SerializeField] private bool validateOnStart = true;

    private void Start()
    {
        if (validateOnStart)
            ValidateIndexes();
    }

    [ContextMenu("Validate NPC Vote Indexes")]
    public void ValidateIndexes()
    {
        PlayerRole[] roles = FindObjectsOfType<PlayerRole>(true);
        Dictionary<int, List<PlayerRole>> byIndex = new Dictionary<int, List<PlayerRole>>();

        foreach (PlayerRole role in roles)
        {
            if (role == null || role.isPlayer)
                continue;

            if (role.npcIndex < 0)
            {
                Debug.LogWarning($"[NPCVoteIndexValidator] {role.name} has npcIndex < 0. Set a unique index in PlayerRole.");
                continue;
            }

            if (!byIndex.ContainsKey(role.npcIndex))
                byIndex.Add(role.npcIndex, new List<PlayerRole>());

            byIndex[role.npcIndex].Add(role);
        }

        foreach (KeyValuePair<int, List<PlayerRole>> pair in byIndex)
        {
            if (pair.Value.Count <= 1)
                continue;

            string names = string.Join(", ", pair.Value.ConvertAll(role => role.name));
            Debug.LogError($"[NPCVoteIndexValidator] Duplicate npcIndex {pair.Key}: {names}. If this index dies, all of these NPCs will disappear.");
        }

        Debug.Log($"[NPCVoteIndexValidator] Checked {roles.Length} PlayerRole objects.");
    }
}
