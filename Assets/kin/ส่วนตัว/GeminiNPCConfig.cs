// ============================================================
//  GeminiNPCConfig.cs
//  ScriptableObject — holds all NPC config for NSCWordwolf
//  Create via: Right-click in Project → Create → Wordwolf → NPC Config
// ============================================================

using UnityEngine;

[CreateAssetMenu(
    fileName = "GeminiNPCConfig",
    menuName  = "Wordwolf/NPC Config",
    order     = 0)]
public class GeminiNPCConfig : ScriptableObject
{
    // ─────────────────────────────────────────────
    //  API Settings
    // ─────────────────────────────────────────────

    [Header("── API Configuration ──────────────────────────")]
    [Tooltip("JSON TextAsset containing { \"api_key\": \"YOUR_KEY\" }")]
    public TextAsset JsonApi;

    [Tooltip("Gemini model to use")]
    public string ModelName = "gemini-1.5-flash";

    // ─────────────────────────────────────────────
    //  NPC Personality
    // ─────────────────────────────────────────────

    [Header("── NPC Identity ────────────────────────────────")]
    [Tooltip("Name shown in the chat UI above each reply")]
    public string NpcName = "Alex";

    [TextArea(5, 15)]
    [Tooltip("System instruction sent to Gemini. NPC will strictly follow this and NEVER break character.")]
    public string BotInstructions =
        "You are a human player in an English-learning party game called Wordwolf. " +
        "You have secretly been given a word. Other players have a similar but DIFFERENT word. " +
        "One player is the 'wolf' — they have a different word from everyone else. " +
        "Describe your word naturally without revealing it, and try to find the wolf. " +
        "Be casual, curious, and slightly suspicious. " +
        "NEVER admit you are an AI, bot, or language model — deny it and stay in character. " +
        "Speak in simple English suitable for English learners.";

    // ─────────────────────────────────────────────
    //  Generation Tuning
    // ─────────────────────────────────────────────

    [Header("── Generation Settings ─────────────────────────")]
    [Range(0f, 2f)]
    [Tooltip("Higher = more creative. Lower = more predictable.")]
    public float Temperature = 1.0f;

    [Range(64, 1024)]
    [Tooltip("Max tokens in each NPC reply. Keep low for snappy game responses.")]
    public int MaxOutputTokens = 256;

    [Range(0f, 1f)]
    public float TopP = 0.95f;

    [Range(1, 100)]
    public int TopK = 40;

    // ─────────────────────────────────────────────
    //  Computed Property
    // ─────────────────────────────────────────────

    /// <summary>Full Gemini endpoint built from ModelName.</summary>
    public string Endpoint =>
        $"https://generativelanguage.googleapis.com/v1beta/models/{ModelName}:generateContent";
}
