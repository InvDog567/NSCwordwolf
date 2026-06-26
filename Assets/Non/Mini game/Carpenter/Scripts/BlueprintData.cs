// Assets/Scripts/BlueprintData.cs
using UnityEngine;

public class BlueprintData : MonoBehaviour
{
    [Header("=== Blueprint Pattern ===")]
    public int gridSize = 30;

    private bool[,] pattern;

    void Awake()
    {
        pattern = new bool[gridSize, gridSize];
        GenerateSwordPattern();
    }

    void GenerateSwordPattern()
    {
        int center = gridSize / 2;

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                pattern[row, col] = IsPartOfSword(row, col, center);
            }
        }

        Debug.Log("Sword blueprint pattern generated");
    }

    bool IsPartOfSword(int row, int col, int center)
    {
        int distFromCenter = Mathf.Abs(col - center);

        if (row >= gridSize * 0.35f)
        {
            float bladeProgress = (row - gridSize * 0.35f) / (gridSize * 0.65f);
            int bladeWidth = Mathf.RoundToInt(Mathf.Lerp(3f, 0.5f, bladeProgress));
            return distFromCenter <= bladeWidth;
        }

        if (row >= gridSize * 0.30f && row < gridSize * 0.35f)
        {
            return distFromCenter <= gridSize * 0.18f;
        }

        if (row < gridSize * 0.30f)
        {
            return distFromCenter <= 1;
        }

        return false;
    }

    public bool ShouldRemain(int row, int col)
    {
        if (row < 0 || row >= gridSize || col < 0 || col >= gridSize) return false;
        return pattern[row, col];
    }

    public bool ShouldBeRemoved(int row, int col)
    {
        return !ShouldRemain(row, col);
    }
}