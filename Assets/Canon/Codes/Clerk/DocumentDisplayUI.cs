using UnityEngine;
using TMPro;

public class DocumentDisplayUI : MonoBehaviour
{
    [Header("Text Fields")]
    public TMP_Text merchantNameText;
    public TMP_Text cargoTypeText;
    public TMP_Text originVillageText;
    public TMP_Text dateText;

    [Header("Seal")]
    public GameObject sealObject;

    public void Populate(DocumentData doc)
    {
        merchantNameText.text = $"Name: {doc.docMerchantName}";
        cargoTypeText.text = $"Cargo: {doc.docCargoType}";
        originVillageText.text = $"Origin: {doc.docOriginVillage}";
        dateText.text = $"Issued: {DocumentGenerator.FormatDate(doc.docDaysOld)}";

        if (sealObject != null)
            sealObject.SetActive(doc.docHasSeal);
    }
}