// Assets/Scripts/CarpenterManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarpenterManager : MonoBehaviour
{
    [Header("=== Core References ===")]
    public VoxelGrid voxelGrid;
    public BlueprintData blueprintData;
    public Material woodNormalMaterial;
    public Material blueprintHintMaterial;

    [Header("=== 3D References ===")]
    public ChiselAnimation chiselAnimation;    // สิ่ว First Person
    public WoodChipSpawner chipSpawner;        // เศษไม้กระเด็น

    [Header("=== UI ===")]
    public Button submitButton;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI blueprintNameText;

    private bool isGameOver = false;

    void Start()
    {
        Debug.Log("=== CarpenterManager Start ===");
        CheckRef(voxelGrid, "voxelGrid");
        CheckRef(blueprintData, "blueprintData");
        CheckRef(submitButton, "submitButton");
        CheckRef(resultText, "resultText");

        resultText.text = "";

        if (blueprintNameText != null)
            blueprintNameText.text = $"Blueprint: {blueprintData.GetPatternName()}";

        if (instructionText != null)
            instructionText.text = "Drag mouse to carve\nFollow the blueprint shape";

        ShowBlueprintAlways();
    }

    void CheckRef(Object obj, string name)
    {
        if (obj == null)
            Debug.LogError("[Missing] " + name);
        else
            Debug.Log("[OK] " + name);
    }

    void ShowBlueprintAlways()
    {
        voxelGrid.ShowBlueprintHint(blueprintData, blueprintHintMaterial, woodNormalMaterial);
    }

    // เรียกจาก CarveController ทุกครั้งที่แกะ
    public void OnVoxelCarved()
    {
        if (isGameOver) return;

        // เล่น Animation สิ่ว
        if (chiselAnimation != null)
            chiselAnimation.PlayStrike();

        // เศษไม้กระเด็น
        if (chipSpawner != null)
            chipSpawner.SpawnChips();

        // อัปเดต Blueprint ให้แสดงอยู่เสมอ
        voxelGrid.ShowBlueprintHint(blueprintData, blueprintHintMaterial, woodNormalMaterial);
    }

    public void SubmitCarving()
    {
        if (isGameOver) return;

        float accuracy = voxelGrid.CalculateAccuracy(blueprintData);
        ShowResult(accuracy);
    }

    void ShowResult(float accuracy)
    {
        isGameOver = true;

        string quality;
        Color color;

        if (accuracy >= 90f)       { quality = "Masterwork!";   color = new Color(1f, 0.84f, 0f); }
        else if (accuracy >= 70f)  { quality = "Good Quality";  color = Color.green; }
        else if (accuracy >= 50f)  { quality = "Rough Shape";   color = Color.yellow; }
        else                       { quality = "Failed";         color = Color.red; }

        resultText.text = $"Accuracy: {accuracy:F1}%\n{quality}";
        resultText.color = color;

        Debug.Log($"Result: {accuracy:F1}% | {quality}");
    }

    public void ResetCarving()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}