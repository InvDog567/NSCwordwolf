// Assets/Scripts/TestColor.cs
using UnityEngine;
using UnityEngine.UI;

public class TestColor : MonoBehaviour
{
    private Image img;
    private float timer = 0f;

    void Start()
    {
        img = GetComponent<Image>();
        Debug.Log("Image found: " + img);
    }

    void Update()
    {
        // เปลี่ยนสีทุก 1 วินาทีอัตโนมัติ
        timer += Time.deltaTime;

        if (timer > 1f)
        {
            timer = 0f;
            img.color = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                1f
            );
            Debug.Log("Color changed to: " + img.color);
        }
    }
}
