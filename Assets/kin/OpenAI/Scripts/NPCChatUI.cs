// Assets/kin/OpenAI/Scripts/NPCChatUI.cs

using TMPro;
using UnityEngine;

[RequireComponent(typeof(NPCChatController))]
public class NPCChatUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private TMP_Text npcReplyText;
    [SerializeField] private TMP_Text statusText;

    private NPCChatController _chatController;

    private void Awake() => _chatController = GetComponent<NPCChatController>();

    private void OnEnable()
    {
        _chatController.OnRequestStarted.AddListener(HandleRequestStarted);
        _chatController.OnResponseReceived.AddListener(HandleResponseReceived);
        _chatController.OnError.AddListener(HandleError);
    }

    private void OnDisable()
    {
        _chatController.OnRequestStarted.RemoveListener(HandleRequestStarted);
        _chatController.OnResponseReceived.RemoveListener(HandleResponseReceived);
        _chatController.OnError.RemoveListener(HandleError);
    }

    public void OnSendButtonClicked()
    {
        if (playerInputField == null) return;
        _chatController.SendPlayerMessageAsync(playerInputField.text);
        playerInputField.text = string.Empty;
        playerInputField.ActivateInputField();
    }

    public void OnResetButtonClicked()
    {
        _chatController.ResetConversation();
        if (statusText != null) statusText.text = "Conversation reset.";
        if (npcReplyText != null) npcReplyText.text = string.Empty;
    }

    private void HandleRequestStarted()
    {
        if (statusText != null)
            statusText.text = $"{_chatController.NpcName} is thinking…";
    }

    private void HandleResponseReceived(string reply)
    {
        if (statusText != null) statusText.text = string.Empty;
        if (npcReplyText != null) npcReplyText.text = reply;
    }

    private void HandleError(string errorMessage)
    {
        if (statusText != null)
            statusText.text = $"<color=#FF5555>{errorMessage}</color>";
    }
}
