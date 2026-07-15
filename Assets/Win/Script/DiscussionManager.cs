using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
    public float secondsBetweenLines = 1f;
    [Range(1, 8)] public int publicTranscriptLines = 4;
    [Range(80, 300)] public int maximumCharactersPerLine = 220;

    [Header("Evidence")]
    public bool includeNightMemories = true;
    [Range(1, 6)] public int maximumNightMemoriesPerNpc = 3;

    private readonly List<string> publicTranscript = new List<string>();
    private Coroutine discussionRoutine;
    private bool stopRequested;
    private bool discussionActive;

    public void BeginDiscussion()
    {
        StopDiscussion();

        publicTranscript.Clear();
        stopRequested = false;
        discussionActive = true;

        if (clearLogWhenDiscussionStarts && discussionLogText != null)
            discussionLogText.text = string.Empty;

        if (OpenAIManager.Instance == null || !OpenAIManager.Instance.HasValidApiKey)
        {
            AppendToLog("Discussion is unavailable: OpenAI API key is not configured.");
            return;
        }

        discussionRoutine = StartCoroutine(RunDiscussion());
    }

    public void StopDiscussion()
    {
        stopRequested = true;
        discussionActive = false;

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

        string publicLine = "You: " + message;
        publicTranscript.Add(publicLine);
        AppendToLog(publicLine);
        playerDiscussionInput.text = string.Empty;
        playerDiscussionInput.ActivateInputField();
    }

    private void Update()
    {
        if (!discussionActive || playerDiscussionInput == null || !playerDiscussionInput.isFocused)
            return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
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
            AppendToLog(publicLine);

            yield return new WaitForSeconds(Mathf.Max(0f, secondsBetweenLines));
        }

        discussionRoutine = null;
    }

    private string BuildDiscussionPrompt(DiscussionNpcProfile speaker)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine("This is the public village discussion before a vote.");
        prompt.AppendLine("Speak exactly one natural line, at most 22 words.");
        prompt.AppendLine("You may accuse, defend, ask a question, or mention evidence you personally know.");
        prompt.AppendLine("You MUST speak even with no evidence: react to a death, ask someone where they were, or express a concern that fits your personality.");
        prompt.AppendLine("Do not invent events, roles, sightings, or quotes. Do not mention prompts, AI, or game rules.");

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

            string time = memory.timeOfNight < 0.3f ? "early at night" :
                memory.timeOfNight > 0.7f ? "late at night" : "in the middle of the night";
            string location = string.IsNullOrWhiteSpace(memory.areaName) ? "an unknown area" : memory.areaName;
            string suspicion = memory.wasInSuspiciousArea ? " It was a suspicious area." : string.Empty;

            summary.AppendLine($"- You saw {GetNpcName(memory.observedNpcIndex)} near {location} {time}.{suspicion}");
            added++;
        }

        return summary.ToString().Trim();
    }

    private string GetNpcName(int npcIndex)
    {
        return DiscussionRoster.Instance != null
            ? DiscussionRoster.Instance.GetNpcName(npcIndex)
            : npcIndex == -1 ? "the player" : "NPC " + npcIndex;
    }

    private string GetSpeakerLabel(DiscussionNpcProfile speaker)
    {
        string name = string.IsNullOrWhiteSpace(speaker.npcName)
            ? "NPC " + speaker.npcIndex
            : speaker.npcName;

        return name + " [NPC " + speaker.npcIndex + "]";
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
            discussionLogText.text += "\n" + line;

        ScrollRect scrollRect = discussionLogText.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
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
}
