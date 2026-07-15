using UnityEngine;

/// <summary>
/// Place this on an empty GameObject to mark it as a valid destination for NPCs during the Night Phase.
/// </summary>
public class NightDestination : MonoBehaviour
{
    [Header("Location Info")]
    [Tooltip("The logical name of the area (e.g., 'Graveyard', 'Streets', 'Forest')")]
    public string areaName = "Unknown";
    
    [Tooltip("0 = Safe, Higher values = More Suspicious")]
    public float suspicionValue = 0f;

    [Header("Wait Parameters")]
    public float waitTimeMin = 3f;
    public float waitTimeMax = 8f;

    [Header("Randomization")]
    [Tooltip("NPC will pick a random point within this radius around the object.")]
    public float radius = 2f;

    // Optional: Draw a gizmo in the editor to see destinations easily
    private void OnDrawGizmos()
    {
        Gizmos.color = suspicionValue > 0f ? new Color(1f, 0f, 0f, 0.3f) : new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, radius);
    }

    public Vector3 GetRandomPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        return transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
    }
}
