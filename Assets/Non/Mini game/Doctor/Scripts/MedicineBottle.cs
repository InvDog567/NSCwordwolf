// Assets/Scripts/MedicineBottle.cs
using UnityEngine;
using UnityEngine.UI;

public class MedicineBottle : MonoBehaviour
{
    public string medicineName;   // ชื่อยาประจำขวดนี้ (ตั้งค่าใน Inspector)
    private DoctorManager doctorManager;

    void Start()
    {
        doctorManager = FindFirstObjectByType<DoctorManager>();

        if (doctorManager == null)
            Debug.LogError("ไม่พบ DoctorManager ใน Scene!");

        // ถ้าใช้ UI Image ให้ตั้งสีตาม medicineName (ไม่บังคับ ทำใน Editor ก็ได้)
    }

    // เรียกจาก Button OnClick() ของขวดนี้
    public void OnBottleClicked()
    {
        Debug.Log($"คลิกขวด: {medicineName}");

        if (doctorManager != null)
        {
            doctorManager.TrySelectMedicine(medicineName);
        }
    }
}
