// Assets/Scripts/ChiselAnimation.cs
using UnityEngine;
using System.Collections;

public class ChiselAnimation : MonoBehaviour
{
    [Header("=== Settings ===")]
    public float strikeDistance = 0.08f;  // ระยะกระแทกลง
    public float strikeSpeed = 15f;       // ความเร็วกระแทก

    private Vector3 restPosition;
    private bool isStriking = false;

    void Start()
    {
        restPosition = transform.localPosition;
    }

    public void PlayStrike()
    {
        if (!isStriking)
            StartCoroutine(StrikeCoroutine());
    }

    IEnumerator StrikeCoroutine()
    {
        isStriking = true;
        Vector3 strikePos = restPosition + new Vector3(0, -strikeDistance, 0);

        // กระแทกลง
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * strikeSpeed;
            transform.localPosition = Vector3.Lerp(restPosition, strikePos, t);
            yield return null;
        }

        // ดีดกลับ
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * strikeSpeed * 0.7f;
            transform.localPosition = Vector3.Lerp(strikePos, restPosition, t);
            yield return null;
        }

        transform.localPosition = restPosition;
        isStriking = false;
    }
}
