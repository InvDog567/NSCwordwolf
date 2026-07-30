using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class ChatManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_Text dialogueText;

    private string apiKey = "";

    bool isWaitingForReply = false;


    public void SendMessage()
    {
        if(isWaitingForReply)
            return;

        string playerMessage = inputField.text;

        if(string.IsNullOrWhiteSpace(playerMessage))
            return;

        dialogueText.text += "\nPlayer: " + playerMessage;

        inputField.text = "";

        StartCoroutine(SendToGemini(playerMessage));
    }

    IEnumerator SendToGemini(string playerMessage)
    {
        isWaitingForReply = true;

        string prompt =
            "You are a medieval villager in a werewolf game. " +
            "Reply naturally and briefly.\n\n" +
            "Player: " + playerMessage;

        string jsonData =
            "{"
            + "\"contents\": ["
            + "{"
            + "\"parts\": ["
            + "{"
            + "\"text\": \"" + prompt + "\""
            + "}"
            + "]"
            + "}"
            + "]"
            + "}";

        string url =
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key="
            + apiKey;

        UnityWebRequest request =
            new UnityWebRequest(url, "POST");

        byte[] bodyRaw =
            Encoding.UTF8.GetBytes(jsonData);

        request.uploadHandler =
            new UploadHandlerRaw(bodyRaw);

        request.downloadHandler =
            new DownloadHandlerBuffer();

        request.SetRequestHeader(
            "Content-Type",
            "application/json"
        );

        yield return request.SendWebRequest();

        Debug.Log(request.downloadHandler.text);

        if(request.result == UnityWebRequest.Result.Success)
        {
            dialogueText.text +=
                "\nNPC: Success!";
        }
        else
        {
            dialogueText.text +=
                "\nERROR: " + request.responseCode;

            Debug.LogError(request.error);
            Debug.LogError(request.downloadHandler.text);
        }

        isWaitingForReply = false;
    }
}
