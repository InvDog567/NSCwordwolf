// Assets/Scripts/BartenderInteraction.cs
using UnityEngine;

public class BartenderInteraction : MonoBehaviour
{
    [Header("=== Settings ===")]
    public Color bottleColor;          // สีของขวดนี้
    public string bottleName;          // ชื่อสี เช่น "Red"
    public float hoverScaleMultiplier = 1.2f;  // ขยายตอน Hover

    private GameManager66 gameManager;
    private Vector3 originalScale;
    private Renderer bottleRenderer;
    private bool isHovering = false;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager66>();
        originalScale = transform.localScale;
        bottleRenderer = GetComponent<Renderer>();

        if (gameManager == null)
            Debug.LogError("ไม่พบ GameManager66!");
    }

    // เมาส์ชี้ที่ขวด
    void OnMouseEnter()
    {
        isHovering = true;
        transform.localScale = originalScale * hoverScaleMultiplier;
        Debug.Log($"Hovering: {bottleName}");
    }

    // เมาส์ออกจากขวด
    void OnMouseExit()
    {
        isHovering = false;
        transform.localScale = originalScale;
    }

    // คลิกขวด
    void OnMouseDown()
    {
        Debug.Log($"Clicked bottle: {bottleName}");
        gameManager.SelectColor(bottleColor);

        // Flash effect — ขวดสว่างขึ้นชั่วคราว
        StartCoroutine(FlashEffect());
    }

    System.Collections.IEnumerator FlashEffect()
    {
        // ขยายใหญ่ขึ้นชั่วคราว
        transform.localScale = originalScale * 1.4f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = isHovering ?
            originalScale * hoverScaleMultiplier : originalScale;
    }
}