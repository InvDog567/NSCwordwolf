// Assets/kin/OpenAI/Scripts/NPCConversationAwareness.cs

using System;
using UnityEngine;

public static class NPCConversationAwareness
{
    private const int MaximumSnippetLength = 72;

    public static void ShareConversation(NPCChatController speaker, string npcReply,
        float hearingDistance, LayerMask hearingLayers)
    {
        if (speaker == null || string.IsNullOrWhiteSpace(npcReply))
            return;

        NPCMemory speakerMemory = speaker.GetComponent<NPCMemory>();
        Vector3 sourcePoint = GetPoint(speaker.transform);
        NPCMemory[] listeners = UnityEngine.Object.FindObjectsByType<NPCMemory>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (NPCMemory listener in listeners)
        {
            if (listener == null || listener == speakerMemory || !CanHear(sourcePoint, speakerMemory,
                    listener, hearingDistance, hearingLayers))
            {
                continue;
            }

            string memory = $"You overheard part of {speaker.NpcName}'s conversation with the player: \"{CreateSnippet(npcReply)}\"";
            listener.Remember(memory);
            NPCOverheardSubtitle.Show(listener, CreateSubtitleSnippet(npcReply));
        }
    }

    private static bool CanHear(Vector3 sourcePoint, NPCMemory speaker, NPCMemory listener,
        float hearingDistance, LayerMask hearingLayers)
    {
        Collider listenerCollider = listener.GetComponentInChildren<Collider>();
        if (listenerCollider == null || !listenerCollider.enabled)
            return false;

        Vector3 listenerPoint = listenerCollider.bounds.center;
        Vector3 direction = listenerPoint - sourcePoint;
        if (direction.sqrMagnitude > hearingDistance * hearingDistance)
            return false;

        RaycastHit[] hits = Physics.RaycastAll(sourcePoint, direction.normalized, direction.magnitude,
            hearingLayers, QueryTriggerInteraction.Ignore);
        Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

        foreach (RaycastHit hit in hits)
        {
            NPCMemory hitNpc = hit.collider.GetComponentInParent<NPCMemory>();
            if (hitNpc == speaker)
                continue;

            return hitNpc == listener;
        }

        return false;
    }

    private static Vector3 GetPoint(Transform target)
    {
        Collider collider = target.GetComponentInChildren<Collider>();
        return collider != null ? collider.bounds.center : target.position + Vector3.up;
    }

    private static string CreateSnippet(string text)
    {
        string cleanText = text.Trim();
        if (cleanText.Length <= MaximumSnippetLength)
            return cleanText;

        return cleanText.Substring(0, MaximumSnippetLength).TrimEnd() + "...";
    }

    private static string CreateSubtitleSnippet(string text)
    {
        return "Overheard: \"" + CreateSnippet(text) + "\"";
    }
}
