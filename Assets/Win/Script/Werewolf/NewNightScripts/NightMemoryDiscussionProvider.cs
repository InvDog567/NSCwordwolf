using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bridge that translates Night Memories into string context
/// that can optionally be appended to the AI's prompt or logic 
/// without modifying the core ChatManager.
/// </summary>
public class NightMemoryDiscussionProvider : MonoBehaviour
{
    public static NightMemoryDiscussionProvider Instance;

    private void Awake()
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

    /// <summary>
    /// Returns a plain text summary of everything the given NPC saw during the last night.
    /// You can call this from your AI scripts right before prompt generation.
    /// Example: "Last night I was at the Graveyard. I saw NPC 3 in a suspicious area."
    /// </summary>
    public string GetFormattedMemories(int npcIndex)
    {
        if (NightMemoryBank.Instance == null) return "";

        List<NightMemory> memories = NightMemoryBank.Instance.GetMemoriesForNpc(npcIndex);
        if (memories.Count == 0) return "";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Night Memory Data:");

        foreach (var m in memories)
        {
            string timeStr = m.timeOfNight < 0.3f ? "early in the night" : (m.timeOfNight > 0.7f ? "late in the night" : "in the middle of the night");
            string suspStr = m.wasInSuspiciousArea ? "(Suspicious Location)" : "(Safe Location)";
            
            sb.AppendLine($"- Saw NPC {m.observedNpcIndex} at the {m.areaName} {timeStr}. {suspStr}");
        }

        return sb.ToString();
    }
}
