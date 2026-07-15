// Assets/kin/OpenAI/Scripts/ChatModels.cs

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public enum EnglishDifficulty
{
    Beginner,
    Intermediate,
    Advanced
}

public enum GrammarTense
{
    PresentSimple,
    PresentContinuous,
    PastSimple,
    PastContinuous,
    FutureSimple,
    Mixed
}

[Serializable]
public class ChatMessage
{
    [JsonProperty("role")]
    public string Role;

    [JsonProperty("content")]
    public string Content;

    public ChatMessage() { }

    public ChatMessage(string role, string content)
    {
        Role = role;
        Content = content;
    }
}

[Serializable]
public class OpenAIChatRequest
{
    [JsonProperty("model")]
    public string Model;

    [JsonProperty("messages")]
    public List<ChatMessage> Messages;

    [JsonProperty("temperature")]
    public float Temperature = 0.8f;

    [JsonProperty("max_tokens")]
    public int MaxTokens = 150;
}

[Serializable]
public class OpenAIChatResponse
{
    [JsonProperty("choices")]
    public List<OpenAIChoice> Choices;

    [JsonProperty("error")]
    public OpenAIError Error;
}

[Serializable]
public class OpenAIChoice
{
    [JsonProperty("message")]
    public ChatMessage Message;
}

[Serializable]
public class OpenAIError
{
    [JsonProperty("message")]
    public string Message;

    [JsonProperty("type")]
    public string Type;
}

[Serializable]
public class OpenAIKeyFile
{
    [JsonProperty("api_key")]
    public string ApiKey;
}
