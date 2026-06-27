// Assets/Scripts/HerbData.cs
using UnityEngine;

[System.Serializable]
public class HerbData
{
    public string herbName;       // ชื่อสมุนไพร เช่น "Mint"
    public string basketType;     // ตะกร้าที่ควรลงไป (ปกติเหมือน herbName แต่แยกไว้เผื่อขยาย)
    public Color herbColor;       // สีของดอกไม้/สมุนไพร (ใช้แสดงผลแทน Sprite)
}
