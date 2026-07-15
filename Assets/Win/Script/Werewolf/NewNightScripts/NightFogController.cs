using UnityEngine;

/// <summary>
/// Controls the fog settings during the Night Phase, including Werewolf vision enhancements.
/// </summary>
public class NightFogController : MonoBehaviour
{
    [Header("Fog Settings")]
    public float normalFogStart = 0f;
    public float normalFogEnd = 18f;
    
    [Header("Werewolf Settings")]
    public float werewolfFogStart = 5f;
    public float werewolfFogEnd = 50f;

    public Color fogColor = new Color(0.05f, 0.05f, 0.08f);

    private bool originalFogState;
    private float originalFogStart;
    private float originalFogEnd;
    private Color originalFogColor;

    private void OnEnable()
    {
        // Save original settings
        originalFogState = RenderSettings.fog;
        originalFogStart = RenderSettings.fogStartDistance;
        originalFogEnd = RenderSettings.fogEndDistance;
        originalFogColor = RenderSettings.fogColor;

        // Apply night settings
        RenderSettings.fog = true;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = FogMode.Linear;

        // Check if player is a Werewolf
        bool isWerewolf = false;
        if (GameManager.Instance != null && GameManager.Instance.playerRole == PlayerRole.Role.Werewolf)
        {
            isWerewolf = true;
        }

        if (isWerewolf)
        {
            RenderSettings.fogStartDistance = werewolfFogStart;
            RenderSettings.fogEndDistance = werewolfFogEnd;
        }
        else
        {
            RenderSettings.fogStartDistance = normalFogStart;
            RenderSettings.fogEndDistance = normalFogEnd;
        }
    }

    private void OnDisable()
    {
        // Restore original settings
        RenderSettings.fog = originalFogState;
        RenderSettings.fogStartDistance = originalFogStart;
        RenderSettings.fogEndDistance = originalFogEnd;
        RenderSettings.fogColor = originalFogColor;
    }
}
