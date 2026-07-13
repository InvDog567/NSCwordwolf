// Assets/kin/OpenAI/Scripts/NPCMemory.cs

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class NPCMemoryEntry
{
    [TextArea(1, 3)] public string description;
    public float timeRecorded;

    public NPCMemoryEntry(string description)
    {
        this.description = description;
        timeRecorded = Time.time;
    }
}

public class NPCMemory : MonoBehaviour
{
    [Header("Memory Settings")]
    [SerializeField] private int maximumMemories = 8;
    [SerializeField] private float duplicateCooldownSeconds = 15f;

    [Header("Read Only At Runtime")]
    [SerializeField] private List<NPCMemoryEntry> memories = new List<NPCMemoryEntry>();

    public bool HasMemories => memories.Count > 0;

    public void Remember(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return;

        string cleanDescription = description.Trim();
        if (WasRecentlyRemembered(cleanDescription))
            return;

        memories.Add(new NPCMemoryEntry(cleanDescription));

        while (memories.Count > Mathf.Max(1, maximumMemories))
            memories.RemoveAt(0);
    }

    public string BuildPromptMemory()
    {
        var summary = new StringBuilder();

        foreach (NPCMemoryEntry memory in memories)
            summary.AppendLine($"- {memory.description}");

        return summary.ToString().Trim();
    }

    public void ClearMemories()
    {
        memories.Clear();
    }

    private bool WasRecentlyRemembered(string description)
    {
        foreach (NPCMemoryEntry memory in memories)
        {
            bool isSameEvent = string.Equals(memory.description, description, StringComparison.Ordinal);
            bool isRecent = Time.time - memory.timeRecorded < duplicateCooldownSeconds;

            if (isSameEvent && isRecent)
                return true;
        }

        return false;
    }
}
