// Assets/Scripts/ColorButton.cs
using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour
{
    public Color buttonColor;
    private GameManager66 gameManager;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager66>();

        if (gameManager == null)
            Debug.LogError("❌ ColorButton: หา GameManager66 ไม่เจอ!");
        else
            Debug.Log("✅ ColorButton: " + gameObject.name + " หา GameManager66 เจอแล้ว");

        GetComponent<Image>().color = buttonColor;
    }

    public void OnClick()
    {
        Debug.Log("=== Button Clicked: " + gameObject.name + " color: " + buttonColor);

        if (gameManager == null)
        {
            Debug.LogError("❌ gameManager = null กดปุ่มไม่ได้!");
            return;
        }

        gameManager.SelectColor(buttonColor);
    }
}