// Assets/Scripts/CarpenterManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarpenterManager : MonoBehaviour
{
    [Header("=== References ===")]
    public VoxelGrid voxelGrid;
    public BlueprintData blueprintData;
    public Material woodNormalMaterial;
    public Material blueprintHintMaterial;

    [Header("=== UI ===")]
    public Button submitButton;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI blueprintNameText;   // แสดงชื่อแบบพิมพ์ที่สุ่มได้

    private bool isGameOver = false;

    void Start()
    {
        Debug.Log("=== CarpenterManager Start ===");
        CheckRef(voxelGrid, "voxelGrid");
        CheckRef(blueprintData, "blueprintData");
        CheckRef(submitButton, "submitButton");
        CheckRef(resultText, "resultText");

        resultText.text = "";

        // แสดงชื่อแบบพิมพ์ที่สุ่มได้
        if (blueprintNameText != null)
            blueprintNameText.text = $"Blueprint: {blueprintData.GetPatternName()}";

        instructionText.text = "Drag the mouse to carve the wood\nFollow the blueprint shape";

        // แสดง Blueprint Hint ตลอดเวลาทันที ไม่ต้องกดปุ่ม
        ShowBlueprintAlways();
    }

    void CheckRef(Object obj, string name)
    {
        if (obj == null)
            Debug.LogError("[Missing] " + name + " ยังไม่ได้ผูกใน Inspector!");
        else
            Debug.Log("[OK] " + name);
    }

    // แสดง Blueprint ทันทีตั้งแต่เริ่ม ไม่ต้องกด HINT
    void ShowBlueprintAlways()
    {
        voxelGrid.ShowBlueprintHint(blueprintData, blueprintHintMaterial, woodNormalMaterial);
        Debug.Log("Blueprint shown automatically");
    }

    // อัปเดต Blueprint ทุกครั้งที่มีการแกะไม้ (เรียกจาก CarveController)
    public void RefreshBlueprint()
    {
        if (!isGameOver)
            voxelGrid.ShowBlueprintHint(blueprintData, blueprintHintMaterial, woodNormalMaterial);
    }

    public void SubmitCarving()
    {
        if (isGameOver)
        {
            Debug.LogWarning("เกมจบไปแล้ว");
            return;
        }

        Debug.Log("SubmitCarving() called");

        float accuracy = voxelGrid.CalculateAccuracy(blueprintData);
        ShowResult(accuracy);
    }

    void ShowResult(float accuracy)
    {
        isGameOver = true;

        string quality;
        Color color;

        if (accuracy >= 90f)
        {
            quality = "Masterwork!";
            color = new Color(1f, 0.84f, 0f);
        }
        else if (accuracy >= 70f)
        {
            quality = "Good Quality";
            color = Color.green;
        }
        else if (accuracy >= 50f)
        {
            quality = "Rough Shape";
            color = Color.yellow;
        }
        else
        {
            quality = "Failed";
            color = Color.red;
        }

        resultText.text = $"Accuracy: {accuracy:F1}%\n{quality}";
        resultText.color = color;

        Debug.Log($"=== Result === Accuracy: {accuracy:F1}% | Quality: {quality}");
    }

    public void ResetCarving()
    {
        Debug.Log("Reset scene requested");
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}