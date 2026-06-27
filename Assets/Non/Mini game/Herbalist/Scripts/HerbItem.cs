// Assets/Scripts/HerbItem.cs
using UnityEngine;
using UnityEngine.UI;

public class HerbItem : MonoBehaviour
{
    public string herbName;          // ชื่อสมุนไพรของชิ้นนี้ (ตั้งตอน Spawn)
    private HerbalistManager manager;
    private Image itemImage;
    private bool isSelected = false;

    void Awake()
    {
        manager = FindFirstObjectByType<HerbalistManager>();
        itemImage = GetComponent<Image>();
    }

    // เรียกจาก Button OnClick() ของชิ้นนี้
    public void OnHerbClicked()
    {
        if (manager == null) return;
        manager.SelectHerb(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        // แสดงผลว่าถูกเลือกอยู่ (ขยายขนาดเล็กน้อย + ใส่ขอบสว่าง)
        transform.localScale = selected ? Vector3.one * 1.15f : Vector3.one;
    }

    public bool IsSelected() => isSelected;
}
