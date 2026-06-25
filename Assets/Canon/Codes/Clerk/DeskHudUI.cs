using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeskHUDUI : MonoBehaviour
{
    [Header("HUD")]
    public TMP_Text customerCountText;
    public TMP_Text scoreText;

    [Header("Feedback Flash")]
    public Image feedbackPanel;    // full-screen transparent panel
    public Color correctColor = new Color(0f, 1f, 0f, 0.25f);
    public Color wrongColor = new Color(1f, 0f, 0f, 0.25f);
    public float flashDuration = 0.6f;

    public void UpdateHUD(int current, int total, int correct, int wrong)
    {
        customerCountText.text = $"Customer {current} / {total}";
        scoreText.text = $"/ {correct} X {wrong}";
    }

    public void ShowFeedback(bool correct)
    {
        StopAllCoroutines();
        StartCoroutine(Flash(correct ? correctColor : wrongColor));
    }

    IEnumerator Flash(Color color)
    {
        feedbackPanel.color = color;
        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(color.a, 0f, t / flashDuration);
            feedbackPanel.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        feedbackPanel.color = Color.clear;
    }
}