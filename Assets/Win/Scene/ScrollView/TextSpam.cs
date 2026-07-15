using TMPro;
using UnityEngine;
using System.Collections;

public class TextSpam : MonoBehaviour
{
    [Header("References")]
    public TMP_Text dialogueText;

    [Header("Settings")]
    public float interval = 0.25f;
    public int maxLines = 1000;

    private int lineNumber = 1;

    void Start()
    {
        dialogueText.text = "";
        StartCoroutine(SpamText());
    }

    IEnumerator SpamText()
    {
        while (true)
        {
            dialogueText.text += $"Line {lineNumber}: This is a test message.\n";
            lineNumber++;

            // Prevent the text from becoming absurdly large.
            if (lineNumber > maxLines)
            {
                dialogueText.text = "";
                lineNumber = 1;
            }

            yield return new WaitForSeconds(interval);
        }
    }
}