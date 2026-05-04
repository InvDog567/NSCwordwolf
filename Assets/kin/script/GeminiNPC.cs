using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class GeminiNPC : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string apiKey = "AIzaSyCha0PhgUIiKU3H7DeWn_ZS-2u4co95lqA";

    // ใช้ชื่อรุ่นแบบเต็มตามที่ API ต้องการ
    // ลองเปลี่ยนเป็นชื่อนี้ครับ (ตัวอักษรเล็กทั้งหมด และเช็คตัวสะกดให้เป๊ะ)
    private string modelName = "models/gemini-1.5-flash";

    void Start()
    {
        // สั่งให้ทำงานทันทีที่กด Play
        StartCoroutine(SendMessageToGemini("Hello, who are you?"));
    }

    IEnumerator SendMessageToGemini(string playerText)
    {
        // 2. ใช้ v1beta ตามที่ลิสต์ระบุว่ารองรับ generateContent
        string url = $"https://generativelanguage.googleapis.com/v1/{modelName}:generateContent?key={apiKey}";

        // 3. อย่าลืมเปลี่ยนส่วนรับข้อมูลกลับเป็นแบบส่งข้อความ (เหมือนโค้ดอันแรกสุดที่เราทำ)
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
                Debug.Log("<color=green>สำเร็จแล้ว!</color> AI ตอบว่า: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error: " + request.responseCode);
                Debug.LogError("Detail: " + request.downloadHandler.text);
            }
        }
    }
}