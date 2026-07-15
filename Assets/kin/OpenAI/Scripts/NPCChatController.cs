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
    [Tooltip("When user/assistant messages reach this count, old messages are summarized into long-term memory.")]
    [SerializeField] private int summarizeAfterMessages = 8;

    [Tooltip("How many recent user/assistant messages stay as exact chat history after summarizing.")]
    [SerializeField] private int recentMessagesToKeep = 4;

    [Tooltip("Used as a safety limit if summarization fails.")]
    [SerializeField] private int maxHistoryMessages = 12;

    [TextArea(3, 8)]
    [SerializeField] private string longTermMemorySummary = "";

    [Header("NPC Identity")]
    [SerializeField] private string npcName = "Eldric";
    [SerializeField] private PlayerRole.Role hiddenRole = PlayerRole.Role.Villager;

    [TextArea(2, 5)]
    [SerializeField] private string personality =
        "Gruff but honest blacksmith who distrusts strangers.";

    [Header("Knowledge Base")]
    [TextArea(3, 10)]
    [Tooltip("What this NPC thinks about other villagers.")]
    [SerializeField] private string relationships = "";

    [TextArea(2, 6)]
    [Tooltip("Secrets, rumors, or clues this NPC knows.")]
    [SerializeField] private string secretsOrRumors = "";

    [Header("Game Context")]
    [SerializeField] private string currentGameState = "It is daytime.";

    [Header("Overhearing")]
    [SerializeField] private bool allowNearbyNPCsToOverhear = true;
    [SerializeField] private float overhearDistance = 7f;
    [SerializeField] private LayerMask overhearingLayers = Physics.DefaultRaycastLayers;

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
    private Player _player;
    private PlayerRole _roleComponent;
    private NPCMemory _npcMemory;
    private bool _chatOpen;
    private string _lastResponse = string.Empty;

    public string NpcName => npcName;
    public bool IsBusy => _isBusy;
    public bool isChatActive => _chatOpen;
    public string LongTermMemorySummary => longTermMemorySummary;
    public string LastResponse => _lastResponse;
    public PlayerRole.Role HiddenRole => hiddenRole;

    private void Awake()
    {
        FindChatCanvasIfNeeded();
        _player = GameObject.FindObjectOfType<Player>();
        _roleComponent = GetComponent<PlayerRole>();
        _npcMemory = GetComponent<NPCMemory>();

        InitializeConversation();
        CloseChat();
    }

    private void Start()
    {
        SyncAssignedRoleFromGameManager();
        DiscussionRoster.Register(this);
    }

    private void OnDisable()
    {
        DiscussionRoster.Register(this);
    }

    private void OnDestroy()
    {
        DiscussionRoster.Register(this);
        CancelActiveRequest();
    }

    public void OpenChat()
    {
        _chatOpen = true;

        if (chatUICanvas != null)
            chatUICanvas.SetActive(true);

        SetPlayerCursorFree(true);
    }

    public void CloseChat()
    {
        _chatOpen = false;

        if (chatUICanvas != null)
            chatUICanvas.SetActive(false);

        SetPlayerCursorFree(false);
    }

    public void ToggleChat()
    {
        if (_chatOpen)
            CloseChat();
        else
            OpenChat();
    }

    public void SetGameState(string gameState)
    {
        currentGameState = gameState ?? string.Empty;
        RefreshDeveloperPrompt();
    }

    public void SetAssignedRole(PlayerRole.Role assignedRole)
    {
        hiddenRole = assignedRole;
        RefreshDeveloperPrompt();
    }

    public void SyncAssignedRoleFromGameManager()
    {
        if (GameManager.Instance == null)
            return;

        if (_roleComponent == null)
            _roleComponent = GetComponent<PlayerRole>();

        if (_roleComponent == null || _roleComponent.isPlayer)
            return;

        int npcIndex = _roleComponent.npcIndex;
        if (npcIndex < 0 || npcIndex >= GameManager.Instance.savedNPCRoles.Count)
            return;

        SetAssignedRole(GameManager.Instance.savedNPCRoles[npcIndex]);
    }

    public void InitializeConversation()
    {
        _conversationHistory.Clear();
        RefreshDeveloperPrompt();
    }

    public void ResetConversation()
    {
        CancelActiveRequest();
        longTermMemorySummary = string.Empty;
        _lastResponse = string.Empty;
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
            RefreshDeveloperPrompt();
            _conversationHistory.Add(new ChatMessage("user", playerMessage.Trim()));

            string npcReply = await OpenAIManager.Instance.SendChatCompletionAsync(
                _conversationHistory, _activeRequestCts.Token);

            _conversationHistory.Add(new ChatMessage("assistant", npcReply));
            _lastResponse = npcReply;
            if (allowNearbyNPCsToOverhear)
                NPCConversationAwareness.ShareConversation(this, npcReply, overhearDistance, overhearingLayers);

            _ = SummarizeOldHistoryIfNeededAsync(_activeRequestCts.Token);
            return npcReply;
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task SummarizeOldHistoryIfNeededAsync(CancellationToken cancellationToken)
    {
        int nonDeveloperCount = _conversationHistory.Count - 1;
        if (nonDeveloperCount < summarizeAfterMessages)
            return;

        int safeRecentCount = Mathf.Clamp(recentMessagesToKeep, 0, nonDeveloperCount);
        int messagesToSummarize = nonDeveloperCount - safeRecentCount;
        if (messagesToSummarize <= 0)
            return;

        List<ChatMessage> oldMessages = _conversationHistory.GetRange(1, messagesToSummarize);
        List<ChatMessage> recentMessages = _conversationHistory.GetRange(1 + messagesToSummarize, safeRecentCount);

        try
        {
            string updatedSummary = await RequestMemorySummaryAsync(oldMessages, cancellationToken);
            longTermMemorySummary = updatedSummary.Trim();

            _conversationHistory.Clear();
            _conversationHistory.Add(new ChatMessage(DeveloperRole, BuildDeveloperPrompt()));
            _conversationHistory.AddRange(recentMessages);

            Debug.Log($"[NPCChat:{npcName}] Summarized {oldMessages.Count} messages into long-term memory.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[NPCChat:{npcName}] Memory summary failed, using sliding window fallback: {ex.Message}");
            TrimHistoryToSafetyLimit();
        }
    }

    private async Task<string> RequestMemorySummaryAsync(List<ChatMessage> oldMessages, CancellationToken cancellationToken)
    {
        var summaryMessages = new List<ChatMessage>
        {
            new ChatMessage("developer",
                "You summarize conversation memory for a medieval werewolf mystery NPC. " +
                "Keep only facts that the NPC should remember later: player claims, accusations, clues, promises, suspicions, and important emotional reactions. " +
                "Do not roleplay. Do not add new facts. Keep it concise, under 120 words."),
            new ChatMessage("user", BuildSummaryPrompt(oldMessages))
        };

        return await OpenAIManager.Instance.SendChatCompletionAsync(summaryMessages, cancellationToken);
    }

    private string BuildSummaryPrompt(List<ChatMessage> oldMessages)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"NPC name: {npcName}");
        sb.AppendLine("Existing long-term memory:");
        sb.AppendLine(string.IsNullOrWhiteSpace(longTermMemorySummary) ? "None yet." : longTermMemorySummary.Trim());
        sb.AppendLine();
        sb.AppendLine("New conversation to merge into memory:");

        foreach (ChatMessage message in oldMessages)
        {
            if (message.Role == DeveloperRole)
                continue;

            string speaker = message.Role == "assistant" ? npcName : "Player";
            sb.AppendLine($"{speaker}: {message.Content}");
        }

        sb.AppendLine();
        sb.AppendLine("Return the updated long-term memory summary only.");
        return sb.ToString();
    }

    private void FindChatCanvasIfNeeded()
    {
        if (chatUICanvas != null)
            return;

        chatUICanvas = GameObject.Find("ChatPanel");
        if (chatUICanvas != null)
        {
            Debug.Log($"[NPCChatController] Automatically assigned chatUICanvas: {chatUICanvas.name}");
            return;
        }

        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas == null)
            return;

        Transform panel = canvas.transform.Find("ChatPanel");
        if (panel != null)
        {
            chatUICanvas = panel.gameObject;
            Debug.Log($"[NPCChatController] Automatically found ChatPanel under Canvas: {chatUICanvas.name}");
        }
    }

    private void SetPlayerCursorFree(bool free)
    {
        if (_player == null)
            _player = GameObject.FindObjectOfType<Player>();

        if (_player != null)
            _player.SetCursorFree(free);
        else
        {
            Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = free;
        }
    }

    private void TrimHistoryToSafetyLimit()
    {
        int nonDeveloperCount = _conversationHistory.Count - 1;
        if (nonDeveloperCount <= maxHistoryMessages)
            return;

        int removeCount = nonDeveloperCount - maxHistoryMessages;
        _conversationHistory.RemoveRange(1, removeCount);
    }

    private void RefreshDeveloperPrompt()
    {
        var prompt = new ChatMessage(DeveloperRole, BuildDeveloperPrompt());

        if (_conversationHistory.Count == 0)
            _conversationHistory.Add(prompt);
        else
            _conversationHistory[0] = prompt;
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

        AppendRoleInstructions(sb);

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

        if (!string.IsNullOrWhiteSpace(longTermMemorySummary))
        {
            sb.AppendLine("LONG-TERM CONVERSATION MEMORY:");
            sb.AppendLine(longTermMemorySummary.Trim());
        }

        if (_npcMemory != null && _npcMemory.HasMemories)
        {
            sb.AppendLine("THINGS YOU PERSONALLY WITNESSED:");
            sb.AppendLine(_npcMemory.BuildPromptMemory());
        }

        sb.AppendLine($"English difficulty: {englishDifficulty}");
        sb.AppendLine($"Grammar tense: {targetGrammarTense}");
        return sb.ToString().Trim();
    }

    private void AppendRoleInstructions(StringBuilder sb)
    {
        switch (hiddenRole)
        {
            case PlayerRole.Role.Werewolf:
                sb.AppendLine("SECRET ROLE: You are a Werewolf. Hide your role, protect fellow Werewolves, and deflect suspicion without inventing witnessed facts.");
                AppendWerewolfTeammates(sb);
                break;
            case PlayerRole.Role.Arsonist:
                sb.AppendLine("SECRET ROLE: You are an Arsonist. Hide your role, mislead the village, and avoid drawing attention to your targets.");
                break;
            case PlayerRole.Role.Seer:
                sb.AppendLine("SECRET ROLE: You are the Seer. You may share only role information you personally discovered, and may keep your role secret for safety.");
                break;
            case PlayerRole.Role.Doctor:
                sb.AppendLine("SECRET ROLE: You are the Doctor. You may protect villagers at night. Do not claim this role unless it helps the village.");
                break;
            case PlayerRole.Role.Jailer:
                sb.AppendLine("SECRET ROLE: You are the Jailer. You may jail a target. Do not claim this role unless it helps the village.");
                break;
            case PlayerRole.Role.Witch:
                sb.AppendLine("SECRET ROLE: You are the Witch. You have limited potions. Do not reveal your role or potion use unless it helps the village.");
                break;
            case PlayerRole.Role.Gunner:
                sb.AppendLine("SECRET ROLE: You are the Gunner. Use public evidence and personal memories before deciding whom to trust.");
                break;
            case PlayerRole.Role.Vigilante:
                sb.AppendLine("SECRET ROLE: You are the Vigilante. Use public evidence and personal memories before deciding whom to trust.");
                break;
            default:
                sb.AppendLine("SECRET ROLE: You are a Villager. Seek the truth using only public discussion and things you personally witnessed.");
                break;
        }
    }

    public string BuildDiscussionDeveloperPrompt()
    {
        return BuildDeveloperPrompt();
    }

    private void AppendWerewolfTeammates(StringBuilder sb)
    {
        if (GameManager.Instance == null || GameManager.Instance.savedNPCRoles.Count == 0)
            return;

        if (_roleComponent == null)
            _roleComponent = GetComponent<PlayerRole>();

        if (_roleComponent == null)
            return;

        var teammateIndices = new List<int>();
        for (int i = 0; i < GameManager.Instance.savedNPCRoles.Count; i++)
        {
            if (i == _roleComponent.npcIndex ||
                !GameManager.Instance.npcAlive[i] ||
                GameManager.Instance.savedNPCRoles[i] != PlayerRole.Role.Werewolf)
                continue;

            teammateIndices.Add(i);
        }

        if (GameManager.Instance.playerRole == PlayerRole.Role.Werewolf)
            sb.AppendLine("YOUR LIVING WEREWOLF TEAMMATE: the player.");

        if (teammateIndices.Count > 0)
            sb.AppendLine("YOUR LIVING WEREWOLF TEAMMATES (NPC indices): " + string.Join(", ", teammateIndices) + ".");
    }

    private void CancelActiveRequest()
    {
        if (_activeRequestCts == null) return;
        _activeRequestCts.Cancel();
        _activeRequestCts.Dispose();
        _activeRequestCts = null;
    }
}
