// Assets/kin/OpenAI/Scripts/NPCChatInteractable.cs

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(NPCChatController))]
public class NPCChatInteractable : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Transform player;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Optional UI")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string promptMessage = "Press E to talk";

    private NPCChatController _chatController;
    private NPCChatUI _chatUI;
    private bool _playerInRange;

    private void Awake()
    {
        _chatController = GetComponent<NPCChatController>();
        _chatUI = GetComponent<NPCChatUI>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        HidePrompt();
    }

    private void Update()
    {
        if (player == null)
            return;

        _playerInRange = Vector3.Distance(transform.position, player.position) <= interactDistance;

        if (_playerInRange && !_chatController.isChatActive)
            ShowPrompt();
        else
            HidePrompt();

        if (!_playerInRange)
        {
            if (_chatController.isChatActive)
                _chatController.CloseChat();

            return;
        }

        if (Input.GetKeyDown(interactKey) && !IsTypingInInputField())
            ToggleChatForThisNpc();
    }

    private bool IsTypingInInputField()
    {
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
            return false;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        TMP_InputField inputField = selectedObject.GetComponent<TMP_InputField>();
        return inputField != null && inputField.isFocused;
    }

    private void ToggleChatForThisNpc()
    {
        if (_chatController.isChatActive)
        {
            _chatController.CloseChat();
            HidePrompt();
            return;
        }

        _chatController.OpenChat();
        HidePrompt();

        if (_chatUI != null)
            _chatUI.FocusInputField();
    }

    private void ShowPrompt()
    {
        if (promptText == null)
            return;

        promptText.gameObject.SetActive(true);
        promptText.text = promptMessage;
    }

    private void HidePrompt()
    {
        if (promptText == null)
            return;

        // Do not disable the GameObject here. If the prompt field is accidentally
        // assigned to the NPC reply text, disabling it hides the AI response too.
        if (promptText.text == promptMessage)
            promptText.text = string.Empty;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}
