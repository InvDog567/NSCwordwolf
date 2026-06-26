// Assets/Scripts/VoxelGrid.cs
using UnityEngine;
using System.Collections.Generic;

public class VoxelGrid : MonoBehaviour
{
    [Header("=== Grid Settings ===")]
    public int gridSize = 30;
    public float woodWidth = 3f;
    public float woodHeight = 3f;
    public float voxelDepth = 0.3f;

    [Header("=== Materials ===")]
    public Material woodMaterial;
    public Material blueprintMaterial;

    private GameObject[,] voxels;
    private bool[,] isAlive;

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
        Debug.Log($"Generating Voxel Grid {gridSize}x{gridSize} = {gridSize * gridSize} voxels");

        float startX = -woodWidth / 2f + (cellWidth / 2f);
        float startY = -woodHeight / 2f + (cellHeight / 2f);

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                GameObject voxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                voxel.name = $"Voxel_{row}_{col}";
                voxel.transform.parent = this.transform;

                float posX = startX + (col * cellWidth);
                float posY = startY + (row * cellHeight);
                voxel.transform.localPosition = new Vector3(posX, posY, voxelDepth / 2f);
                voxel.transform.localScale = new Vector3(cellWidth * 0.98f, cellHeight * 0.98f, voxelDepth);

                if (woodMaterial != null)
                    voxel.GetComponent<Renderer>().material = woodMaterial;

                voxel.layer = LayerMask.NameToLayer("WoodVoxel");

                voxels[row, col] = voxel;
                isAlive[row, col] = true;
            }
        }

        Debug.Log("Grid generated successfully");
    }

    public void CarveVoxel(int row, int col)
    {
        if (row < 0 || row >= gridSize || col < 0 || col >= gridSize) return;
        if (!isAlive[row, col]) return;

        isAlive[row, col] = false;
        voxels[row, col].SetActive(false);
    }

    public bool IsAlive(int row, int col)
    {
        if (row < 0 || row >= gridSize || col < 0 || col >= gridSize) return false;
        return isAlive[row, col];
    }

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

    // แสดง Voxel ที่ "ควรถูกแกะออก" เป็นสีอื่น (Hint)
    public void ShowBlueprintHint(BlueprintData blueprint, Material hintMaterial, Material normalMaterial)
    {
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                if (!isAlive[row, col]) continue;

                bool shouldRemain = blueprint.ShouldRemain(row, col);
                Renderer rend = voxels[row, col].GetComponent<Renderer>();

                rend.material = shouldRemain ? normalMaterial : hintMaterial;
            }
        }
    }

    public void HideBlueprintHint(Material normalMaterial)
    {
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                if (!isAlive[row, col]) continue;
                voxels[row, col].GetComponent<Renderer>().material = normalMaterial;
            }
        }
    }

    public float CalculateAccuracy(BlueprintData blueprint)
    {
        int correctCount = 0;
        int totalCount = gridSize * gridSize;

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                bool shouldRemain = blueprint.ShouldRemain(row, col);
                bool currentlyAlive = isAlive[row, col];

                if (shouldRemain == currentlyAlive)
                {
                    correctCount++;
                }
            }
        }

        float accuracy = (float)correctCount / totalCount * 100f;
        Debug.Log($"Accuracy: {correctCount}/{totalCount} = {accuracy:F1}%");
        return accuracy;
    }
}