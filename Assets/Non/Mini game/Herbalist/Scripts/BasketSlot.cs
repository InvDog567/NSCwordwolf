// Assets/Scripts/BasketSlot.cs
using UnityEngine;

public class BasketSlot : MonoBehaviour
{
    public string basketType;   // ประเภทตะกร้านี้ เช่น "Mint" (ตั้งใน Inspector)
    private HerbalistManager manager;

    void Start()
    {
        manager = FindFirstObjectByType<HerbalistManager>();
    }

    // เรียกจาก Button OnClick() ของตะกร้านี้
    public void OnBasketClicked()
    {
        if (manager == null) return;
        manager.TryDropIntoBasket(basketType);
    }
}