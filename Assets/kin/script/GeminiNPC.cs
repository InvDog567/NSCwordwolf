using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GeminiNPC : MonoBehaviour
{
    // ใส่ API Key ของคุณตรงนี้
    [SerializeField] private string apiKey = "YOUR_API_KEY_HERE";
    private string modelName = "models/gemini-2.0-flash";

    // ฟังก์ชันสำหรับเรียกใช้งานจากภายนอก
    void Start()
    {
        Talk("Hello, are you there?");
    }
    public void Talk(string message)
    {
        StartCoroutine(SendMessageToGemini(message));
    }

    IEnumerator SendMessageToGemini(string playerText)
    {
        // 1. ใช้ v1beta เพราะเป็นรุ่นใหม่
        string url = $"https://generativelanguage.googleapis.com/v1beta/{modelName}:generateContent?key={apiKey}";

        // 2. โครงสร้าง JSON 
        string jsonData = "{\"contents\":[{\"parts\":[{\"text\":\"" + playerText + "\"}]}]}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("<color=green>สำเร็จ!</color> NPC ตอบว่า: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error: " + request.responseCode);
                Debug.LogError("Detail: " + request.downloadHandler.text);
            }
        }
    }
}