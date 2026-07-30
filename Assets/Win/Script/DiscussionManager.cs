using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiscussionManager : MonoBehaviour
{
    [Header("Discussion UI")]
    public TMP_Text discussionLogText;
    public TMP_InputField playerDiscussionInput;
    public bool clearLogWhenDiscussionStarts = true;

    [Header("AI Cost and Pace")]
    [Tooltip("One API request is made for each line. Keep this low to control cost.")]
    [Range(1, 12)] public int maximumNpcLines = 5;
    [Tooltip("NPCs pause for a random time in this range between spoken lines.")]
    [Range(0f, 10f)] public float minimumSecondsBetweenLines = 2f;
    [Range(0f, 10f)] public float maximumSecondsBetweenLines = 5f;
    [Range(1, 8)] public int publicTranscriptLines = 4;
    [Range(80, 300)] public int maximumCharactersPerLine = 220;

    [Header("Player Responses")]
    [Range(1, 3)] public int minimumNpcResponses = 1;
    [Range(1, 3)] public int maximumNpcResponses = 3;

    [Header("Evidence")]
    public bool includeNightMemories = true;
    [Range(1, 6)] public int maximumNightMemoriesPerNpc = 3;

    private readonly List<string> publicTranscript = new List<string>();
    private readonly List<DiscussionNpcProfile> recentSpeakers = new List<DiscussionNpcProfile>();
    private readonly Queue<string> pendingPlayerMessages = new Queue<string>();
    private Coroutine discussionRoutine;
    private bool stopRequested;
    private bool discussionActive;

    // ── Discussion → Voting influence tracking ──
    public static readonly Dictionary<int, float> DynamicSuspicionModifiers = new Dictionary<int, float>();
    public static readonly HashSet<int> ClaimedSeers = new HashSet<int>();

    private static readonly string[] SightingKeywords =
    {
        "i saw", "saw", "spotted", "was near", "was at", "walked past", "were near", "were at", "noticed"
    };

    private static readonly string[] AccusationKeywords =
    {
        "guilty", "werewolf", "wolf", "killer", "murderer", "lying", "liar",
        "vote for", "vote out", "accuse", "dangerous", "cannot be trusted",
        "can't be trusted", "eliminate", "don't trust", "is the wolf", "is a wolf", "arsonist"
    };

    private static readonly string[] DefenseKeywords =
    {
        "innocent", "not guilty", "trust", "harmless", "safe",
        "not suspicious", "don't think", "isn't guilty",
        "protect", "defend", "leave them alone", "wasn't involved",
        "no proof", "not enough evidence", "coincidence"
    };

    private static readonly string[] SeerClaimKeywords =
    {
        "i am the seer", "i'm the seer", "as the seer", "as seer",
        "my vision", "my scan", "i scanned", "i checked", "seer ability",
        "i used my power", "i saw their role"
    };

    [Header("Discussion Influence")]
    [Tooltip("Suspicion added when someone merely mentions seeing/spotting an NPC ('I saw Anna'). Keep low so casual sightings don't force everyone to vote.")]
    [Range(0.05f, 1f)] public float sightingAccusationWeight = 0.2f;

    [Tooltip("Suspicion added for direct general accusations ('Anna is suspicious', 'Don't trust Anna').")]
    [Range(0.1f, 3f)] public float generalAccusationWeight = 0.6f;

    [Tooltip("Suspicion removed when someone defends an NPC ('Anna is innocent').")]
    [Range(0.1f, 3f)] public float defenseWeight = 0.5f;

    [Tooltip("Suspicion added when an NPC claims to be Seer and accuses someone ('I am the Seer! Anna is the Werewolf!').")]
    [Range(1f, 8f)] public float seerClaimWeight = 4.0f;

    private void Awake()
    {
        if (playerDiscussionInput != null)
            playerDiscussionInput.onSubmit.AddListener(HandlePlayerInputSubmitted);
    }

    private void OnDestroy()
    {
        if (playerDiscussionInput != null)
            playerDiscussionInput.onSubmit.RemoveListener(HandlePlayerInputSubmitted);
    }

    public void BeginDiscussion()
    {
        StopDiscussion();

        publicTranscript.Clear();
        recentSpeakers.Clear();
        pendingPlayerMessages.Clear();
        stopRequested = false;
        discussionActive = true;
        SetPlayerInputEnabled(true);
        ClearDiscussionInfluence();

        if (clearLogWhenDiscussionStarts && discussionLogText != null)
            discussionLogText.text = string.Empty;

        if (OpenAIManager.Instance == null || !OpenAIManager.Instance.HasValidApiKey)
        {
            AppendToLog("Discussion is unavailable: OpenAI API key is not configured.");
            return;
        }

        discussionRoutine = StartCoroutine(RunDiscussion());
    }

    public static void ClearDiscussionInfluence()
    {
        DynamicSuspicionModifiers.Clear();
        ClaimedSeers.Clear();
    }

    public void StopDiscussion()
    {
        stopRequested = true;
        discussionActive = false;
        SetPlayerInputEnabled(false);

        if (discussionRoutine != null)
        {
            StopCoroutine(discussionRoutine);
            discussionRoutine = null;
        }
    }

    public void SubmitPlayerDiscussionMessage()
    {
        if (!discussionActive || playerDiscussionInput == null)
            return;

        string message = CleanLine(playerDiscussionInput.text);
        if (string.IsNullOrWhiteSpace(message))
            return;

        string playerLine = "You: " + message;
        publicTranscript.Add(playerLine);
        AppendToLog(playerLine);
        AnalyzePlayerLineForInfluence(message);
        pendingPlayerMessages.Enqueue(message);
        playerDiscussionInput.text = string.Empty;
        playerDiscussionInput.ActivateInputField();
    }

    private void HandlePlayerInputSubmitted(string _)
    {
        SubmitPlayerDiscussionMessage();
    }

    private IEnumerator RunDiscussion()
    {
        List<DiscussionNpcProfile> speakers = DiscussionRoster.Instance != null
            ? DiscussionRoster.Instance.GetAliveProfiles()
            : new List<DiscussionNpcProfile>();
        Shuffle(speakers);

        if (speakers.Count == 0)
        {
            Debug.LogWarning("[DiscussionManager] No NPC discussion profiles were saved before the Vote scene loaded.");
            AppendToLog("No NPCs are ready to speak.");
            yield break;
        }

        int linesToGenerate = Mathf.Min(maximumNpcLines, speakers.Count);
        for (int i = 0; i < linesToGenerate && !stopRequested; i++)
        {
            if (pendingPlayerMessages.Count > 0)
                yield return RespondToPlayerMessages(speakers);

            DiscussionNpcProfile speaker = speakers[i];

            string prompt = BuildDiscussionPrompt(speaker);
            var messages = new List<ChatMessage>
            {
                new ChatMessage("developer", speaker.developerPrompt),
                new ChatMessage("user", prompt)
            };
            Task<string> responseTask = OpenAIManager.Instance.SendChatCompletionAsync(messages);

            while (!responseTask.IsCompleted && !stopRequested)
                yield return null;

            if (stopRequested)
                yield break;

            if (responseTask.IsFaulted)
            {
                Debug.LogWarning($"[DiscussionManager] {speaker.npcName} could not speak: {responseTask.Exception?.GetBaseException().Message}");
                AppendToLog(GetSpeakerLabel(speaker) + " could not speak.");
                continue;
            }

            if (responseTask.IsCanceled)
                continue;

            string line = CleanLine(responseTask.Result);
            if (string.IsNullOrEmpty(line))
                continue;

            string publicLine = GetSpeakerLabel(speaker) + ": " + line;
            publicTranscript.Add(publicLine);
            RememberSpeaker(speaker);
            AppendToLog(publicLine);
            AnalyzeLineForInfluence(speaker, line);

            if (pendingPlayerMessages.Count > 0 && !stopRequested)
                yield return RespondToPlayerMessages(speakers);

            // If another living NPC was mentioned or accused in this line, allow them to respond back directly!
            DiscussionNpcProfile mentionedTarget = FindAddressedOrMentionedNpc(speakers, line, speaker.npcIndex);
            if (mentionedTarget != null && !stopRequested)
            {
                yield return RespondToNpcMention(mentionedTarget, speaker, line);
            }

            yield return WaitForRandomLineDelay();
        }

        while (!stopRequested)
        {
            if (pendingPlayerMessages.Count > 0)
                yield return RespondToPlayerMessages(speakers);

            yield return null;
        }

        discussionRoutine = null;
    }

    private IEnumerator RespondToPlayerMessages(List<DiscussionNpcProfile> availableSpeakers)
    {
        while (pendingPlayerMessages.Count > 0 && !stopRequested)
        {
            string playerMessage = pendingPlayerMessages.Dequeue();

            List<DiscussionNpcProfile> responders = PickResponders(availableSpeakers, playerMessage);
            foreach (DiscussionNpcProfile responder in responders)
            {
                if (stopRequested)
                    yield break;

                string prompt = BuildPlayerResponsePrompt(responder, playerMessage);
                var messages = new List<ChatMessage>
                {
                    new ChatMessage("developer", responder.developerPrompt),
                    new ChatMessage("user", prompt)
                };
                Task<string> responseTask = OpenAIManager.Instance.SendChatCompletionAsync(messages);

                while (!responseTask.IsCompleted && !stopRequested)
                    yield return null;

                if (stopRequested)
                    yield break;

                if (responseTask.IsFaulted)
                {
                    Debug.LogWarning($"[DiscussionManager] {responder.npcName} could not answer the player: {responseTask.Exception?.GetBaseException().Message}");
                    continue;
                }

                if (responseTask.IsCanceled)
                    continue;

                string line = CleanLine(responseTask.Result);
                if (string.IsNullOrEmpty(line))
                    continue;

                string publicLine = GetSpeakerLabel(responder) + ": " + line;
                publicTranscript.Add(publicLine);
                RememberSpeaker(responder);
                AppendToLog(publicLine);
                AnalyzeLineForInfluence(responder, line);

                yield return WaitForRandomLineDelay();
            }
        }
    }

    private List<DiscussionNpcProfile> PickResponders(List<DiscussionNpcProfile> availableSpeakers, string playerMessage)
    {
        DiscussionNpcProfile addressedNpc = FindAddressedNpc(availableSpeakers, playerMessage);
        if (addressedNpc != null)
            return new List<DiscussionNpcProfile> { addressedNpc };

        int minimum = Mathf.Min(minimumNpcResponses, availableSpeakers.Count);
        int maximum = Mathf.Min(Mathf.Max(minimum, maximumNpcResponses), availableSpeakers.Count);
        int responseCount = UnityEngine.Random.Range(minimum, maximum + 1);

        var responders = new List<DiscussionNpcProfile>();
        for (int i = recentSpeakers.Count - 1; i >= 0 && responders.Count < responseCount; i--)
        {
            DiscussionNpcProfile recentSpeaker = recentSpeakers[i];
            if (availableSpeakers.Contains(recentSpeaker) && !responders.Contains(recentSpeaker))
                responders.Add(recentSpeaker);
        }

        var remaining = new List<DiscussionNpcProfile>(availableSpeakers);
        Shuffle(remaining);
        foreach (DiscussionNpcProfile candidate in remaining)
        {
            if (responders.Count >= responseCount)
                break;

            if (!responders.Contains(candidate))
                responders.Add(candidate);
        }

        return responders;
    }

    private DiscussionNpcProfile FindAddressedNpc(List<DiscussionNpcProfile> availableSpeakers, string message)
    {
        foreach (DiscussionNpcProfile speaker in availableSpeakers)
        {
            string name = speaker.npcName;
            if (string.IsNullOrWhiteSpace(name))
                continue;

            int position = message.IndexOf(name, System.StringComparison.OrdinalIgnoreCase);
            if (position < 0)
                continue;

            int afterName = position + name.Length;
            bool startsAtWordBoundary = position == 0 || !char.IsLetterOrDigit(message[position - 1]);
            bool endsAtWordBoundary = afterName >= message.Length || !char.IsLetterOrDigit(message[afterName]);
            if (startsAtWordBoundary && endsAtWordBoundary)
                return speaker;
        }

        return null;
    }

    private string BuildPlayerResponsePrompt(DiscussionNpcProfile speaker, string playerMessage)
    {
        var prompt = new StringBuilder(BuildDiscussionPrompt(speaker));
        prompt.AppendLine();
        prompt.AppendLine("The player has just spoken publicly. Reply directly to their latest message or its implication.");
        prompt.AppendLine("Do not repeat a previous statement. Stay consistent with the public transcript and your own evidence.");
        prompt.AppendLine("Latest player message: " + playerMessage);
        return prompt.ToString();
    }

    private DiscussionNpcProfile FindAddressedOrMentionedNpc(List<DiscussionNpcProfile> availableSpeakers, string line, int speakerNpcIndex)
    {
        foreach (DiscussionNpcProfile candidate in availableSpeakers)
        {
            if (candidate.npcIndex == speakerNpcIndex)
                continue;

            string name = candidate.npcName;
            if (string.IsNullOrWhiteSpace(name))
                continue;

            int position = line.IndexOf(name, System.StringComparison.OrdinalIgnoreCase);
            if (position < 0)
                continue;

            int afterName = position + name.Length;
            bool startsAtWordBoundary = position == 0 || !char.IsLetterOrDigit(line[position - 1]);
            bool endsAtWordBoundary = afterName >= line.Length || !char.IsLetterOrDigit(line[afterName]);
            if (startsAtWordBoundary && endsAtWordBoundary)
                return candidate;
        }

        return null;
    }

    private string BuildNpcResponsePrompt(DiscussionNpcProfile responder, DiscussionNpcProfile speaker, string speakerLine)
    {
        var prompt = new StringBuilder(BuildDiscussionPrompt(responder));
        prompt.AppendLine();
        prompt.AppendLine($"{speaker.npcName} just mentioned, questioned, or accused you publicly: \"{speakerLine}\"");
        prompt.AppendLine("Reply directly to them! Answer their question, defend yourself, or question their motives.");
        prompt.AppendLine("If defending your location, state a real village location (e.g. Main Square, Bakery, Graveyard, Mine, Church, Docks, Forest Edge) or give a reason. NEVER say 'at home'.");
        prompt.AppendLine("Do not just give your location — use other defenses too (e.g. question their proof, point out a contradiction, or counter-ask).");
        prompt.AppendLine("If they asked a calm or friendly question, answer normally and calmly.");
        return prompt.ToString();
    }

    private IEnumerator RespondToNpcMention(DiscussionNpcProfile targetNpc, DiscussionNpcProfile speaker, string speakerLine)
    {
        if (stopRequested || targetNpc == null || speaker == null)
            yield break;

        yield return WaitForRandomLineDelay();

        if (stopRequested)
            yield break;

        string prompt = BuildNpcResponsePrompt(targetNpc, speaker, speakerLine);
        var messages = new List<ChatMessage>
        {
            new ChatMessage("developer", targetNpc.developerPrompt),
            new ChatMessage("user", prompt)
        };

        Task<string> responseTask = OpenAIManager.Instance.SendChatCompletionAsync(messages);
        while (!responseTask.IsCompleted && !stopRequested)
            yield return null;

        if (stopRequested || responseTask.IsFaulted || responseTask.IsCanceled)
            yield break;

        string line = CleanLine(responseTask.Result);
        if (string.IsNullOrWhiteSpace(line))
            yield break;

        string publicLine = GetSpeakerLabel(targetNpc) + ": " + line;
        publicTranscript.Add(publicLine);
        RememberSpeaker(targetNpc);
        AppendToLog(publicLine);
        AnalyzeLineForInfluence(targetNpc, line);
    }

    private void RememberSpeaker(DiscussionNpcProfile speaker)
    {
        recentSpeakers.Remove(speaker);
        recentSpeakers.Add(speaker);

        const int recentSpeakerLimit = 3;
        if (recentSpeakers.Count > recentSpeakerLimit)
            recentSpeakers.RemoveAt(0);
    }

    private string BuildDiscussionPrompt(DiscussionNpcProfile speaker)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine("This is the public village discussion before a vote.");
        prompt.AppendLine("Speak exactly one natural line, at most 22 words. Do not repeat previous statements.");
        prompt.AppendLine("Vary your communication style. Pick one that fits your personality and evidence:");
        prompt.AppendLine("- Direct witness: 'I saw [Name] near the forest last night.'");
        prompt.AppendLine("- Uncertain witness: 'I think I saw [Name]... it was dark.'");
        prompt.AppendLine("- Suspicion: 'Something felt strange about [Name].'");
        prompt.AppendLine("- Defending someone or yourself: 'I don't think [Name] is guilty.' / 'I wasn't near the forest, I was at the square.'");
        prompt.AppendLine("- Disagreement: 'Seeing someone outside isn't proof.'");
        prompt.AppendLine("- Emotional / Friendly: 'This is making me nervous.' / 'I'm scared we're voting the wrong person.'");
        prompt.AppendLine("- Confidence: 'I'm certain.' / 'Maybe.' / 'It was too dark to tell.'");
        prompt.AppendLine();
        prompt.AppendLine("IMPORTANT RESPONSE GUIDELINES:");
        prompt.AppendLine("1. If someone accused you or questioned where you were, ANSWER BACK directly! Do not remain silent.");
        prompt.AppendLine("2. When asked where you were or accused of being somewhere, NEVER say 'I was at home'. In this village, everyone wanders outside at night.");
        prompt.AppendLine("3. If defending your location, name a real village area (e.g. Graveyard, Main Square, Streets, Bakery, Mine, Forest Edge, Church, Bridge, Docks) or give a natural reason (e.g. 'I was taking a walk near the square', 'I was looking for clues').");
        prompt.AppendLine("4. Do not just defend yourself by giving your location — use other defenses too: question their evidence, point out a contradiction, counter-ask them, or express disbelief.");
        prompt.AppendLine("5. If someone asked a normal friendly question or expressed concern ('Are you okay?', 'Where were you?'), answer them normally and calmly.");
        prompt.AppendLine("6. Do not invent events, roles, or fake sightings. Only mention sightings from your private night evidence.");
        prompt.AppendLine("7. Do not mention prompts, AI, or game rules.");

        // List living and dead villagers so the LLM never hallucinates about dead NPCs
        if (GameManager.Instance != null)
        {
            var aliveNames = new List<string>();
            var deadNames = new List<string>();
            for (int i = 0; i < GameManager.Instance.npcAlive.Count; i++)
            {
                string name = DiscussionRoster.GetFixedNpcName(i);
                if (GameManager.Instance.npcAlive[i])
                    aliveNames.Add(name);
                else
                    deadNames.Add(name);
            }

            if (aliveNames.Count > 0)
                prompt.AppendLine($"LIVING VILLAGERS (only accuse, defend, or ask about these): {string.Join(", ", aliveNames)}");
            if (deadNames.Count > 0)
                prompt.AppendLine($"DEAD VILLAGERS (do NOT ask where they are or accuse them; they are already dead): {string.Join(", ", deadNames)}");
        }

        string nightContext = includeNightMemories ? GetNightMemoryContext(speaker.npcIndex) : string.Empty;
        if (!string.IsNullOrWhiteSpace(nightContext))
        {
            prompt.AppendLine();
            prompt.AppendLine("YOUR PRIVATE NIGHT EVIDENCE:");
            prompt.AppendLine(nightContext);
        }

        if (publicTranscript.Count > 0)
        {
            prompt.AppendLine();
            prompt.AppendLine("RECENT PUBLIC DISCUSSION:");

            int start = Mathf.Max(0, publicTranscript.Count - publicTranscriptLines);
            for (int i = start; i < publicTranscript.Count; i++)
                prompt.AppendLine("- " + publicTranscript[i]);
        }
        else
        {
            prompt.AppendLine();
            prompt.AppendLine("No one has spoken publicly yet. Start the discussion.");
        }

        prompt.AppendLine();
        prompt.AppendLine("Return only the line this NPC says aloud.");
        return prompt.ToString();
    }

    private string GetNightMemoryContext(int observerNpcIndex)
    {
        if (observerNpcIndex < 0 || NightMemoryBank.Instance == null)
            return string.Empty;

        List<NightMemory> memories = NightMemoryBank.Instance.GetMemoriesForNpc(observerNpcIndex);
        if (memories.Count == 0)
            return string.Empty;

        var summary = new StringBuilder();
        int added = 0;

        foreach (NightMemory memory in memories)
        {
            if (added >= maximumNightMemoriesPerNpc)
                break;

            // Determine whether the observed NPC is still alive
            bool isTargetAlive = true;
            if (memory.observedNpcIndex >= 0 && GameManager.Instance != null &&
                memory.observedNpcIndex < GameManager.Instance.npcAlive.Count)
            {
                isTargetAlive = GameManager.Instance.npcAlive[memory.observedNpcIndex];
            }

            string time = memory.timeOfNight < 0.3f ? "early at night" :
                memory.timeOfNight > 0.7f ? "late at night" : "in the middle of the night";
            string location = string.IsNullOrWhiteSpace(memory.areaName) ? "an unknown area" : memory.areaName;
            string suspicion = memory.wasInSuspiciousArea ? " It was a suspicious area." : string.Empty;

            if (isTargetAlive)
            {
                summary.AppendLine($"- You saw {GetNpcName(memory.observedNpcIndex)} near {location} {time}.{suspicion}");
            }
            else
            {
                // Present the memory as historical context — the target is now dead
                summary.AppendLine($"- Before {GetNpcName(memory.observedNpcIndex)} died, you saw them near {location} {time}.{suspicion}");
            }

            added++;
        }

        return summary.ToString().Trim();
    }

    private string GetNpcName(int npcIndex)
    {
        return DiscussionRoster.Instance != null
            ? DiscussionRoster.Instance.GetNpcName(npcIndex)
            : npcIndex == -1 ? "the player" : DiscussionRoster.GetFixedNpcName(npcIndex);
    }

    private string GetSpeakerLabel(DiscussionNpcProfile speaker)
    {
        string name = string.IsNullOrWhiteSpace(speaker.npcName)
            ? DiscussionRoster.GetFixedNpcName(speaker.npcIndex)
            : speaker.npcName;

        return name;
    }

    private void AppendToLog(string line)
    {
        if (discussionLogText == null)
        {
            Debug.Log("[Discussion] " + line);
            return;
        }

        if (string.IsNullOrEmpty(discussionLogText.text))
            discussionLogText.text = line;
        else
            discussionLogText.text += "\n\n" + line;

        ScrollRect scrollRect = discussionLogText.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void SetPlayerInputEnabled(bool enabled)
    {
        if (playerDiscussionInput == null)
            return;

        playerDiscussionInput.interactable = enabled;
        if (enabled)
            playerDiscussionInput.ActivateInputField();
    }

    private IEnumerator WaitForRandomLineDelay()
    {
        float minimum = Mathf.Min(minimumSecondsBetweenLines, maximumSecondsBetweenLines);
        float maximum = Mathf.Max(minimumSecondsBetweenLines, maximumSecondsBetweenLines);
        yield return new WaitForSeconds(UnityEngine.Random.Range(minimum, maximum));
    }

    private string CleanLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return string.Empty;

        string cleanLine = line.Replace("\r", " ").Replace("\n", " ").Trim();
        if (cleanLine.Length > maximumCharactersPerLine)
            cleanLine = cleanLine.Substring(0, maximumCharactersPerLine).TrimEnd() + "...";

        return cleanLine;
    }

    private void Shuffle<T>(List<T> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, items.Count);
            T temporary = items[i];
            items[i] = items[randomIndex];
            items[randomIndex] = temporary;
        }
    }

    // ── Discussion influence parsing ──

    /// <summary>
    /// Analyzes a spoken discussion line to detect accusations, defenses, sightings, and Seer claims.
    /// Updates DynamicSuspicionModifiers so votes reflect the discussion appropriately.
    /// </summary>
    private void AnalyzeLineForInfluence(DiscussionNpcProfile speaker, string line)
    {
        if (string.IsNullOrWhiteSpace(line) || GameManager.Instance == null)
            return;

        string lowerLine = line.ToLowerInvariant();

        // Detect Seer claim from this speaker
        bool isSeerClaim = false;
        foreach (string keyword in SeerClaimKeywords)
        {
            if (lowerLine.Contains(keyword))
            {
                isSeerClaim = true;
                ClaimedSeers.Add(speaker.npcIndex);
                Debug.Log($"[Discussion Influence] {speaker.npcName} claimed to be the Seer!");
                break;
            }
        }

        // Build list of living NPC names to search for in the line
        for (int i = 0; i < GameManager.Instance.npcAlive.Count; i++)
        {
            if (i == speaker.npcIndex)
                continue; // Skip self-references

            if (!GameManager.Instance.npcAlive[i])
                continue; // Skip dead NPCs

            string targetName = DiscussionRoster.GetFixedNpcName(i);
            if (string.IsNullOrWhiteSpace(targetName))
                continue;

            // Check if this NPC is mentioned in the line
            if (lowerLine.IndexOf(targetName.ToLowerInvariant(), StringComparison.Ordinal) < 0)
                continue;

            // Check for direct accusation keywords
            bool directAccused = false;
            foreach (string keyword in AccusationKeywords)
            {
                if (lowerLine.Contains(keyword))
                {
                    directAccused = true;
                    break;
                }
            }

            // Check for casual sighting keywords
            bool casualSighting = false;
            if (!directAccused)
            {
                foreach (string keyword in SightingKeywords)
                {
                    if (lowerLine.Contains(keyword))
                    {
                        casualSighting = true;
                        break;
                    }
                }
            }

            // Check for defense keywords
            bool defended = false;
            foreach (string keyword in DefenseKeywords)
            {
                if (lowerLine.Contains(keyword))
                {
                    defended = true;
                    break;
                }
            }

            if (directAccused || casualSighting)
            {
                float weight = isSeerClaim ? seerClaimWeight : (directAccused ? generalAccusationWeight : sightingAccusationWeight);
                AddSuspicionModifier(i, weight);
                string typeStr = isSeerClaim ? "Seer Claim" : (directAccused ? "Direct Accusation" : "Casual Sighting");
                Debug.Log($"[Discussion Influence] {speaker.npcName} mentioned {targetName} ({typeStr}, weight: +{weight})");
            }

            if (defended)
            {
                AddSuspicionModifier(i, -defenseWeight);
                Debug.Log($"[Discussion Influence] {speaker.npcName} defended {targetName} (weight: -{defenseWeight})");
            }
        }

        // Also check if the player is mentioned
        if (lowerLine.Contains("player") || lowerLine.Contains("you"))
        {
            bool directAccusedPlayer = false;
            foreach (string keyword in AccusationKeywords)
            {
                if (lowerLine.Contains(keyword))
                {
                    directAccusedPlayer = true;
                    break;
                }
            }

            bool casualSightingPlayer = false;
            if (!directAccusedPlayer)
            {
                foreach (string keyword in SightingKeywords)
                {
                    if (lowerLine.Contains(keyword))
                    {
                        casualSightingPlayer = true;
                        break;
                    }
                }
            }

            bool defendedPlayer = false;
            foreach (string keyword in DefenseKeywords)
            {
                if (lowerLine.Contains(keyword))
                {
                    defendedPlayer = true;
                    break;
                }
            }

            if (directAccusedPlayer || casualSightingPlayer)
            {
                float weight = isSeerClaim ? seerClaimWeight : (directAccusedPlayer ? generalAccusationWeight : sightingAccusationWeight);
                AddSuspicionModifier(-1, weight);
                string typeStr = isSeerClaim ? "Seer Claim" : (directAccusedPlayer ? "Direct Accusation" : "Casual Sighting");
                Debug.Log($"[Discussion Influence] {speaker.npcName} mentioned Player ({typeStr}, weight: +{weight})");
            }

            if (defendedPlayer)
            {
                AddSuspicionModifier(-1, -defenseWeight);
                Debug.Log($"[Discussion Influence] {speaker.npcName} defended the Player (weight: -{defenseWeight})");
            }
        }
    }

    /// <summary>
    /// Analyzes a player-spoken line for influence on the vote.
    /// </summary>
    private void AnalyzePlayerLineForInfluence(string line)
    {
        if (string.IsNullOrWhiteSpace(line) || GameManager.Instance == null)
            return;

        string lowerLine = line.ToLowerInvariant();

        // Check if player claims Seer
        bool isSeerClaim = false;
        foreach (string keyword in SeerClaimKeywords)
        {
            if (lowerLine.Contains(keyword))
            {
                isSeerClaim = true;
                Debug.Log("[Discussion Influence] Player claimed to be the Seer!");
                break;
            }
        }

        for (int i = 0; i < GameManager.Instance.npcAlive.Count; i++)
        {
            if (!GameManager.Instance.npcAlive[i])
                continue;

            string targetName = DiscussionRoster.GetFixedNpcName(i);
            if (string.IsNullOrWhiteSpace(targetName))
                continue;

            if (lowerLine.IndexOf(targetName.ToLowerInvariant(), StringComparison.Ordinal) < 0)
                continue;

            bool directAccused = false;
            foreach (string keyword in AccusationKeywords)
            {
                if (lowerLine.Contains(keyword))
                {
                    directAccused = true;
                    break;
                }
            }

            bool casualSighting = false;
            if (!directAccused)
            {
                foreach (string keyword in SightingKeywords)
                {
                    if (lowerLine.Contains(keyword))
                    {
                        casualSighting = true;
                        break;
                    }
                }
            }

            bool defended = false;
            foreach (string keyword in DefenseKeywords)
            {
                if (lowerLine.Contains(keyword))
                {
                    defended = true;
                    break;
                }
            }

            if (directAccused || casualSighting)
            {
                float weight = isSeerClaim ? seerClaimWeight : (directAccused ? generalAccusationWeight : sightingAccusationWeight);
                AddSuspicionModifier(i, weight);
                string typeStr = isSeerClaim ? "Seer Claim" : (directAccused ? "Direct Accusation" : "Casual Sighting");
                Debug.Log($"[Discussion Influence] Player mentioned {targetName} ({typeStr}, weight: +{weight})");
            }

            if (defended)
            {
                AddSuspicionModifier(i, -defenseWeight);
                Debug.Log($"[Discussion Influence] Player defended {targetName} (weight: -{defenseWeight})");
            }
        }
    }

    private static void AddSuspicionModifier(int targetIndex, float amount)
    {
        if (DynamicSuspicionModifiers.ContainsKey(targetIndex))
            DynamicSuspicionModifiers[targetIndex] += amount;
        else
            DynamicSuspicionModifiers[targetIndex] = amount;
    }
}
