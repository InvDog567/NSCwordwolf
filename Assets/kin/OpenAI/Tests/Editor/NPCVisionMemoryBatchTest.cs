using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class NPCVisionMemoryBatchTest
{
    private const string ScenePath = "Assets/kin/sence/t2.unity";

    public static void Run()
    {
        int failures = 0;

        try
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            NPCMemory[] memories = UnityEngine.Object.FindObjectsByType<NPCMemory>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            NPCMemory[] usableNPCs = memories
                .Where(memory => memory.enabled && memory.GetComponentInChildren<Collider>() != null)
                .Take(2)
                .ToArray();

            failures += Check(memories.Length >= 4,
                $"Scene contains NPCMemory components (found {memories.Length}, expected at least 4).");
            failures += Check(usableNPCs.Length == 2,
                "Scene contains at least two enabled NPCMemory components with Colliders.");

            if (usableNPCs.Length == 2)
                failures += RunVisionTests(usableNPCs[0], usableNPCs[1]);
        }
        catch (Exception exception)
        {
            failures++;
            Debug.LogError($"[NPC TEST] Unexpected exception: {exception}");
        }

        Debug.Log(failures == 0
            ? "[NPC TEST] RESULT: PASS - NPCVision and NPCMemory work in t2."
            : $"[NPC TEST] RESULT: FAIL - {failures} check(s) failed.");
        EditorApplication.Exit(failures == 0 ? 0 : 1);
    }

    public static void RunVotePrototype()
    {
        int failures = 0;

        try
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject root = new GameObject("Vote Prototype Test");
            SimpleVotePrototypeUI prototype = root.AddComponent<SimpleVotePrototypeUI>();
            InvokeLifecycle(prototype, "Awake");
            InvokeLifecycle(prototype, "Start");

            SimpleVoteManager manager = root.GetComponent<SimpleVoteManager>();
            failures += Check(manager != null, "Vote prototype creates a SimpleVoteManager.");
            failures += Check(manager != null && manager.Candidates.Count >= 4,
                "Vote prototype finds at least four NPC candidates in t2.");

            Canvas canvas = root.GetComponentInChildren<Canvas>(true);
            SimpleVoteButton[] buttons = root.GetComponentsInChildren<SimpleVoteButton>(true);
            failures += Check(canvas != null, "Vote prototype creates a runtime Canvas.");
            failures += Check(buttons.Length >= 4, "Vote prototype creates one vote button per NPC.");

            if (manager != null && manager.Candidates.Count > 0)
            {
                FieldInfo randomVotesField = typeof(SimpleVoteManager).GetField(
                    "includeRandomNpcVotes", BindingFlags.Instance | BindingFlags.NonPublic);
                randomVotesField?.SetValue(manager, false);

                SimpleVoteCandidate chosen = manager.Candidates[0];
                PlayerRole chosenRole = chosen.GetComponent<PlayerRole>();
                chosenRole.currentRole = PlayerRole.Role.Werewolf;

                manager.SubmitVote(chosen);
                failures += Check(manager.HasVoted, "Clicking a candidate submits exactly one player vote.");
                failures += Check(!chosen.IsEliminated,
                    "Choosing the werewolf produces a correct result without eliminating the wrong NPC.");
            }

            UnityEngine.Object.DestroyImmediate(root);
        }
        catch (Exception exception)
        {
            failures++;
            Debug.LogError($"[VOTE TEST] Unexpected exception: {exception}");
        }

        Debug.Log(failures == 0
            ? "[VOTE TEST] RESULT: PASS - Vote prototype works in t2."
            : $"[VOTE TEST] RESULT: FAIL - {failures} check(s) failed.");
        EditorApplication.Exit(failures == 0 ? 0 : 1);
    }

    public static void RunChatSwitching()
    {
        int failures = 0;

        try
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            NPCChatUI chatUI = UnityEngine.Object.FindFirstObjectByType<NPCChatUI>(
                FindObjectsInactive.Include);
            NPCChatController[] controllers = UnityEngine.Object.FindObjectsByType<NPCChatController>(
                FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(controller => controller.enabled)
                .Take(2)
                .ToArray();

            failures += Check(chatUI != null, "t2 contains the shared NPCChatUI.");
            failures += Check(controllers.Length == 2, "t2 contains at least two enabled NPCChatController components.");

            if (chatUI != null && controllers.Length == 2)
            {
                const string npcOneReply = "NPC1 remembers this conversation.";
                FieldInfo responseField = typeof(NPCChatController).GetField(
                    "_lastResponse", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo displayedReplyField = typeof(NPCChatUI).GetField(
                    "_lastOriginalReply", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo replyTextField = typeof(NPCChatUI).GetField(
                    "npcReplyText", BindingFlags.Instance | BindingFlags.NonPublic);

                responseField?.SetValue(controllers[0], npcOneReply);
                chatUI.SetActiveChatController(controllers[0]);
                chatUI.SetActiveChatController(controllers[1]);
                chatUI.SetActiveChatController(controllers[0]);

                string displayedReply = displayedReplyField?.GetValue(chatUI) as string;
                failures += Check(displayedReply == npcOneReply,
                    "Switching NPC1 to NPC2 and back restores NPC1's previous reply.");

                TMP_Text replyText = replyTextField?.GetValue(chatUI) as TMP_Text;
                failures += Check(replyText != null && replyText.text == npcOneReply,
                    "The shared chat UI visibly shows NPC1's restored reply.");
            }
        }
        catch (Exception exception)
        {
            failures++;
            Debug.LogError($"[CHAT SWITCH TEST] Unexpected exception: {exception}");
        }

        Debug.Log(failures == 0
            ? "[CHAT SWITCH TEST] RESULT: PASS - Chat display persists per NPC."
            : $"[CHAT SWITCH TEST] RESULT: FAIL - {failures} check(s) failed.");
        EditorApplication.Exit(failures == 0 ? 0 : 1);
    }

    public static void RunConversationAwareness()
    {
        int failures = 0;

        try
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            NPCChatController[] controllers = UnityEngine.Object.FindObjectsByType<NPCChatController>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(controller => controller.GetComponent<NPCMemory>() != null &&
                                     controller.GetComponentInChildren<Collider>() != null)
                .Take(2)
                .ToArray();

            failures += Check(controllers.Length == 2,
                "t2 contains two NPCs ready for the overhearing test.");

            if (controllers.Length == 2)
                failures += RunOverhearingTests(controllers[0], controllers[1]);
        }
        catch (Exception exception)
        {
            failures++;
            Debug.LogError($"[OVERHEAR TEST] Unexpected exception: {exception}");
        }

        Debug.Log(failures == 0
            ? "[OVERHEAR TEST] RESULT: PASS - Nearby NPCs hear partial dialogue correctly."
            : $"[OVERHEAR TEST] RESULT: FAIL - {failures} check(s) failed.");
        EditorApplication.Exit(failures == 0 ? 0 : 1);
    }

    private static int RunOverhearingTests(NPCChatController speaker, NPCChatController listenerController)
    {
        int failures = 0;
        NPCMemory listenerMemory = listenerController.GetComponent<NPCMemory>();
        Transform speakerTransform = speaker.transform;
        Transform listenerTransform = listenerController.transform;
        Vector3 speakerPosition = speakerTransform.position;
        Vector3 listenerPosition = listenerTransform.position;
        GameObject wall = null;

        try
        {
            speakerTransform.position = new Vector3(0f, 100f, 0f);
            listenerTransform.position = new Vector3(0f, 100f, 3f);
            Physics.SyncTransforms();

            listenerMemory.ClearMemories();
            NPCConversationAwareness.ShareConversation(speaker,
                "I saw someone near the village square after dark.", 7f, Physics.DefaultRaycastLayers);

            failures += Check(listenerMemory.HasMemories &&
                              listenerMemory.BuildPromptMemory().Contains("village square"),
                "A nearby NPC remembers a partial overheard conversation.");
            failures += Check(listenerMemory.GetComponentInChildren<NPCOverheardSubtitle>(true) != null,
                "A nearby NPC receives an overheard subtitle object.");

            listenerMemory.ClearMemories();
            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = new Vector3(0f, 100f, 1.5f);
            wall.transform.localScale = new Vector3(2f, 3f, 0.25f);
            Physics.SyncTransforms();

            NPCConversationAwareness.ShareConversation(speaker,
                "This should not pass through a wall.", 7f, Physics.DefaultRaycastLayers);
            failures += Check(!listenerMemory.HasMemories,
                "An NPC behind a wall does not receive the conversation memory.");
        }
        finally
        {
            if (wall != null)
                UnityEngine.Object.DestroyImmediate(wall);

            speakerTransform.position = speakerPosition;
            listenerTransform.position = listenerPosition;
            listenerMemory.ClearMemories();
        }

        return failures;
    }

    private static void InvokeLifecycle(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(
            methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method?.Invoke(target, null);
    }

    private static int RunVisionTests(NPCMemory observerMemory, NPCMemory targetMemory)
    {
        int failures = 0;
        NPCVision observerVision = observerMemory.GetComponent<NPCVision>();
        MethodInfo scanMethod = typeof(NPCVision).GetMethod(
            "ScanForOtherNPCs", BindingFlags.Instance | BindingFlags.NonPublic);

        failures += Check(observerVision != null, "Observer has NPCVision.");
        failures += Check(scanMethod != null, "NPCVision scan function is available.");
        if (observerVision == null || scanMethod == null)
            return failures;

        MethodInfo visionAwake = typeof(NPCVision).GetMethod(
            "Awake", BindingFlags.Instance | BindingFlags.NonPublic);
        visionAwake?.Invoke(observerVision, null);

        Transform observer = observerMemory.transform;
        Transform target = targetMemory.transform;
        Vector3 observerPosition = observer.position;
        Vector3 targetPosition = target.position;
        Quaternion observerRotation = observer.rotation;
        Quaternion targetRotation = target.rotation;
        GameObject wall = null;

        try
        {
            observerMemory.ClearMemories();
            observerMemory.Remember("Direct memory test.");
            failures += Check(observerMemory.HasMemories &&
                              observerMemory.BuildPromptMemory().Contains("Direct memory test."),
                "NPCMemory stores and returns a memory added directly.");

            NPCChatController directMemoryChat = observerMemory.GetComponent<NPCChatController>();
            MethodInfo chatAwake = typeof(NPCChatController).GetMethod(
                "Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            chatAwake?.Invoke(directMemoryChat, null);
            MethodInfo directPromptMethod = typeof(NPCChatController).GetMethod(
                "BuildDeveloperPrompt", BindingFlags.Instance | BindingFlags.NonPublic);
            string directMemoryPrompt = directMemoryChat != null && directPromptMethod != null
                ? (string)directPromptMethod.Invoke(directMemoryChat, null)
                : string.Empty;
            failures += Check(directMemoryPrompt.Contains("Direct memory test."),
                "NPCChatController includes directly added memory in the AI prompt.");

            observer.position = new Vector3(0f, 100f, 0f);
            observer.rotation = Quaternion.LookRotation(Vector3.forward);
            target.position = new Vector3(0f, 100f, 3f);
            target.rotation = Quaternion.LookRotation(Vector3.back);
            Physics.SyncTransforms();

            observerMemory.ClearMemories();
            targetMemory.ClearMemories();
            scanMethod.Invoke(observerVision, null);

            string visibleMemory = observerMemory.BuildPromptMemory();
            failures += Check(observerMemory.HasMemories,
                "Observer remembers a visible NPC three metres ahead.");
            failures += Check(!targetMemory.HasMemories,
                "A sighting is stored only in the observer's memory.");

            observerMemory.ClearMemories();
            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "NPCVisionTestWall";
            wall.transform.position = new Vector3(0f, 100.8f, 1.5f);
            wall.transform.localScale = new Vector3(2f, 3f, 0.25f);
            Physics.SyncTransforms();

            scanMethod.Invoke(observerVision, null);
            failures += Check(!observerMemory.HasMemories,
                "Observer does not remember an NPC hidden behind a wall.");
        }
        finally
        {
            if (wall != null)
                UnityEngine.Object.DestroyImmediate(wall);

            observer.position = observerPosition;
            observer.rotation = observerRotation;
            target.position = targetPosition;
            target.rotation = targetRotation;
            observerMemory.ClearMemories();
            targetMemory.ClearMemories();
        }

        return failures;
    }

    private static int Check(bool condition, string message)
    {
        if (condition)
        {
            Debug.Log($"[NPC TEST] PASS: {message}");
            return 0;
        }

        Debug.LogError($"[NPC TEST] FAIL: {message}");
        return 1;
    }
}
