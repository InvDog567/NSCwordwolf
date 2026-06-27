// Assets/kin/OpenAI/Scripts/NPCChatController.cs

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class NPCChatController : MonoBehaviour
{
    private const string DeveloperRole = "developer";

    [Header("NPC Memory")]
    [Tooltip("จำนวนข้อความที่ NPC จะจำย้อนหลังได้สูงสุด")]
    [SerializeField] private int maxHistoryMessages = 8;

    [Header("NPC Identity")]
    [SerializeField] private string npcName = "Eldric";
    [SerializeField] private NPCRole hiddenRole = NPCRole.Villager;

    [TextArea(2, 5)]
    [SerializeField] private string personality =
        "Gruff but honest blacksmith who distrusts strangers.";

    [Header("Knowledge Base")]
    [TextArea(3, 10)]
    [Tooltip("ความเห็นหรือความสัมพันธ์ต่อคนอื่นๆ ในหมู่บ้าน")]
    [SerializeField] private string relationships = "";

    [TextArea(2, 6)]
    [Tooltip("ความลับ ข้อมูลข่าวลือ หรือคำใบ้สำคัญที่ NPC ตัวนี้ล่วงรู้")]
    [SerializeField] private string secretsOrRumors = "";

    [Header("Game Context")]
    [SerializeField] private string currentGameState = "It is daytime.";

    [Header("English Settings")]
    [SerializeField] private EnglishDifficulty englishDifficulty = EnglishDifficulty.Intermediate;
    [SerializeField] private GrammarTense targetGrammarTense = GrammarTense.PresentSimple;

    [Header("UI")]
    [SerializeField] private GameObject chatUICanvas;

    [Header("Events")]
    public UnityEvent<string> OnResponseReceived;
    public UnityEvent<string> OnError;
    public UnityEvent OnRequestStarted;

    private readonly List<ChatMessage> _conversationHistory = new List<ChatMessage>();
    private bool _isBusy;
    private CancellationTokenSource _activeRequestCts;

    public string NpcName => npcName;
    public bool IsBusy => _isBusy;
    public bool isChatActive => chatUICanvas != null && chatUICanvas.activeInHierarchy;

    public void OpenChat()
    {
        if (chatUICanvas != null)
            chatUICanvas.SetActive(true);
    }

    public void CloseChat()
    {
        if (chatUICanvas != null)
            chatUICanvas.SetActive(false);
    }

    public void ToggleChat()
    {
        if (chatUICanvas != null)
            chatUICanvas.SetActive(!chatUICanvas.activeInHierarchy);
    }

    public void SetGameState(string gameState)
    {
        currentGameState = gameState ?? string.Empty;
        RefreshDeveloperPrompt();
    }

    private void Awake()
    {
        // ค้นหา ChatPanel อัตโนมัติใน Scene หากไม่ได้ลากใส่ใน Inspector
        if (chatUICanvas == null)
        {
            chatUICanvas = GameObject.Find("ChatPanel");
            if (chatUICanvas != null)
            {
                Debug.Log($"[NPCChatController] Automatically assigned chatUICanvas: {chatUICanvas.name}");
            }
            else
            {
                var canvas = GameObject.FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    Transform panel = canvas.transform.Find("ChatPanel");
                    if (panel != null)
                    {
                        chatUICanvas = panel.gameObject;
                        Debug.Log($"[NPCChatController] Automatically found ChatPanel under Canvas: {chatUICanvas.name}");
                    }
                }
            }
        }

        InitializeConversation();
        CloseChat();
    }

    private void OnDestroy() => CancelActiveRequest();

    public void InitializeConversation()
    {
        _conversationHistory.Clear();
        _conversationHistory.Add(new ChatMessage(DeveloperRole, BuildDeveloperPrompt()));
    }

    public void ResetConversation()
    {
        CancelActiveRequest();
        InitializeConversation();
    }

    public async void SendPlayerMessageAsync(string playerMessage)
    {
        try
        {
            string reply = await SendPlayerMessageInternalAsync(playerMessage);
            OnResponseReceived?.Invoke(reply);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Debug.LogError($"[NPCChat:{npcName}] {ex.Message}");
            OnError?.Invoke(ex.Message);
        }
    }

    public async Task<string> SendPlayerMessageInternalAsync(string playerMessage)
    {
        if (_isBusy)
            throw new InvalidOperationException($"{npcName} is still responding.");

        if (string.IsNullOrWhiteSpace(playerMessage))
            throw new ArgumentException("Player message cannot be empty.", nameof(playerMessage));

        if (OpenAIManager.Instance == null)
            throw new InvalidOperationException("OpenAIManager missing in scene.");

        if (!OpenAIManager.Instance.HasValidApiKey)
            throw new InvalidOperationException("OpenAI API key not configured.");

        _isBusy = true;
        CancelActiveRequest();
        _activeRequestCts = new CancellationTokenSource();
        OnRequestStarted?.Invoke();

        try
        {
            _conversationHistory.Add(new ChatMessage("user", playerMessage.Trim()));
            TrimHistoryToSlidingWindow();

            string npcReply = await OpenAIManager.Instance.SendChatCompletionAsync(
                _conversationHistory, _activeRequestCts.Token);

            _conversationHistory.Add(new ChatMessage("assistant", npcReply));
            TrimHistoryToSlidingWindow();
            return npcReply;
        }
        finally
        {
            _isBusy = false;
        }
    }

    private void TrimHistoryToSlidingWindow()
    {
        int nonDeveloperCount = _conversationHistory.Count - 1;
        if (nonDeveloperCount <= maxHistoryMessages) return;
        _conversationHistory.RemoveRange(1, nonDeveloperCount - maxHistoryMessages);
    }

    private void RefreshDeveloperPrompt()
    {
        if (_conversationHistory.Count == 0)
            _conversationHistory.Add(new ChatMessage(DeveloperRole, BuildDeveloperPrompt()));
        else
            _conversationHistory[0] = new ChatMessage(DeveloperRole, BuildDeveloperPrompt());
    }

    private string BuildDeveloperPrompt()
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are NOT an AI. You are an NPC in a medieval werewolf village.");
        sb.AppendLine("Never break character. Never mention AI or the real world.");
        sb.AppendLine("Keep replies to 1-3 sentences.");
        sb.AppendLine($"Name: {npcName}");
        sb.AppendLine($"Personality: {personality}");
        sb.AppendLine($"Situation: {currentGameState}");

        if (hiddenRole == NPCRole.Werewolf)
            sb.AppendLine("SECRET: You are the werewolf. Lie and deflect suspicion.");
        else
            sb.AppendLine("SECRET: You are an innocent villager. Tell the truth.");

        if (!string.IsNullOrWhiteSpace(relationships))
        {
            sb.AppendLine("RELATIONSHIPS AND OPINIONS:");
            sb.AppendLine(relationships.Trim());
        }

        if (!string.IsNullOrWhiteSpace(secretsOrRumors))
        {
            sb.AppendLine("YOUR SECRETS AND RUMORS YOU KNOW:");
            sb.AppendLine(secretsOrRumors.Trim());
        }

        sb.AppendLine($"English difficulty: {englishDifficulty}");
        sb.AppendLine($"Grammar tense: {targetGrammarTense}");
        return sb.ToString().Trim();
    }

    private void CancelActiveRequest()
    {
        if (_activeRequestCts == null) return;
        _activeRequestCts.Cancel();
        _activeRequestCts.Dispose();
        _activeRequestCts = null;
    }
}
