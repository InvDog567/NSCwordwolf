using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Discovers all NightDestinations in the scene and provides random selections.
/// </summary>
public class NightDestinationRegistry : MonoBehaviour
{
    public static NightDestinationRegistry Instance;

    private List<NightDestination> allDestinations = new List<NightDestination>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Auto-discover all destinations in the scene
        NightDestination[] foundDestinations = FindObjectsOfType<NightDestination>();
        allDestinations.AddRange(foundDestinations);

        if (allDestinations.Count == 0)
        {
            Debug.LogWarning("NightDestinationRegistry: No NightDestination objects found in the scene.");
        }
    }

    public NightDestination GetRandomDestination()
    {
        if (allDestinations.Count == 0) return null;
        return allDestinations[Random.Range(0, allDestinations.Count)];
    }

    public NightDestination GetRandomDestinationExcluding(NightDestination excludeDest)
    {
        if (allDestinations.Count == 0) return null;
        if (allDestinations.Count == 1) return allDestinations[0];

        NightDestination chosen;
        int maxAttempts = 10;
        int attempts = 0;
        do
        {
            chosen = allDestinations[Random.Range(0, allDestinations.Count)];
            attempts++;
        } while (chosen == excludeDest && attempts < maxAttempts);

        return chosen;
    }

    public List<NightDestination> GetAllDestinations()
    {
        return allDestinations;
    }
}
