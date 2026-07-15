using System.Collections;
using UnityEngine;
using UnityEngine.AI;
 
public class NPCWaypointWalker : MonoBehaviour
{
    [System.Serializable]
    public struct Waypoint
    {
        public Transform location;
        public float stayDuration;
        public string waypointName;
    }
 
    [Header("Waypoints")]
    public Waypoint[] waypoints;
 
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float arrivalThreshold = 0.5f;
 
    [Header("Countdown Display (optional)")]
    public UnityEngine.UI.Text countdownText;
 
    [Header("Loop Behaviour")]
    public bool loopRoute = true;
    public bool pingPong = false;
 
    private NavMeshAgent _agent;
    private int _currentWaypointIndex;
    private bool _isWaiting;
    private bool _movingForward = true;
    private float _countdownTimer;
 
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
 
        if (_agent == null)
            Debug.LogError($"[NPCWaypointWalker] '{gameObject.name}' is missing a NavMeshAgent component!");
    }
 
    private void Start()
    {
        _agent.speed = walkSpeed;
 
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"[NPCWaypointWalker] '{gameObject.name}' has no waypoints assigned!");
            return;
        }
 
        StartCoroutine(DelayedStart());
    }
 
    private IEnumerator DelayedStart()
    {
        // Wait a few frames for all NavMeshAgents to fully initialize
        yield return null;
        yield return null;
        yield return null;
 
        if (!_agent.isOnNavMesh)
        {
            Debug.LogError($"[NPCWaypointWalker] '{gameObject.name}' is NOT on the NavMesh! Make sure it's placed on baked ground.");
            yield break;
        }
 
        StartCoroutine(WaypointRoutine());
    }
 
    private void Update()
    {
        if (_isWaiting && countdownText != null)
            countdownText.text = $"Waiting: {_countdownTimer:F1}s";
        else if (countdownText != null)
            countdownText.text = "Walking...";
    }
 
    private IEnumerator WaypointRoutine()
    {
        while (true)
        {
            Waypoint target = waypoints[_currentWaypointIndex];
 
            if (target.location == null)
            {
                Debug.LogWarning($"[NPCWaypointWalker] '{gameObject.name}' waypoint {_currentWaypointIndex} has no location. Skipping.");
                AdvanceWaypoint();
                continue;
            }
 
            _agent.SetDestination(target.location.position);
 
            string label = string.IsNullOrEmpty(target.waypointName)
                ? $"Waypoint {_currentWaypointIndex}"
                : target.waypointName;
 
            Debug.Log($"[NPC] '{gameObject.name}' heading to '{label}'");
 
            yield return new WaitUntil(() => HasArrived());
 
            _agent.isStopped = true;
            _isWaiting = true;
            _countdownTimer = target.stayDuration;
 
            Debug.Log($"[NPC] '{gameObject.name}' arrived at '{label}'. Waiting {target.stayDuration}s.");
 
            while (_countdownTimer > 0f)
            {
                _countdownTimer -= Time.deltaTime;
                yield return null;
            }
 
            _agent.isStopped = false;
            _isWaiting = false;
 
            bool routeEnded = AdvanceWaypoint();
 
            if (routeEnded && !loopRoute)
            {
                Debug.Log($"[NPC] '{gameObject.name}' finished route.");
                yield break;
            }
        }
    }
 
    private bool HasArrived()
    {
        if (_agent.pathPending) return false;
        if (_agent.remainingDistance <= arrivalThreshold) return true;
        return false;
    }
 
    private bool AdvanceWaypoint()
    {
        bool loopCompleted = false;
 
        if (pingPong)
        {
            if (_movingForward)
            {
                _currentWaypointIndex++;
                if (_currentWaypointIndex >= waypoints.Length)
                {
                    _currentWaypointIndex = waypoints.Length - 2;
                    _movingForward = false;
                    loopCompleted = true;
                }
            }
            else
            {
                _currentWaypointIndex--;
                if (_currentWaypointIndex < 0)
                {
                    _currentWaypointIndex = 1;
                    _movingForward = true;
                    loopCompleted = true;
                }
            }
        }
        else
        {
            _currentWaypointIndex++;
            if (_currentWaypointIndex >= waypoints.Length)
            {
                _currentWaypointIndex = 0;
                loopCompleted = true;
            }
        }
 
        return loopCompleted;
    }
 
    public void GoToWaypoint(int index)
    {
        if (index < 0 || index >= waypoints.Length)
        {
            Debug.LogWarning($"[NPCWaypointWalker] Waypoint index {index} is out of range.");
            return;
        }
 
        StopAllCoroutines();
        _agent.isStopped = false;
        _isWaiting = false;
        _currentWaypointIndex = index;
        StartCoroutine(WaypointRoutine());
    }
 
    public void PauseNPC()
    {
        _agent.isStopped = true;
        StopAllCoroutines();
        _isWaiting = false;
    }
 
    public void ResumeNPC()
    {
        _agent.isStopped = false;
        StartCoroutine(WaypointRoutine());
    }
 
    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2) return;
 
        Gizmos.color = Color.cyan;
 
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i].location == null) continue;
 
            Gizmos.DrawSphere(waypoints[i].location.position, 0.25f);
 
            int nextIndex = (i + 1) % waypoints.Length;
            if (waypoints[nextIndex].location != null)
                Gizmos.DrawLine(waypoints[i].location.position, waypoints[nextIndex].location.position);
        }
    }
}