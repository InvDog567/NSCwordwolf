// Assets/Scripts/WoodChipSpawner.cs
using UnityEngine;
using System.Collections;

public class WoodChipSpawner : MonoBehaviour
{
    [Header("=== Settings ===")]
    public int chipCount = 5;
    public float chipSpeed = 2f;
    public float chipLifetime = 0.6f;
    public Color chipColor = new Color(0.6f, 0.4f, 0.2f);

    public void SpawnChips()
    {
        StartCoroutine(SpawnCoroutine());
    }

    IEnumerator SpawnCoroutine()
    {
        for (int i = 0; i < chipCount; i++)
        {
            GameObject chip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chip.name = "WoodChip";
            chip.transform.position = transform.position + new Vector3(
                Random.Range(-0.1f, 0.1f), 0.05f, Random.Range(-0.1f, 0.1f));
            chip.transform.localScale = new Vector3(
                Random.Range(0.02f, 0.06f),
                Random.Range(0.01f, 0.02f),
                Random.Range(0.02f, 0.05f));
            chip.transform.rotation = Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f));

            Destroy(chip.GetComponent<Collider>());

            Renderer rend = chip.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = chipColor;
            rend.material = mat;

            Vector3 dir = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.3f, 1f),
                Random.Range(-0.5f, 0.5f)).normalized;

            StartCoroutine(MoveChip(chip, dir));
        }
        yield return null;
    }

    IEnumerator MoveChip(GameObject chip, Vector3 direction)
    {
        float elapsed = 0f;

        while (elapsed < chipLifetime)
        {
            if (chip == null) yield break;

            chip.transform.position += direction * chipSpeed * Time.deltaTime;
            direction += Vector3.down * 8f * Time.deltaTime;
            chip.transform.Rotate(direction * 200f * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (chip != null) Destroy(chip);
    }
}