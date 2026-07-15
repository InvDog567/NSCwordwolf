using UnityEngine;

/// <summary>
/// Data class representing a single memory an NPC gathered during the Night.
/// </summary>
[System.Serializable]
public class NightMemory
{
    public int observerNpcIndex;
    public int observedNpcIndex;

    // Time normalized 0 to 1 over the night phase duration
    public float timeOfNight;

    public string areaName;
    public float areaSuspicionValue;
    public bool wasInSuspiciousArea;

    public NightMemory(int observer, int observed, float timeNormalized, string area, float suspicionValue)
    {
        observerNpcIndex = observer;
        observedNpcIndex = observed;
        timeOfNight = timeNormalized;
        areaName = area;
        areaSuspicionValue = suspicionValue;
        wasInSuspiciousArea = suspicionValue > 0f;
    }
}
