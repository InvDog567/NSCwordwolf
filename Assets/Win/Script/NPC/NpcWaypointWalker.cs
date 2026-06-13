using System.Collections;        // Needed for IEnumerator (coroutines)
using UnityEngine;                // Core Unity stuff (Vector3, Debug, etc.)
using UnityEngine.AI;             // Needed for NavMeshAgent
 
public class NPCWaypointWalker : MonoBehaviour
{
    // --------------------------------------------------------
    // DATA STRUCTURES
    // A "struct" is like a small container that groups
    // related pieces of information together.
    // --------------------------------------------------------
 
    [System.Serializable]   // Makes this struct visible in the Inspector
    public struct Waypoint
    {
        public Transform location;      // The position to walk TO
        public float stayDuration;      // How many seconds to wait there
        public string waypointName;     // Optional label for debugging
    }
 
    // --------------------------------------------------------
    // INSPECTOR FIELDS
    // Variables marked [Header] and [SerializeField] show up
    // in the Unity Inspector panel so you can edit them
    // without touching any code.
    // --------------------------------------------------------
 
    [Header("Waypoints")]
    [Tooltip("Add waypoint targets and how long the NPC stays at each one.")]
    public Waypoint[] waypoints;        // Array = a list of waypoints
 
    [Header("Movement Settings")]
    [Tooltip("How fast the NPC walks (meters per second).")]
    public float walkSpeed = 2f;
 
    [Tooltip("How close the NPC needs to get before it counts as 'arrived'.")]
    public float arrivalThreshold = 0.5f;
 
    [Header("Countdown Display (optional)")]
    [Tooltip("Optional: assign a UI Text or TextMeshPro component to show countdown on screen.")]
    public UnityEngine.UI.Text countdownText;   // Drag a UI Text here in Inspector (optional)
 
    [Header("Loop Behaviour")]
    [Tooltip("Should the NPC repeat the route forever?")]
    public bool loopRoute = true;
 
    [Tooltip("Reverse the route when it reaches the end instead of jumping back to start.")]
    public bool pingPong = false;
 
    // --------------------------------------------------------
    // PRIVATE VARIABLES
    // These are internal — you won't see them in the Inspector.
    // The underscore prefix is a common naming convention for
    // private fields.
    // --------------------------------------------------------
 
    private NavMeshAgent _agent;        // Reference to the NavMeshAgent on this NPC
    private int _currentWaypointIndex;  // Which waypoint we are heading to right now
    private bool _isWaiting;            // True while the NPC is standing still at a waypoint
    private bool _movingForward = true; // Used for ping-pong direction
    private float _countdownTimer;      // Counts down the stay duration
 
    // --------------------------------------------------------
    // UNITY LIFECYCLE METHODS
    // Unity calls these automatically at specific moments.
    // --------------------------------------------------------
 
    // Awake() runs ONCE when the object first exists (before Start)
    // Good place to grab component references.
    private void Awake()
    {
        // GetComponent<T>() finds a component of type T on THIS GameObject.
        _agent = GetComponent<NavMeshAgent>();
 
        // Safety check — if there's no NavMeshAgent we log an error.
        if (_agent == null)
        {
            Debug.LogError($"[NPCWaypointWalker] '{gameObject.name}' is missing a NavMeshAgent component!");
        }
    }
 
    // Start() runs ONCE just before the first frame.
    // Good place to initialise state and kick off routines.
    private void Start()
    {
        // Apply walk speed to the NavMeshAgent
        _agent.speed = walkSpeed;
 
        // Make sure we actually have waypoints to visit
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"[NPCWaypointWalker] '{gameObject.name}' has no waypoints assigned!");
            return;
        }
 
        // StartCoroutine kicks off a coroutine — a function that can
        // "pause" mid-execution and resume later (great for waiting).
        StartCoroutine(WaypointRoutine());
    }
 
    // Update() runs EVERY FRAME (~60 times per second).
    // We use it only for the live countdown display.
    private void Update()
    {
        // If we're currently waiting at a waypoint, update the on-screen timer
        if (_isWaiting && countdownText != null)
        {
            // Show the remaining seconds, rounded to one decimal place
            countdownText.text = $"Waiting: {_countdownTimer:F1}s";
        }
        else if (countdownText != null)
        {
            // Show that the NPC is walking
            countdownText.text = "Walking...";
        }
    }
 
    // --------------------------------------------------------
    // COROUTINE — THE MAIN NPC BRAIN
    // IEnumerator means this is a coroutine.
    // "yield return" is how we pause execution.
    // --------------------------------------------------------
 
    private IEnumerator WaypointRoutine()
    {
        // We loop forever (or until loopRoute is false and we finish the route)
        while (true)
        {
            // --- Step 1: Get the current waypoint ---
            Waypoint target = waypoints[_currentWaypointIndex];
 
            // Safety: skip this waypoint if no Transform was assigned
            if (target.location == null)
            {
                Debug.LogWarning($"[NPCWaypointWalker] Waypoint {_currentWaypointIndex} has no location assigned. Skipping.");
                AdvanceWaypoint();
                continue;   // Jump back to the top of the while loop
            }
 
            // --- Step 2: Tell the NavMeshAgent where to walk ---
            // SetDestination() does ALL the pathfinding — it figures out how
            // to walk around obstacles and over the NavMesh terrain for us.
            _agent.SetDestination(target.location.position);
 
            string label = string.IsNullOrEmpty(target.waypointName)
                ? $"Waypoint {_currentWaypointIndex}"
                : target.waypointName;
 
            Debug.Log($"[NPC] '{gameObject.name}' heading to '{label}'");
 
            // --- Step 3: Wait until we arrive ---
            // "yield return null" means "wait one frame then continue"
            // We keep yielding frames until the NPC is close enough.
            yield return new WaitUntil(() => HasArrived());
 
            // --- Step 4: Stop moving and wait ---
            _agent.isStopped = true;    // Freeze the agent in place
            _isWaiting = true;
            _countdownTimer = target.stayDuration;
 
            Debug.Log($"[NPC] '{gameObject.name}' arrived at '{label}'. Waiting {target.stayDuration}s.");
 
            // Count down the timer, one frame at a time
            while (_countdownTimer > 0f)
            {
                _countdownTimer -= Time.deltaTime;  // Time.deltaTime = seconds since last frame
                yield return null;                  // Wait one frame
            }
 
            // --- Step 5: Resume movement ---
            _agent.isStopped = false;
            _isWaiting = false;
 
            // --- Step 6: Move to the next waypoint ---
            bool routeEnded = AdvanceWaypoint();
 
            // If we've finished the route and looping is off, we stop here.
            if (routeEnded && !loopRoute)
            {
                Debug.Log($"[NPC] '{gameObject.name}' finished route.");
                yield break;    // Exit the coroutine entirely
            }
        }
    }
 
    // --------------------------------------------------------
    // HELPER METHODS
    // Small focused functions that do one clear job.
    // --------------------------------------------------------
 
    // Returns true when the NPC is close enough to its target.
    private bool HasArrived()
    {
        // pathPending = NavMeshAgent is still calculating the path (not ready yet)
        if (_agent.pathPending) return false;
 
        // remainingDistance = how far (in metres) until we reach the destination
        if (_agent.remainingDistance <= arrivalThreshold) return true;
 
        return false;
    }
 
    // Moves to the next waypoint index.
    // Returns true if we've completed a full loop/pass.
    private bool AdvanceWaypoint()
    {
        bool loopCompleted = false;
 
        if (pingPong)
        {
            // Ping-pong: go forward then backward along the list
            if (_movingForward)
            {
                _currentWaypointIndex++;
                if (_currentWaypointIndex >= waypoints.Length)
                {
                    _currentWaypointIndex = waypoints.Length - 2;   // Step back
                    _movingForward = false;
                    loopCompleted = true;
                }
            }
            else
            {
                _currentWaypointIndex--;
                if (_currentWaypointIndex < 0)
                {
                    _currentWaypointIndex = 1;  // Step forward
                    _movingForward = true;
                    loopCompleted = true;
                }
            }
        }
        else
        {
            // Normal loop: 0 → 1 → 2 → … → 0
            _currentWaypointIndex++;
            if (_currentWaypointIndex >= waypoints.Length)
            {
                _currentWaypointIndex = 0;
                loopCompleted = true;
            }
        }
 
        return loopCompleted;
    }
 
    // --------------------------------------------------------
    // PUBLIC METHODS
    // Call these from other scripts if you want to control
    // this NPC from outside (e.g., from a quest script).
    // --------------------------------------------------------
 
    // Force the NPC to immediately head to a specific waypoint index.
    public void GoToWaypoint(int index)
    {
        if (index < 0 || index >= waypoints.Length)
        {
            Debug.LogWarning($"[NPCWaypointWalker] Waypoint index {index} is out of range.");
            return;
        }
 
        StopAllCoroutines();                // Cancel whatever the NPC was doing
        _agent.isStopped = false;
        _isWaiting = false;
        _currentWaypointIndex = index;
        StartCoroutine(WaypointRoutine()); // Restart from the new waypoint
    }
 
    // Pause the NPC in place.
    public void PauseNPC()
    {
        _agent.isStopped = true;
        StopAllCoroutines();
        _isWaiting = false;
        Debug.Log($"[NPC] '{gameObject.name}' paused.");
    }
 
    // Resume from where the NPC left off.
    public void ResumeNPC()
    {
        _agent.isStopped = false;
        StartCoroutine(WaypointRoutine());
        Debug.Log($"[NPC] '{gameObject.name}' resumed.");
    }
 
    // --------------------------------------------------------
    // GIZMOS — Debug visuals in the Scene View
    // Unity calls OnDrawGizmosSelected when this object
    // is selected in the Editor. Draws lines between waypoints
    // so you can see the route without running the game.
    // --------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2) return;
 
        Gizmos.color = Color.cyan;
 
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i].location == null) continue;
 
            // Draw a small sphere at each waypoint
            Gizmos.DrawSphere(waypoints[i].location.position, 0.25f);
 
            // Draw a line to the next waypoint
            int nextIndex = (i + 1) % waypoints.Length;
            if (waypoints[nextIndex].location != null)
            {
                Gizmos.DrawLine(waypoints[i].location.position,
                                waypoints[nextIndex].location.position);
            }
        }
    }
}