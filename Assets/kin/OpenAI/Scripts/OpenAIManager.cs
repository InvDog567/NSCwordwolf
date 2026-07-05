// Assets/kin/OpenAI/Scripts/OpenAIManager.cs

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class OpenAIManager : MonoBehaviour
{
    public static OpenAIManager Instance { get; private set; }

    private const string ChatCompletionsUrl = "https://api.openai.com/v1/chat/completions";
    private const string DefaultModel = "gpt-4o-mini";

    [Header("API Configuration")]
    [SerializeField] private TextAsset apiKeyJson;
    [SerializeField] private string apiKeyOverride = "";

    [Header("Request Defaults")]
    [SerializeField] private float temperature = 0.8f;
    [SerializeField] private int maxTokens = 150;

    private string _apiKey = string.Empty;

    private void Awake()
    {

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadApiKey();
    }

    private void LoadApiKey()
    {
        if (apiKeyJson != null)
        {
            try
            {
                var keyFile = JsonConvert.DeserializeObject<OpenAIKeyFile>(apiKeyJson.text);
                if (!string.IsNullOrWhiteSpace(keyFile?.ApiKey))
                {
                    _apiKey = keyFile.ApiKey.Trim();
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OpenAIManager] Failed to parse apiKeyJson: {ex.Message}");
            }
        }

        _apiKey = apiKeyOverride?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(_apiKey))
            Debug.LogWarning("[OpenAIManager] No API key. Assign openai_key.json in Inspector.");
    }

    public bool HasValidApiKey => !string.IsNullOrEmpty(_apiKey);

    public async Task<string> SendChatCompletionAsync(
        IReadOnlyList<ChatMessage> messages,
        CancellationToken cancellationToken = default)
    {
        if (!HasValidApiKey)
            throw new InvalidOperationException("OpenAI API key is missing.");

        if (messages == null || messages.Count == 0)
            throw new ArgumentException("Message list cannot be empty.", nameof(messages));

        var requestBody = new OpenAIChatRequest
        {
            Model = DefaultModel,
            Messages = new List<ChatMessage>(messages),
            Temperature = temperature,
            MaxTokens = maxTokens
        };

        string jsonPayload = JsonConvert.SerializeObject(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using var request = new UnityWebRequest(ChatCompletionsUrl, UnityWebRequest.kHttpVerbPOST);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

        UnityWebRequestAsyncOperation operation = request.SendWebRequest();

        while (!operation.isDone)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
        }

#if UNITY_2020_1_OR_NEWER
        if (request.result != UnityWebRequest.Result.Success)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
        {
            string errorDetail = request.downloadHandler?.text ?? request.error;
            throw new Exception($"OpenAI request failed ({request.responseCode}): {errorDetail}");
        }

        var response = JsonConvert.DeserializeObject<OpenAIChatResponse>(request.downloadHandler.text);

        if (response?.Error != null)
            throw new Exception($"OpenAI API error: {response.Error.Message}");

        if (response?.Choices == null || response.Choices.Count == 0)
            throw new Exception("OpenAI returned an empty choices array.");

        string reply = response.Choices[0].Message?.Content?.Trim();
        if (string.IsNullOrEmpty(reply))
            throw new Exception("OpenAI returned an empty assistant message.");

        return reply;
    }
}
