using System;

[Serializable]
public class DocumentData
{
    // --- Ledger (what player expects) ---
    public string merchantName;
    public string cargoType;
    public string originVillage;
    public bool sealRequired;
    public int maxDaysOld;

    // --- Document (what customer hands in) ---
    public string docMerchantName;
    public string docCargoType;
    public string docOriginVillage;
    public bool docHasSeal;
    public int docDaysOld;       // how many days ago it was issued

    // --- Ground truth ---
    public bool isValid;
    public string invalidField;  // "name" | "cargo" | "origin" | "seal" | "date" | ""
}