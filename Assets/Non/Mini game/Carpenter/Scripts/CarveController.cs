// Assets/Scripts/CarveController.cs
using UnityEngine;

public class CarveController : MonoBehaviour
{
    [Header("=== References ===")]
    public VoxelGrid voxelGrid;
    public CarpenterManager carpenterManager;
    public Camera carveCamera;

    [Header("=== Settings ===")]
    public LayerMask voxelLayerMask;
    public float maxCarveDistance = 20f;

    private bool isDragging = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) isDragging = true;
        if (Input.GetMouseButtonUp(0))   isDragging = false;
        if (isDragging) TryCarve();
    }

    void TryCarve()
    {
        Ray ray = carveCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxCarveDistance, voxelLayerMask))
        {
            if (voxelGrid.TryGetGridPosition(hit.collider.gameObject, out int row, out int col))
            {
                voxelGrid.CarveVoxel(row, col);

                // เรียก OnVoxelCarved → เล่น Animation + เศษไม้
                if (carpenterManager != null)
                    carpenterManager.OnVoxelCarved();
            }
        }
    }
}