// Assets/Scripts/HammerAnimation.cs
using UnityEngine;
using System.Collections;

public class HammerAnimation : MonoBehaviour
{
    [Header("=== Settings ===")]
    public float swingAngle = 60f;       // องศาที่แกว่ง
    public float swingSpeed = 8f;        // ความเร็วการแกว่ง

    private Quaternion restRotation;
    private Quaternion swingRotation;
    private bool isSwinging = false;

    void Start()
    {
        restRotation  = transform.localRotation;
        swingRotation = Quaternion.Euler(
            transform.localEulerAngles.x - swingAngle,
            transform.localEulerAngles.y,
            transform.localEulerAngles.z
        );
    }

    // เรียกจาก BlacksmithManager ตอน HIT
    public void PlaySwing()
    {
        if (!isSwinging)
            StartCoroutine(SwingCoroutine());
    }

    IEnumerator SwingCoroutine()
    {
        isSwinging = true;

        // แกว่งลง
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * swingSpeed;
            transform.localRotation = Quaternion.Lerp(restRotation, swingRotation, t);
            yield return null;
        }

        // แกว่งกลับ
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * swingSpeed;
            transform.localRotation = Quaternion.Lerp(swingRotation, restRotation, t);
            yield return null;
        }

        transform.localRotation = restRotation;
        isSwinging = false;
    }
}