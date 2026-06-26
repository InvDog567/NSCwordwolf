// Assets/Scripts/CarveController.cs
using UnityEngine;

public class CarveController : MonoBehaviour
{
    [Header("=== References ===")]
    public VoxelGrid voxelGrid;
    public Camera carveCamera;

    [Header("=== Settings ===")]
    public LayerMask voxelLayerMask;
    public float maxCarveDistance = 20f;

    private bool isDragging = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            TryCarveAtMousePosition();
        }
    }

    void TryCarveAtMousePosition()
    {
        Ray ray = carveCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxCarveDistance, voxelLayerMask))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (voxelGrid.TryGetGridPosition(hitObject, out int row, out int col))
            {
                voxelGrid.CarveVoxel(row, col);
                Debug.Log($"Carved voxel at [{row}, {col}]");
            }
        }
    }
}