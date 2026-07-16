// Assets/Scripts/VoxelGrid.cs
using UnityEngine;
using System.Collections.Generic;

public class VoxelGrid : MonoBehaviour
{
    [Header("=== Grid Settings ===")]
    public int gridSize = 30;           // 30x30 = 900 ช่อง
    public float woodWidth = 3f;        // ความกว้างจริงของก้อนไม้ (Unity Unit)
    public float woodHeight = 3f;       // ความสูงจริงของก้อนไม้
    public float voxelDepth = 0.3f;     // ความลึกของแต่ละ Voxel

    [Header("=== Materials ===")]
    public Material woodMaterial;       // เนื้อไม้ปกติ
    public Material blueprintMaterial;  // ส่วนที่ "ควรหาย" ตาม Blueprint (โชว์ตอนเปิด Hint)

    // เก็บ Voxel ทั้งหมดในรูปแบบ 2D Array [row, col]
    private GameObject[,] voxels;
    private bool[,] isAlive;            // true = ยังอยู่, false = ถูกแกะออกแล้ว

    private float cellWidth;
    private float cellHeight;

    void Awake()
    {
        cellWidth = woodWidth / gridSize;
        cellHeight = woodHeight / gridSize;

        voxels = new GameObject[gridSize, gridSize];
        isAlive = new bool[gridSize, gridSize];

        GenerateGrid();
    }

    void GenerateGrid()
    {
        Debug.Log($"🪵 Generating Voxel Grid {gridSize}x{gridSize} = {gridSize * gridSize} voxels");

        // จุดเริ่มต้น (มุมล่างซ้ายของก้อนไม้ เทียบกับจุดกึ่งกลาง)
        float startX = -woodWidth / 2f + (cellWidth / 2f);
        float startY = -woodHeight / 2f + (cellHeight / 2f);

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                GameObject voxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                voxel.name = $"Voxel_{row}_{col}";
                voxel.transform.parent = this.transform;

                // ตำแหน่ง Local (X = col, Y = row, Z = ลึกเข้าไปในไม้)
                float posX = startX + (col * cellWidth);
                float posY = startY + (row * cellHeight);
                voxel.transform.localPosition = new Vector3(posX, posY, voxelDepth / 2f);
                voxel.transform.localScale = new Vector3(cellWidth * 0.98f, cellHeight * 0.98f, voxelDepth);

                // ใส่ Material เนื้อไม้
                if (woodMaterial != null)
                    voxel.GetComponent<Renderer>().material = woodMaterial;

                // ตั้ง Layer สำหรับ Raycast
                voxel.layer = LayerMask.NameToLayer("WoodVoxel");

                voxels[row, col] = voxel;
                isAlive[row, col] = true;
            }
        }

        Debug.Log("✅ Grid generated successfully");
    }

    // ลบ Voxel ที่ตำแหน่ง row, col (เรียกจากตอนลากเมาส์)
    public void CarveVoxel(int row, int col)
    {
        if (row < 0 || row >= gridSize || col < 0 || col >= gridSize) return;
        if (!isAlive[row, col]) return; // ลบไปแล้ว ไม่ต้องทำซ้ำ

        isAlive[row, col] = false;
        voxels[row, col].SetActive(false);
    }

    // เช็คว่า Voxel นี้ยังอยู่ไหม
    public bool IsAlive(int row, int col)
    {
        if (row < 0 || row >= gridSize || col < 0 || col >= gridSize) return false;
        return isAlive[row, col];
    }

    // คืนค่าตำแหน่ง Row, Col จากชื่อ GameObject ที่โดน Raycast
    public bool TryGetGridPosition(GameObject hitObject, out int row, out int col)
    {
        row = -1;
        col = -1;

        string[] parts = hitObject.name.Split('_');
        if (parts.Length == 3 && parts[0] == "Voxel")
        {
            if (int.TryParse(parts[1], out row) && int.TryParse(parts[2], out col))
            {
                return true;
            }
        }
        return false;
    }

    public int GetGridSize() => gridSize;

    // Show blueprint hint by changing voxel materials based on the blueprint
    public void ShowBlueprintHint(BlueprintData blueprint, Material hintMaterial, Material normalMaterial)
    {
        if (blueprint == null) return;

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                GameObject voxel = voxels[row, col];
                if (voxel == null) continue;

                Renderer r = voxel.GetComponent<Renderer>();
                if (r == null) continue;

                if (blueprint.ShouldBeRemoved(row, col))
                {
                    if (hintMaterial != null)
                        r.material = hintMaterial;
                }
                else
                {
                    if (normalMaterial != null)
                        r.material = normalMaterial;
                }
            }
        }
    }

    // Calculate carving accuracy against the blueprint (0-100)
    public float CalculateAccuracy(BlueprintData blueprint)
    {
        if (blueprint == null) return 0f;

        int total = gridSize * gridSize;
        int correct = 0;

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                bool shouldRemain = blueprint.ShouldRemain(row, col);
                bool alive = IsAlive(row, col);

                if (shouldRemain && alive) correct++;
                else if (!shouldRemain && !alive) correct++;
            }
        }

        if (total == 0) return 0f;
        return (correct / (float)total) * 100f;
    }
}
