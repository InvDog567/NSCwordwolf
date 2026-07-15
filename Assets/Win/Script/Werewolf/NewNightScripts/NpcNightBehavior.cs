using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Controls an NPC's movement and detection logic during the Night Phase.
/// Requires NavMeshAgent and PlayerRole components.
/// </summary>
public class NpcNightBehavior : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRadius = 15f;
    public float detectionInterval = 2f;
    [Tooltip("If true, requires line of sight via raycast to detect others")]
    public bool requireLineOfSight = true;
    public LayerMask sightObstacles; // Configure to include walls/environment

    [Header("Movement Settings")]
    public float nightWalkSpeed = 2.5f;

    [HideInInspector]
    public bool forceSafeDestinationsOnly = false;

    private NavMeshAgent agent;
    private PlayerRole playerRole;
    private NPCWaypointWalker dayWalker;

    private NightDestination currentDestination;
    private float originalSpeed;

    private Coroutine roamingRoutine;
    private Coroutine detectionRoutine;

    // Used to calculate normalized time of night (for memories)
    private float nightStartTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        playerRole = GetComponent<PlayerRole>();
        dayWalker = GetComponent<NPCWaypointWalker>();

        if (agent != null)
            originalSpeed = agent.speed;
    }

    private void OnEnable()
    {
        nightStartTime = Time.time;

        // Pause daytime walker
        if (dayWalker != null)
        {
            dayWalker.enabled = false;
        }

        if (agent != null)
        {
            agent.speed = nightWalkSpeed;
        }

        roamingRoutine = StartCoroutine(RoamRoutine());
        detectionRoutine = StartCoroutine(DetectionRoutine());
    }

    private void OnDisable()
    {
        if (roamingRoutine != null) StopCoroutine(roamingRoutine);
        if (detectionRoutine != null) StopCoroutine(detectionRoutine);

        if (agent != null)
        {
            agent.speed = originalSpeed;
            agent.ResetPath();
        }

        // Resume daytime walker
        if (dayWalker != null)
        {
            dayWalker.enabled = true;
        }
    }

    private IEnumerator RoamRoutine()
    {
        // Give registry time to init if we enable very early
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            if (NightDestinationRegistry.Instance == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            // Pick a destination
            NightDestination chosenDest = null;
            if (forceSafeDestinationsOnly)
            {
                var allDests = NightDestinationRegistry.Instance.GetAllDestinations();
                var safeDests = allDests.FindAll(d => d.suspicionValue == 0f);
                if (safeDests.Count > 0)
                {
                    var filtered = safeDests.FindAll(d => d != currentDestination);
                    if (filtered.Count > 0)
                        chosenDest = filtered[Random.Range(0, filtered.Count)];
                    else
                        chosenDest = safeDests[Random.Range(0, safeDests.Count)];
                }
            }

            if (chosenDest == null)
            {
                var allDests = NightDestinationRegistry.Instance.GetAllDestinations();
                var redDests = allDests.FindAll(d => d.suspicionValue >= 1f && d != currentDestination);
                var otherDests = allDests.FindAll(d => d.suspicionValue < 1f && d != currentDestination);

                // 75% chance to pick a Red Zone if available
                if (redDests.Count > 0 && Random.Range(0f, 1f) < 0.75f)
                {
                    chosenDest = redDests[Random.Range(0, redDests.Count)];
                }
                else if (otherDests.Count > 0)
                {
                    chosenDest = otherDests[Random.Range(0, otherDests.Count)];
                }
                else
                {
                    chosenDest = NightDestinationRegistry.Instance.GetRandomDestinationExcluding(currentDestination);
                }
            }

            currentDestination = chosenDest;

            if (currentDestination != null && agent != null && agent.isOnNavMesh)
            {
                Vector3 targetPos = currentDestination.GetRandomPosition();
                agent.SetDestination(targetPos);

                // Wait until arrived
                while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                {
                    yield return null;
                }

                // Wait at destination
                float waitTime = Random.Range(currentDestination.waitTimeMin, currentDestination.waitTimeMax);
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                // No destinations available, fallback wait
                yield return new WaitForSeconds(2f);
            }
        }
    }

    private IEnumerator DetectionRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(detectionInterval);

            if (playerRole == null || NightMemoryBank.Instance == null) continue;

            // Find all nearby colliders
            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
            
            foreach (var hit in hits)
            {
                // Don't detect ourselves
                if (hit.gameObject == gameObject) continue;

                // Check if it's an NPC
                PlayerRole otherRole = hit.GetComponent<PlayerRole>();
                if (otherRole != null)
                {
                    // Check line of sight
                    if (requireLineOfSight)
                    {
                        Vector3 dir = (hit.transform.position - transform.position).normalized;
                        float dist = Vector3.Distance(transform.position, hit.transform.position);
                        if (Physics.Raycast(transform.position + Vector3.up, dir, dist, sightObstacles))
                        {
                            // Blocked by wall
                            continue;
                        }
                    }

                    // Create memory
                    string area = currentDestination != null ? currentDestination.areaName : "Unknown";
                    float suspicion = currentDestination != null ? currentDestination.suspicionValue : 0f;
                    
                    // Simple normalized time (assuming ~30s night duration from NightTimer)
                    float timeOfNight = Mathf.Clamp01((Time.time - nightStartTime) / 30f);

                    NightMemory memory = new NightMemory(
                        playerRole.npcIndex,
                        otherRole.npcIndex,
                        timeOfNight,
                        area,
                        suspicion
                    );

                    NightMemoryBank.Instance.AddMemory(memory);
                }
            }
        }
    }
}
