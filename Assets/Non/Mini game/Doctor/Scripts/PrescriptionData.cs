// Assets/Scripts/PrescriptionData.cs
using System.Collections.Generic;

[System.Serializable]
public class PrescriptionData
{
    public string patientName;         // ชื่อผู้ป่วย เช่น "Jack"
    public List<string> requiredMedicines; // รายชื่อยาที่ต้องส่ง ตามลำดับ เช่น ["Red Tonic", "Blue Serum", "Green Elixir"]
}
