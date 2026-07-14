// Assets/Scripts/SparkEffect.cs
using UnityEngine;
using System.Collections;

public class SparkEffect : MonoBehaviour
{
    [Header("=== Spark Settings ===")]
    public int sparkCount = 8;
    public float sparkSpeed = 3f;
    public float sparkLifetime = 0.4f;
    public Color sparkColor = new Color(1f, 0.8f, 0.2f);

    // เรียกจาก BlacksmithManager ตอน HIT
    public void PlaySparks()
    {
        StartCoroutine(SpawnSparks());
    }

    IEnumerator SpawnSparks()
    {
        for (int i = 0; i < sparkCount; i++)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.name = "Spark";
            spark.transform.position = transform.position;
            spark.transform.localScale = Vector3.one * 0.03f;

            // ลบ Collider ออก (ไม่ต้องการ Physics)
            Destroy(spark.GetComponent<Collider>());

            // ใส่ Material สีเหลืองสว่าง
            Renderer rend = spark.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = sparkColor;
            rend.material = mat;

            // ยิง Spark ออกไปในทิศสุ่ม
            Vector3 randomDir = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(-0.5f, 0.5f)
            ).normalized;

            StartCoroutine(MoveSpark(spark, randomDir));
        }

        yield return null;
    }

    IEnumerator MoveSpark(GameObject spark, Vector3 direction)
    {
        float elapsed = 0f;

        while (elapsed < sparkLifetime)
        {
            if (spark == null) yield break;

            // เคลื่อนที่ + ตกลงด้วย Gravity
            spark.transform.position += direction * sparkSpeed * Time.deltaTime;
            direction += Vector3.down * 5f * Time.deltaTime;

            // ค่อยๆ หายไป
            float alpha = 1f - (elapsed / sparkLifetime);
            Renderer rend = spark.GetComponent<Renderer>();
            if (rend != null)
            {
                Color c = rend.material.color;
                c.a = alpha;
                rend.material.color = c;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (spark != null) Destroy(spark);
    }
}