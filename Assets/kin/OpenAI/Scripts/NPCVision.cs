// Assets/kin/OpenAI/Scripts/NPCVision.cs

using UnityEngine;

[RequireComponent(typeof(NPCMemory))]
public class NPCVision : MonoBehaviour
{
    [Header("Vision")]
    [SerializeField] private Transform eyePoint;
    [SerializeField] private float viewDistance = 8f;
    [Range(1f, 360f)]
    [SerializeField] private float viewAngle = 110f;
    [SerializeField] private float scanIntervalSeconds = 1f;
    [SerializeField] private LayerMask visionLayers = Physics.DefaultRaycastLayers;

    private NPCMemory _memory;
    private float _nextScanTime;

    private void Awake()
    {
        _memory = GetComponent<NPCMemory>();
    }

    private void Update()
    {
        if (Time.time < _nextScanTime)
            return;

        _nextScanTime = Time.time + Mathf.Max(0.1f, scanIntervalSeconds);
        ScanForOtherNPCs();
    }

    private void ScanForOtherNPCs()
    {
        NPCMemory[] allNPCs = FindObjectsByType<NPCMemory>(FindObjectsSortMode.None);

        foreach (NPCMemory otherNPC in allNPCs)
        {
            if (otherNPC == _memory || !CanSee(otherNPC))
                continue;

            string otherName = GetNPCName(otherNPC);
            _memory.Remember($"You recently saw {otherName} nearby.");
        }
    }

    private bool CanSee(NPCMemory target)
    {
        Vector3 origin = eyePoint != null ? eyePoint.position : transform.position + Vector3.up * 1.6f;
        Collider targetCollider = target.GetComponentInChildren<Collider>();
        if (targetCollider == null || !targetCollider.enabled)
            return false;

        Vector3 targetPoint = targetCollider.bounds.center;
        Vector3 direction = targetPoint - origin;

        if (direction.sqrMagnitude > viewDistance * viewDistance)
            return false;

        if (Vector3.Angle(transform.forward, direction) > viewAngle * 0.5f)
            return false;

        if (!Physics.Raycast(origin, direction.normalized, out RaycastHit hit, direction.magnitude, visionLayers,
                QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        NPCMemory visibleNPC = hit.collider.GetComponentInParent<NPCMemory>();
        return visibleNPC == target;
    }

    private string GetNPCName(NPCMemory npc)
    {
        NPCChatController chatController = npc.GetComponent<NPCChatController>();
        return chatController != null ? chatController.NpcName : npc.gameObject.name;
    }
}
