using System.Collections;
using UnityEngine;
using UnityEngine.AI;
 
// ============================================================
//  NPCInteraction.cs
//  Attach this to the SAME NPC GameObject as NPCWaypointWalker.
//
//  What it does:
//  - Detects when player is close enough to interact
//  - Press F (or whatever key you set) to open dialogue
//  - NPC stops walking and smoothly turns to face the player
//  - Press F again (or close button) to end dialogue and resume walking
//
//  To change the key later: just change the "interactKey" field
//  in the Inspector — no code changes needed.
// ============================================================
 
public class NpcTalkTo : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("How close the player must be to interact.")]
    public float interactRange = 3f;
 
    [Tooltip("The key to press to open/close dialogue. Change freely in Inspector.")]
    public KeyCode interactKey = KeyCode.F;
 
    [Tooltip("How fast the NPC rotates to face the player (degrees per second).")]
    public float turnSpeed = 5f;
 
    [Header("References")]
    [Tooltip("Drag your dialogue UI panel (Canvas/Panel GameObject) here.")]
    public GameObject dialoguePanel;
 
    [Tooltip("Drag the Player GameObject here.")]
    public Transform player;
 
    // --------------------------------------------------------
    // Private state
    // --------------------------------------------------------
    private NPCWaypointWalker _walker;      // Controls NPC walking
    private NavMeshAgent _agent;            // Controls NPC movement
    private bool _isDialogueOpen = false;   // Are we currently talking?
    private bool _playerInRange = false;    // Is player close enough?
    private Coroutine _turnCoroutine;       // Reference to the turn routine
 
    private void Awake()
    {
        _walker = GetComponent<NPCWaypointWalker>();
        _agent = GetComponent<NavMeshAgent>();
 
        // Auto-find player if not assigned in Inspector
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning($"[NPCInteraction] '{gameObject.name}' could not find Player. Tag your player GameObject as 'Player' or assign it manually.");
        }
 
        // Make sure dialogue panel starts hidden
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
 
    private void Update()
    {
        if (player == null) return;
 
        // Check if player is within interact range
        float distance = Vector3.Distance(transform.position, player.position);
        _playerInRange = distance <= interactRange;
 
        // Listen for the interact key
        if (_playerInRange && Input.GetKeyDown(interactKey))
        {
            if (_isDialogueOpen)
                CloseDialogue();
            else
                OpenDialogue();
        }
 
        // If dialogue is open, keep smoothly turning to face player
        // (player might move slightly during conversation)
        if (_isDialogueOpen)
        {
            FacePlayer();
        }
 
        // Auto-close if player walks away during dialogue
        if (_isDialogueOpen && !_playerInRange)
        {
            CloseDialogue();
        }
    }
 
    // --------------------------------------------------------
    // OPEN DIALOGUE
    // Call this from anywhere — a button, a trigger, another script
    // --------------------------------------------------------
    public virtual void OpenDialogue()
    {
        if (_isDialogueOpen) return;
 
        _isDialogueOpen = true;
 
        // Stop the NPC from walking
        if (_walker != null) _walker.PauseNPC();
 
        // Show the dialogue UI
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
 
        // Start turning to face the player
        if (_turnCoroutine != null) StopCoroutine(_turnCoroutine);
        _turnCoroutine = StartCoroutine(TurnToFacePlayer());
 
        // Lock the cursor so player can click UI buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
 
        Debug.Log($"[NPCInteraction] '{gameObject.name}' dialogue opened.");
    }
 
    // --------------------------------------------------------
    // CLOSE DIALOGUE
    // Call this from a UI close button too:
    // drag the NPC into the button's OnClick and call NPCInteraction.CloseDialogue()
    // --------------------------------------------------------
    public virtual void CloseDialogue()
    {
        if (!_isDialogueOpen) return;
 
        _isDialogueOpen = false;
 
        // Resume walking
        if (_walker != null) _walker.ResumeNPC();
 
        // Hide the dialogue UI
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
 
        // Stop the turn coroutine
        if (_turnCoroutine != null)
        {
            StopCoroutine(_turnCoroutine);
            _turnCoroutine = null;
        }
 
        // Re-lock cursor if your game uses cursor locking
        // Comment this out if your game doesn't lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
 
        Debug.Log($"[NPCInteraction] '{gameObject.name}' dialogue closed.");
    }
 
    // --------------------------------------------------------
    // TURN TO FACE PLAYER — smooth rotation coroutine
    // --------------------------------------------------------
    private IEnumerator TurnToFacePlayer()
    {
        // Keep turning until we're roughly facing the player
        while (_isDialogueOpen)
        {
            FacePlayer();
 
            // Check if we're close enough to facing — if so stop the coroutine
            // but FacePlayer() in Update() will keep micro-adjusting
            yield return null;
        }
    }
 
    private void FacePlayer()
    {
        if (player == null) return;
 
        // Get direction to player on the horizontal plane only
        // (ignore Y so NPC doesn't tilt up/down toward player)
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
 
        if (direction == Vector3.zero) return;
 
        // Smoothly rotate toward player
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * turnSpeed
        );
    }
 
    // --------------------------------------------------------
    // GIZMO — shows interact range as a yellow sphere in Scene View
    // --------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
