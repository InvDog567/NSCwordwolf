// Assets/Scripts/ForgeFlicker.cs
using UnityEngine;

public class ForgeFlicker : MonoBehaviour
{
    [Header("=== Settings ===")]
    public float minIntensity = 2f;
    public float maxIntensity = 5f;
    public float flickerSpeed = 8f;

    private Light forgeLight;
    private float baseIntensity;

    void Start()
    {
        forgeLight = GetComponent<Light>();
        baseIntensity = forgeLight.intensity;
    }

    void Update()
    {
        // ทำให้แสงกระพริบแบบ Perlin Noise (ดูเป็นธรรมชาติ)
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        forgeLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}