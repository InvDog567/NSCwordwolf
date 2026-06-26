using UnityEngine;
using TMPro;

public class ResultsScreenUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text titleText;
    public TMP_Text summaryText;

    void Start() => panel.SetActive(false);

    public void Show(int correct, int wrong, int total)
    {
        panel.SetActive(true);

        float accuracy = (float)correct / total * 100f;

        titleText.text = accuracy >= 70f ? "Shift Complete!" : "Poor Performance...";

        summaryText.text =
            $"Customers Processed:  {total}\n" +
            $"Correct Decisions:    {correct}\n" +
            $"Mistakes:             {wrong}\n\n" +
            $"Accuracy:             {accuracy:F0}%\n\n" +
            GetRemark(accuracy);
    }

    string GetRemark(float accuracy)
    {
        if (accuracy == 100f) return "\"Perfect record. The Guild is pleased.\"";
        if (accuracy >= 80f) return "\"Good work, inspector. The roads are safer.\"";
        if (accuracy >= 60f) return "\"Acceptable, but stay sharp.\"";
        return "\"Contraband may have slipped through. Disgraceful.\"";
    }

    // Hook this to a "Continue" button in the UI
    public void OnContinuePressed()
    {
        panel.SetActive(false);
        // FPS controller is already re-enabled by DeskInteractable.ExitDeskMode()
    }
}