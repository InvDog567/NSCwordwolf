// Assets/Scripts/HerbItem.cs
using UnityEngine;
using UnityEngine.UI;

public class HerbItem : MonoBehaviour
{
    public string herbName;
    private HerbalistManager manager;
    private bool isSelected = false;

    void Awake()
    {
        manager = FindFirstObjectByType<HerbalistManager>();
    }

    public void OnHerbClicked()
    {
        // ระบบใหม่ไม่ต้องคลิกเลือกดอกไม้แล้ว ปล่อยว่างไว้
        Debug.Log("HerbItem clicked (not used in new system)");
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        transform.localScale = selected ? Vector3.one * 1.15f : Vector3.one;
    }

    public bool IsSelected() => isSelected;
}
