// Assets/Scripts/BlueprintData.cs
using UnityEngine;

public class BlueprintData : MonoBehaviour
{
    [Header("=== Blueprint Pattern ===")]
    public int gridSize = 30;

    // true = ช่องนี้ "ควรเหลืออยู่" (เป็นส่วนของดาบ)
    // false = ช่องนี้ "ควรถูกแกะออก" (ไม่ใช่ส่วนของดาบ)
    private bool[,] pattern;

    void Awake()
    {
        pattern = new bool[gridSize, gridSize];
        GenerateSwordPattern();
    }

    // สร้างรูปดาบง่ายๆ ด้วยสมการ (ใบดาบ + ด้าม + การ์ด)
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

        Debug.Log("✅ Sword blueprint pattern generated");
    }

    bool IsPartOfSword(int row, int col, int center)
    {
        int distFromCenter = Mathf.Abs(col - center);

        // ใบดาบ (Blade) — แถวบน 60% ของ Grid, แหลมขึ้นด้านบน
        if (row >= gridSize * 0.35f)
        {
            float bladeProgress = (row - gridSize * 0.35f) / (gridSize * 0.65f); // 0 ถึง 1
            int bladeWidth = Mathf.RoundToInt(Mathf.Lerp(3f, 0.5f, bladeProgress)); // กว้างฐาน แหลมปลาย
            return distFromCenter <= bladeWidth;
        }

        // การ์ดดาบ (Guard) — เส้นกว้างแถวกลาง
        if (row >= gridSize * 0.30f && row < gridSize * 0.35f)
        {
            return distFromCenter <= gridSize * 0.18f;
        }

        // ด้ามดาบ (Handle) — แถวล่าง แคบ
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

    // Human-friendly name for the pattern
    public string GetPatternName()
    {
        return "Sword";
    }
}