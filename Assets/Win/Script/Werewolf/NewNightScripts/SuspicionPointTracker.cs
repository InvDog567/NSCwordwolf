using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Persists Suspicion Points (SP) gained from night memories.
/// This works ALONGSIDE the existing NPCVoteLogic system, allowing
/// night actions to influence voting without rewriting the voting script.
/// </summary>
public class SuspicionPointTracker : MonoBehaviour
{
    public static SuspicionPointTracker Instance;

    // Maps npcIndex -> accumulated Suspicion Points from night
    private Dictionary<int, float> nightSuspicionPoints = new Dictionary<int, float>();

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

    public void ClearPoints()
    {
        nightSuspicionPoints.Clear();
    }

    public void AddPoints(int npcIndex, float amount)
    {
        if (nightSuspicionPoints.ContainsKey(npcIndex))
        {
            nightSuspicionPoints[npcIndex] += amount;
        }
        else
        {
            nightSuspicionPoints[npcIndex] = amount;
        }
    }

    public float GetPoints(int npcIndex)
    {
        if (nightSuspicionPoints.TryGetValue(npcIndex, out float points))
        {
            return points;
        }
        return 0f;
    }

    /// <summary>
    /// Processes all memories in the bank and converts them to Suspicion Points.
    /// Call this right after the night ends.
    /// </summary>
    public void ProcessMemoriesToSP()
    {
        if (NightMemoryBank.Instance == null) return;

        ClearPoints();

        List<NightMemory> allMemories = NightMemoryBank.Instance.GetAllMemories();
        
        foreach (var mem in allMemories)
        {
            if (mem.wasInSuspiciousArea)
            {
                // Multiply the suspicion value of the area by a factor (e.g., 5 SP per suspicious point)
                float spGain = mem.areaSuspicionValue * 5f;
                AddPoints(mem.observedNpcIndex, spGain);
            }
        }
    }
}
