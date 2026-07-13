// Assets/kin/OpenAI/Scripts/NPCChatUI.cs

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NPCChatUI : MonoBehaviour
{
    private class ChatDisplayState
    {
        public string originalReply;
        public string thaiTranslation;
        public bool showingThai;
    }

    [Header("UI References")]
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private TMP_Text npcReplyText;
    [SerializeField] private TMP_Text statusText;

    [Header("Translation")]
    [SerializeField] private TMP_Text translateButtonLabel;
    [SerializeField] private string showThaiLabel = "TH";
    [SerializeField] private string showOriginalLabel = "EN";

    private NPCChatController _chatController;
    private string _lastOriginalReply = string.Empty;
    private string _lastThaiTranslation = string.Empty;
    private bool _showingThai;
    private bool _isTranslating;
    private readonly Dictionary<NPCChatController, ChatDisplayState> _displayStates =
        new Dictionary<NPCChatController, ChatDisplayState>();

    private void Start()
    {
        AutoAssignMissingReferences();
        UpdateTranslateButtonLabel();

        if (playerInputField != null)
            playerInputField.onSubmit.AddListener(HandleInputSubmitted);
    }

    private void Update()
    {
        if (playerInputField == null || _chatController == null)
            return;

        if (Input.GetKeyDown(KeyCode.Escape) && _chatController.isChatActive)
        {
            _chatController.CloseChat();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!_chatController.isChatActive)
                _chatController.OpenChat();

            if (playerInputField.isFocused && !string.IsNullOrWhiteSpace(playerInputField.text))
                OnSendButtonClicked();
            else
                FocusInputField();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromCurrentNpc();

        if (playerInputField != null)
            playerInputField.onSubmit.RemoveListener(HandleInputSubmitted);
    }

    public void SetActiveChatController(NPCChatController chatController)
    {
        if (_chatController == chatController)
            return;

        SaveCurrentDisplayState();
        UnsubscribeFromCurrentNpc();
        _chatController = chatController;
        SubscribeToCurrentNpc();

        RestoreDisplayState();
        UpdateTranslateButtonLabel();
        ClearStatus();

        if (npcReplyText == null)
            return;

        string visibleReply = _showingThai ? _lastThaiTranslation : _lastOriginalReply;
        npcReplyText.text = visibleReply;
    }

    public void FocusInputField()
    {
        if (playerInputField == null)
            return;

        EventSystem.current?.SetSelectedGameObject(playerInputField.gameObject);
        playerInputField.Select();
        playerInputField.ActivateInputField();
    }

    public void OnSendButtonClicked()
    {
        if (_chatController == null)
        {
            Debug.LogError("[NPCChatUI] No active NPC selected. Press E near an NPC first.");
            return;
        }

        if (playerInputField == null)
        {
            Debug.LogError("[NPCChatUI] Player Input Field is not assigned.");
            return;
        }

        if (_chatController.IsBusy)
        {
            Debug.Log("[NPCChatUI] Ignored send because NPC is still responding.");
            return;
        }

        string message = playerInputField.text;
        if (string.IsNullOrWhiteSpace(message))
        {
            FocusInputField();
            return;
        }

        Debug.Log($"[NPCChatUI] Sending player message to {_chatController.NpcName}: {message}");
        _chatController.SendPlayerMessageAsync(message);
        playerInputField.text = string.Empty;
        FocusInputField();
    }

    public async void OnTranslateToThaiButtonClicked()
    {
        Debug.Log("[NPCChatUI] Translate button clicked.");

        if (_isTranslating)
            return;

        if (string.IsNullOrWhiteSpace(_lastOriginalReply))
        {
            SetStatus("No NPC reply to translate yet.");
            return;
        }

        if (_showingThai)
        {
            ShowReply(_lastOriginalReply);
            _showingThai = false;
            UpdateTranslateButtonLabel();
            FocusInputField();
            return;
        }

        if (!string.IsNullOrWhiteSpace(_lastThaiTranslation))
        {
            ShowReply(_lastThaiTranslation);
            _showingThai = true;
            UpdateTranslateButtonLabel();
            FocusInputField();
            return;
        }

        if (OpenAIManager.Instance == null || !OpenAIManager.Instance.HasValidApiKey)
        {
            HandleError("OpenAI API key not configured. Cannot translate.");
            return;
        }

        _isTranslating = true;
        SetStatus("Translating...");

        try
        {
            _lastThaiTranslation = await TranslateReplyToThaiAsync(_lastOriginalReply);
            ShowReply(_lastThaiTranslation);
            _showingThai = true;
        }
        catch (Exception ex)
        {
            HandleError($"Translate failed: {ex.Message}");
        }
        finally
        {
            _isTranslating = false;
            ClearStatus();
            UpdateTranslateButtonLabel();
            FocusInputField();
        }
    }

    public void OnResetButtonClicked()
    {
        if (_chatController == null)
            return;

        _chatController.ResetConversation();
        _displayStates.Remove(_chatController);
        _lastOriginalReply = string.Empty;
        _lastThaiTranslation = string.Empty;
        _showingThai = false;
        UpdateTranslateButtonLabel();

        if (statusText != null) statusText.text = "Conversation reset.";
        if (npcReplyText != null) npcReplyText.text = string.Empty;
        FocusInputField();
    }

    private void SubscribeToCurrentNpc()
    {
        if (_chatController == null)
            return;

        _chatController.OnRequestStarted.AddListener(HandleRequestStarted);
        _chatController.OnResponseReceived.AddListener(HandleResponseReceived);
        _chatController.OnError.AddListener(HandleError);
    }

    private void UnsubscribeFromCurrentNpc()
    {
        if (_chatController == null)
            return;

        _chatController.OnRequestStarted.RemoveListener(HandleRequestStarted);
        _chatController.OnResponseReceived.RemoveListener(HandleResponseReceived);
        _chatController.OnError.RemoveListener(HandleError);
    }

    private async Task<string> TranslateReplyToThaiAsync(string englishReply)
    {
        var messages = new List<ChatMessage>
        {
            new ChatMessage("developer",
                "Translate the NPC dialogue into natural Thai. Keep the meaning and tone. " +
                "Do not add explanations. Return Thai translation only."),
            new ChatMessage("user", englishReply)
        };

        return await OpenAIManager.Instance.SendChatCompletionAsync(messages);
    }

    private void AutoAssignMissingReferences()
    {
        if (playerInputField == null)
        {
            playerInputField = GameObject.FindObjectOfType<TMP_InputField>(true);
            if (playerInputField != null)
                Debug.Log($"[NPCChatUI] Automatically found playerInputField: {playerInputField.name}");
            else
                Debug.LogError("[NPCChatUI] No TMP_InputField found. Drag InputField (TMP) into Player Input Field.");
        }

        if (npcReplyText == null)
        {
            TMP_Text[] texts = GameObject.FindObjectsOfType<TMP_Text>(true);
            foreach (TMP_Text text in texts)
            {
                if (playerInputField != null && text.transform.IsChildOf(playerInputField.transform))
                    continue;

                if (text.gameObject.name.ToLower().Contains("placeholder"))
                    continue;

                if (text.gameObject.name.ToLower().Contains("button"))
                    continue;

                npcReplyText = text;
                Debug.Log($"[NPCChatUI] Automatically found npcReplyText: {npcReplyText.gameObject.name}");
                break;
            }

            if (npcReplyText == null)
                Debug.LogError("[NPCChatUI] Npc Reply Text is not assigned. Drag the reply Text (TMP) into Npc Reply Text.");
        }
    }

    private void HandleInputSubmitted(string message)
    {
        if (_chatController == null || !_chatController.isChatActive || string.IsNullOrWhiteSpace(message))
            return;

        OnSendButtonClicked();
    }

    private void HandleRequestStarted()
    {
        Debug.Log($"[NPCChatUI] {_chatController.NpcName} request started.");
        SetStatus($"{_chatController.NpcName} is thinking...");
    }

    private void HandleResponseReceived(string reply)
    {
        Debug.Log($"[NPCChatUI] Response received: {reply}");

        _lastOriginalReply = reply;
        _lastThaiTranslation = string.Empty;
        _showingThai = false;
        UpdateTranslateButtonLabel();
        ClearStatus();
        ShowReply(reply);
        FocusInputField();
    }

    private void SaveCurrentDisplayState()
    {
        if (_chatController == null)
            return;

        _displayStates[_chatController] = new ChatDisplayState
        {
            originalReply = _lastOriginalReply,
            thaiTranslation = _lastThaiTranslation,
            showingThai = _showingThai
        };
    }

    private void RestoreDisplayState()
    {
        _lastOriginalReply = string.Empty;
        _lastThaiTranslation = string.Empty;
        _showingThai = false;

        if (_chatController == null)
            return;

        if (_displayStates.TryGetValue(_chatController, out ChatDisplayState state))
        {
            _lastOriginalReply = state.originalReply ?? string.Empty;
            _lastThaiTranslation = state.thaiTranslation ?? string.Empty;
            _showingThai = state.showingThai && !string.IsNullOrWhiteSpace(_lastThaiTranslation);
            return;
        }

        _lastOriginalReply = _chatController.LastResponse;
    }

    private void HandleError(string errorMessage)
    {
        Debug.LogError($"[NPCChatUI] Chat error: {errorMessage}");

        if (statusText != null)
            statusText.text = $"<color=#FF5555>{errorMessage}</color>";
        else if (npcReplyText != null)
            npcReplyText.text = $"<color=#FF5555>{errorMessage}</color>";

        FocusInputField();
    }

    private void ShowReply(string text)
    {
        if (npcReplyText == null)
        {
            Debug.LogError("[NPCChatUI] Got a response, but Npc Reply Text is not assigned, so it cannot be shown.");
            return;
        }

        npcReplyText.gameObject.SetActive(true);
        npcReplyText.text = text;
        npcReplyText.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();
    }

    private void SetStatus(string text)
    {
        if (statusText != null)
            statusText.text = text;
    }

    private void ClearStatus()
    {
        if (statusText != null && statusText != npcReplyText)
            statusText.text = string.Empty;
    }

    private void UpdateTranslateButtonLabel()
    {
        if (translateButtonLabel == null)
            return;

        translateButtonLabel.text = _showingThai ? showOriginalLabel : showThaiLabel;
    }
}
