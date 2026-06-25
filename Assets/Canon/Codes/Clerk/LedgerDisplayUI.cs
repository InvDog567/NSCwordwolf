using UnityEngine;
using TMPro;

public class LedgerDisplayUI : MonoBehaviour
{
    [Header("Text Fields")]
    public TMP_Text todayDateText;
    public TMP_Text merchantNameText;
    public TMP_Text cargoTypeText;
    public TMP_Text originVillageText;
    public TMP_Text sealRuleText;
    public TMP_Text dateRuleText;

    public void Populate(DocumentData doc)
    {
        todayDateText.text = $"Today: {DocumentGenerator.TodayDate}";

        merchantNameText.text = $"Correct Name: {doc.merchantName}";
        cargoTypeText.text = $"Correct Cargo: {doc.cargoType}";
        originVillageText.text = $"Correct Origin: {doc.originVillage}";

        sealRuleText.text = doc.sealRequired
            ? "Seal Rule: Must have Guild Seal"
            : "Seal Rule: No seal needed";

        dateRuleText.text = $"Date Rule: Must be {doc.maxDaysOld} days old or newer";
    }
}