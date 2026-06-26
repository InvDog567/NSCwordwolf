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
    public Button toggleHintButton;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI instructionText;

    private bool isHintShowing = false;
    private bool isGameOver = false;

    void Start()
    {
        Debug.Log("=== CarpenterManager Start ===");
        CheckRef(voxelGrid, "voxelGrid");
        CheckRef(blueprintData, "blueprintData");
        CheckRef(submitButton, "submitButton");
        CheckRef(resultText, "resultText");

        resultText.text = "";
        instructionText.text = "Drag the mouse to carve the wood\nPress HINT to see the blueprint";
    }

    void CheckRef(Object obj, string name)
    {
        if (obj == null)
            Debug.LogError("[Missing] " + name + " ยังไม่ได้ผูกใน Inspector!");
        else
            Debug.Log("[OK] " + name);
    }

    public void ToggleHint()
    {
        isHintShowing = !isHintShowing;

        if (isHintShowing)
        {
            voxelGrid.ShowBlueprintHint(blueprintData, blueprintHintMaterial, woodNormalMaterial);
            Debug.Log("Hint ON");
        }
        else
        {
            voxelGrid.HideBlueprintHint(woodNormalMaterial);
            Debug.Log("Hint OFF");
        }
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