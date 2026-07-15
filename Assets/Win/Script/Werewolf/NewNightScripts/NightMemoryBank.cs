using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Persistent storage for Night Memories that lasts across scenes.
/// </summary>
public class NightMemoryBank : MonoBehaviour
{
    public static NightMemoryBank Instance;

    private List<NightMemory> memories = new List<NightMemory>();

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

    public void Clear()
    {
        memories.Clear();
    }

    public void AddMemory(NightMemory memory)
    {
        memories.Add(memory);
    }

    public List<NightMemory> GetMemoriesForNpc(int npcIndex)
    {
        List<NightMemory> result = new List<NightMemory>();
        foreach (var m in memories)
        {
            if (m.observerNpcIndex == npcIndex)
            {
                result.Add(m);
            }
        }
        return result;
    }

    public List<NightMemory> GetAllMemories()
    {
        return memories;
    }
}
