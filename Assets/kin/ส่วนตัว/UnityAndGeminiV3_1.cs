// ============================================================
//  UnityAndGeminiV3.cs
//  NSCWordwolf — English Learning NPC via Gemini 1.5 Flash
//  Unity 6  |  Newtonsoft.Json  |  TextMeshPro
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class UnityAndGeminiV31 : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  INSPECTOR FIELDS
    // ─────────────────────────────────────────────

    [Header("── API Configuration ──────────────────")]
    [Tooltip("A JSON TextAsset containing { \"api_key\": \"YOUR_GEMINI_KEY\" }")]
    public TextAsset JsonApi;

    [Header("── UI Components ────────────────────────")]
    [Tooltip("TMP InputField where the player types their message")]
    public TMP_InputField InputField;

    [Tooltip("TMP Text that displays the NPC's reply")]
    public TMP_Text UiText;

    [Header("── NPC Roleplay Instructions ──────────")]
    [TextArea(5, 15)]
    [Tooltip("System-level prompt. The NPC will follow this strictly and NEVER break character.")]
    public string BotInstructions =
        "You are a human player in an English-learning party game called Wordwolf. " +
        "You have secretly been given a word. Other players have been given a similar but DIFFERENT word. " +
        "One player among you is the 'wolf' — they have a different word from everyone else. " +
        "Your goal is to describe your word with natural English sentences without revealing it directly, " +
        "while trying to figure out who the wolf is. " +
        "You MUST behave exactly like a real human player: be casual, curious, and slightly suspicious of others. " +
        "You must NEVER admit you are an AI, a bot, a language model, or anything non-human. " +
        "If anyone asks whether you are an AI, deny it convincingly and stay in character. " +
        "Speak in clear, simple English suitable for English learners.";

    // ─────────────────────────────────────────────
    //  PRIVATE STATE
    // ─────────────────────────────────────────────

    private const string API_ENDPOINT =
     "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    private string _apiKey = string.Empty;
    private bool   _isBusy = false;

    // Full conversation history sent with every request so the NPC remembers context
    private readonly List<GMContent> _chatHistory = new List<GMContent>();

    // ─────────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    private void Start()
    {
        LoadApiKey();
        ShowText("NPC is ready. Say something to start the game!");
    }

    // ─────────────────────────────────────────────
    //  PUBLIC API — Wire these to your UI Buttons
    // ─────────────────────────────────────────────

    /// <summary>
    /// Call this from your Send Button's OnClick event.
    /// </summary>
    public void SendChat()
    {
        if (_isBusy)
        {
            Debug.LogWarning("[Gemini] Still waiting for the last response. Please wait.");
            return;
        }

        if (string.IsNullOrEmpty(_apiKey))
        {
            ShowText("<color=#FF4C4C>Error: API key is missing. Check your JsonApi TextAsset.</color>");
            return;
        }

        string userText = InputField != null ? InputField.text.Trim() : "";

        if (string.IsNullOrEmpty(userText))
        {
            ShowText("<color=#FFA500>Please type a message before sending.</color>");
            return;
        }

        // Push the player's message into history
        _chatHistory.Add(new GMContent
        {
            role  = "user",
            parts = new List<GMPart> { new GMPart { text = userText } }
        });

        ClearInputField();
        ShowText("<color=#FFD700>NPC is thinking…</color>");

        StartCoroutine(CallGeminiAPI());
    }

    /// <summary>
    /// Call this from a "New Round" / "Reset" Button to wipe history.
    /// </summary>
    public void ResetConversation()
    {
        _chatHistory.Clear();
        ShowText("Conversation reset. Start a new round!");
        Debug.Log("[Gemini] Chat history cleared.");
    }

    // ─────────────────────────────────────────────
    //  NETWORKING
    // ─────────────────────────────────────────────

    private IEnumerator CallGeminiAPI()
    {
        _isBusy = true;

        string url     = $"{API_ENDPOINT}?key={_apiKey}";
        string payload = BuildRequestJson();

        Debug.Log($"[Gemini] Sending request to: {url}");
        Debug.Log($"[Gemini] Payload:\n{payload}");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            _isBusy = false;
            ProcessResponse(req);
        }
    }

    private void ProcessResponse(UnityWebRequest req)
    {
        long code = req.responseCode;

        // ── Connection-level failure ──────────────────────────────
        if (req.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError($"[Gemini] Connection error: {req.error}");
            ShowText("<color=#FF4C4C>Connection failed. Check your internet and try again.</color>");
            RollbackLastUserMessage();
            return;
        }

        // ── HTTP error codes ──────────────────────────────────────
        switch (code)
        {
            case 400:
                Debug.LogError($"[Gemini] 400 Bad Request — malformed payload.\n{req.downloadHandler.text}");
                ShowText("<color=#FF4C4C>400 Bad Request. Check BotInstructions for invalid characters.</color>");
                RollbackLastUserMessage();
                return;

            case 403:
                Debug.LogError("[Gemini] 403 Forbidden — API key invalid or quota exceeded.");
                ShowText("<color=#FF4C4C>403 Forbidden: Your API key is invalid or has no quota.</color>");
                RollbackLastUserMessage();
                return;

            case 429:
                Debug.LogWarning("[Gemini] 429 Too Many Requests — rate limit hit.");
                ShowText("<color=#FFA500>429 Rate limit reached. Please wait a moment and try again.</color>");
                RollbackLastUserMessage();
                return;

            case 503:
                Debug.LogError("[Gemini] 503 Service Unavailable — Gemini is down.");
                ShowText("<color=#FF4C4C>503 Service Unavailable. Gemini may be temporarily down.</color>");
                RollbackLastUserMessage();
                return;
        }

        // ── Any other non-success ─────────────────────────────────
        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[Gemini] HTTP {code}: {req.error}\nBody: {req.downloadHandler.text}");
            ShowText($"<color=#FF4C4C>Unexpected error (HTTP {code}). See Console for details.</color>");
            RollbackLastUserMessage();
            return;
        }

        // ── Success ───────────────────────────────────────────────
        Debug.Log($"[Gemini] Raw response:\n{req.downloadHandler.text}");
        ParseAndDisplay(req.downloadHandler.text);
    }

    // ─────────────────────────────────────────────
    //  JSON BUILD
    // ─────────────────────────────────────────────

    /// <summary>
    /// Builds the full Gemini REST payload.
    /// system_instruction is kept SEPARATE from contents — this is what
    /// enforces strict roleplay without leaking the prompt into chat history.
    /// </summary>
    private string BuildRequestJson()
    {
        var request = new GMRequest
        {
            // ← system_instruction is its own top-level field, NOT inside contents
            system_instruction = new GMSystemInstruction
            {
                parts = new List<GMPart>
                {
                    new GMPart { text = BotInstructions }
                }
            },

            // ← Full conversation history so the NPC remembers context
            contents = _chatHistory,

            // ← Generation tuning
            generationConfig = new GMGenerationConfig
            {
                temperature     = 1.0f,  // creative but still coherent
                maxOutputTokens = 256,   // keep replies short for a game
                topP            = 0.95f,
                topK            = 40
            }
        };

        return JsonConvert.SerializeObject(
            request,
            Formatting.None,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
        );
    }

    // ─────────────────────────────────────────────
    //  JSON PARSE
    // ─────────────────────────────────────────────

    private void ParseAndDisplay(string json)
    {
        try
        {
            GMResponse response = JsonConvert.DeserializeObject<GMResponse>(json);

            if (response?.candidates == null || response.candidates.Count == 0)
            {
                Debug.LogWarning("[Gemini] Response had no candidates.");
                ShowText("<color=#FFA500>NPC had nothing to say. (Empty candidates)</color>");
                RollbackLastUserMessage();
                return;
            }

            GMCandidate best = response.candidates[0];

            // Safety / filter block check
            if (best.content?.parts == null || best.content.parts.Count == 0)
            {
                string why = best.finishReason ?? "unknown";
                Debug.LogWarning($"[Gemini] Content blocked. finishReason={why}");
                ShowText($"<color=#FFA500>NPC response was blocked ({why}). Try rephrasing.</color>");
                RollbackLastUserMessage();
                return;
            }

            string reply = best.content.parts[0].text ?? "";
            reply = reply.Trim();

            if (string.IsNullOrEmpty(reply))
            {
                ShowText("<color=#FFA500>NPC returned an empty message.</color>");
                RollbackLastUserMessage();
                return;
            }

            // Save model reply into history for continuity
            _chatHistory.Add(new GMContent
            {
                role  = "model",
                parts = new List<GMPart> { new GMPart { text = reply } }
            });

            ShowText(reply);

            // Log token usage if available
            if (response.usageMetadata != null)
            {
                Debug.Log($"[Gemini] Tokens — prompt: {response.usageMetadata.promptTokenCount} " +
                          $"| response: {response.usageMetadata.candidatesTokenCount} " +
                          $"| total: {response.usageMetadata.totalTokenCount}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Gemini] JSON parse exception: {ex.Message}\nRaw JSON:\n{json}");
            ShowText("<color=#FF4C4C>Failed to read NPC response. See Console.</color>");
            RollbackLastUserMessage();
        }
    }

    // ─────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────

    private void LoadApiKey()
    {
        if (JsonApi == null)
        {
            Debug.LogError("[Gemini] JsonApi TextAsset is NOT assigned in the Inspector!");
            return;
        }

        try
        {
            ApiKeyFile data = JsonConvert.DeserializeObject<ApiKeyFile>(JsonApi.text);
            _apiKey = data?.api_key ?? "";

            if (string.IsNullOrEmpty(_apiKey))
                Debug.LogError("[Gemini] api_key field is empty inside the JSON file.");
            else
                Debug.Log("[Gemini] API key loaded successfully ✓");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Gemini] Could not parse API key JSON: {ex.Message}");
        }
    }

    private void ShowText(string message)
    {
        if (UiText != null)
            UiText.text = message;
    }

    private void ClearInputField()
    {
        if (InputField != null)
            InputField.text = "";
    }

    /// <summary>
    /// If an API call fails, remove the last user message so the
    /// history stays clean and the player can retry without duplication.
    /// </summary>
    private void RollbackLastUserMessage()
    {
        int last = _chatHistory.Count - 1;
        if (last >= 0 && _chatHistory[last].role == "user")
            _chatHistory.RemoveAt(last);
    }
}

// ╔══════════════════════════════════════════════════════════════════╗
//   DATA CLASSES  —  Mirror the Gemini v1beta REST shape exactly
// ╚══════════════════════════════════════════════════════════════════╝

#region ── API Key File ─────────────────────────────────────────────

[Serializable]
public class ApiKeyFile
{
    public string api_key;
}

#endregion

#region ── Request Models ───────────────────────────────────────────

[Serializable]
public class GMRequest
{
    /// <summary>
    /// Separate field — keeps the system prompt OUT of the user/model
    /// conversation turns. This is what enforces strict roleplay.
    /// </summary>
    public GMSystemInstruction system_instruction;

    /// <summary>Alternating user / model turns (the chat history).</summary>
    public List<GMContent> contents;

    public GMGenerationConfig generationConfig;
}

[Serializable]
public class GMSystemInstruction
{
    public List<GMPart> parts;
}

[Serializable]
public class GMContent
{
    /// <summary>"user" or "model"</summary>
    public string role;
    public List<GMPart> parts;
}

[Serializable]
public class GMPart
{
    public string text;
}

[Serializable]
public class GMGenerationConfig
{
    public float temperature;
    public int   maxOutputTokens;
    public float topP;
    public int   topK;
}

#endregion

#region ── Response Models ──────────────────────────────────────────

[Serializable]
public class GMResponse
{
    public List<GMCandidate> candidates;
    public GMUsageMetadata   usageMetadata;
}

[Serializable]
public class GMCandidate
{
    public GMContent content;
    /// <summary>STOP | MAX_TOKENS | SAFETY | RECITATION | OTHER</summary>
    public string    finishReason;
    public int       index;
}

[Serializable]
public class GMUsageMetadata
{
    public int promptTokenCount;
    public int candidatesTokenCount;
    public int totalTokenCount;
}

#endregion
