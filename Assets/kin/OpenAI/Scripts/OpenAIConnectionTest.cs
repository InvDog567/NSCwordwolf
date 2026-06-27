// Assets/kin/OpenAI/Scripts/OpenAIConnectionTest.cs
// กด Play → ดู Console ว่า API ต่อได้ไหม

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class OpenAIConnectionTest : MonoBehaviour
{
    [SerializeField] private bool runOnStart = true;

    [TextArea(1, 3)]
    [SerializeField] private string testMessage = "Say hello in one short sentence.";

    private bool _hasRun;

    private async void Start()
    {
        if (!runOnStart || _hasRun)
            return;

        await Task.Yield();
        RunTest();
    }

    [ContextMenu("Test API Connection")]
    public void RunTest()
    {
        if (_hasRun && Application.isPlaying)
            return;

        _hasRun = true;
        RunTestAsync();
    }

    private async void RunTestAsync()
    {
        Debug.Log("=== [OpenAITest] Starting ===");

        if (OpenAIManager.Instance == null)
        {
            Debug.LogError("[OpenAITest] ไม่เจอ OpenAIManager — Add Component ใน scene ก่อน");
            return;
        }

        if (!OpenAIManager.Instance.HasValidApiKey)
        {
            Debug.LogError("[OpenAITest] ไม่มี API key — สร้าง openai_key.json แล้วลากใส่ OpenAIManager");
            return;
        }

        var messages = new List<ChatMessage>
        {
            new ChatMessage("developer", "You are a helpful assistant. Reply in one short sentence."),
            new ChatMessage("user", testMessage)
        };

        Debug.Log($"[OpenAITest] Sending: \"{testMessage}\"");

        try
        {
            string reply = await OpenAIManager.Instance.SendChatCompletionAsync(messages);
            Debug.Log($"[OpenAITest] SUCCESS!\n→ {reply}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[OpenAITest] FAILED: {ex.Message}");
        }
    }
}
