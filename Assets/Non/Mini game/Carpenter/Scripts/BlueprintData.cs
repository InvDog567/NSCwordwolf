// Assets/Scripts/BlueprintData.cs
using UnityEngine;

public class BlueprintData : MonoBehaviour
{
    [Header("=== Blueprint Pattern ===")]
    public int gridSize = 30;

    private bool[,] pattern;

    // สุ่มแบบพิมพ์ตอนเริ่ม
    private int selectedPattern;

    void Awake()
    {
        pattern = new bool[gridSize, gridSize];

        // สุ่มเลือก 1 ใน 3 แบบ
        selectedPattern = Random.Range(0, 3);

        switch (selectedPattern)
        {
            case 0: GenerateSwordPattern(); break;
            case 1: GenerateAxePattern();   break;
            case 2: GenerateSpearPattern(); break;
        }

        Debug.Log("Blueprint selected: " + GetPatternName());
    }

    public string GetPatternName()
    {
        switch (selectedPattern)
        {
            case 0: return "Sword";
            case 1: return "Axe";
            case 2: return "Spear";
            default: return "Unknown";
        }
    }

    // ===== แบบที่ 1: ดาบ (Sword) =====
    void GenerateSwordPattern()
    {
        int center = gridSize / 2;

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                int distFromCenter = Mathf.Abs(col - center);

                // ใบดาบ — แหลมขึ้นด้านบน
                if (row >= gridSize * 0.35f)
                {
                    float bladeProgress = (row - gridSize * 0.35f) / (gridSize * 0.65f);
                    int bladeWidth = Mathf.RoundToInt(Mathf.Lerp(3f, 0.5f, bladeProgress));
                    pattern[row, col] = distFromCenter <= bladeWidth;
                }
                // การ์ด (Guard) — เส้นกว้าง
                else if (row >= gridSize * 0.28f && row < gridSize * 0.35f)
                {
                    pattern[row, col] = distFromCenter <= gridSize * 0.18f;
                }
                // ด้าม (Handle) — แถวล่างแคบ
                else
                {
                    pattern[row, col] = distFromCenter <= 1;
                }
            }
        }

        Debug.Log("Generated: Sword pattern");
    }

    // ===== แบบที่ 2: ขวาน (Axe) =====
    void GenerateAxePattern()
    {
        int center = gridSize / 2;

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                int distFromCenter = Mathf.Abs(col - center);

                // หัวขวาน — ส่วนบน กว้างและโค้ง
                if (row >= gridSize * 0.45f)
                {
                    // หัวขวานด้านขวา (ใบมีด)
                    bool isBladeRight = col >= center && col <= center + (int)(gridSize * 0.4f);
                    // ส่วนหนา (Body)
                    bool isBody = distFromCenter <= 2;

                    float rowProgress = (float)(row - gridSize * 0.45f) / (gridSize * 0.55f);
                    int bladeHeight = Mathf.RoundToInt(gridSize * 0.3f * (1f - rowProgress));
                    bool isTopBlade = col >= center - bladeHeight && col <= center + bladeHeight + (int)(gridSize * 0.15f);

                    pattern[row, col] = isBody || isTopBlade;
                }
                // ด้ามขวาน — กลาง แคบ ยาว
                else
                {
                    pattern[row, col] = distFromCenter <= 1;
                }
            }
        }

        Debug.Log("Generated: Axe pattern");
    }

    // ===== แบบที่ 3: หอก (Spear) =====
    void GenerateSpearPattern()
    {
        int center = gridSize / 2;

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                int distFromCenter = Mathf.Abs(col - center);

                // ปลายหอก — แหลมมากและยาว
                if (row >= gridSize * 0.55f)
                {
                    float tipProgress = (row - gridSize * 0.55f) / (gridSize * 0.45f);
                    int tipWidth = Mathf.RoundToInt(Mathf.Lerp(4f, 0f, tipProgress));
                    pattern[row, col] = distFromCenter <= tipWidth;
                }
                // คอหอก (Socket) — เชื่อมปลายกับด้าม กว้างกว่าด้ามนิดหน่อย
                else if (row >= gridSize * 0.45f && row < gridSize * 0.55f)
                {
                    pattern[row, col] = distFromCenter <= 3;
                }
                // ด้ามหอก — ยาวและแคบมาก
                else
                {
                    pattern[row, col] = distFromCenter <= 1;
                }
            }
        }

        Debug.Log("Generated: Spear pattern");
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