// Assets/Scripts/BlacksmithManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class BlacksmithManager : MonoBehaviour
{
    [Header("=== UI References ===")]
    public RectTransform barArea;
    public RectTransform indicator;
    public RectTransform sweetSpot;
    public TextMeshProUGUI hitText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI weaponCountText;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI earningsText;
    public TextMeshProUGUI totalEarningsText;
    public TextMeshProUGUI resultText;
    public GameObject summaryPanel;
    public TextMeshProUGUI summaryText;

    [Header("=== 3D References ===")]
    public HammerAnimation hammerAnimation;   // ค้อน 3D
    public SparkEffect sparkEffect;           // ประกายไฟ
    public Renderer weaponRenderer;           // WeaponOnAnvil Renderer
    public Light forgeGlowLight;             // แสงเตา (ปรับความสว่างตอน HIT)

    [Header("=== Settings ===")]
    public float timePerRound = 3f;
    public int roundsPerWeapon = 5;
    public float startSweetSpotWidth = 160f;
    public float sweetSpotShrinkPerRound = 15f;
    public float minSweetSpotWidth = 40f;
    public float indicatorSpeed = 300f;

    // URP
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    // Weapon Database
    [System.Serializable]
    private struct WeaponData
    {
        public string weaponName;
        public int basePrice;
        public Color metalColor;   // สีของเหล็กที่กำลังตี
    }

    private List<WeaponData> weaponDatabase = new List<WeaponData>
    {
        new WeaponData { weaponName = "Sword",   basePrice = 100, metalColor = new Color(0.8f, 0.4f, 0.1f) },
        new WeaponData { weaponName = "Axe",     basePrice = 80,  metalColor = new Color(0.9f, 0.3f, 0.05f) },
        new WeaponData { weaponName = "Spear",   basePrice = 90,  metalColor = new Color(0.85f, 0.35f, 0.08f) },
        new WeaponData { weaponName = "Shield",  basePrice = 70,  metalColor = new Color(0.7f, 0.5f, 0.15f) },
        new WeaponData { weaponName = "Dagger",  basePrice = 60,  metalColor = new Color(0.9f, 0.45f, 0.1f) },
        new WeaponData { weaponName = "Hammer",  basePrice = 85,  metalColor = new Color(0.75f, 0.38f, 0.08f) },
    };

    // Session State
    private List<WeaponData> weaponsThisSession = new List<WeaponData>();
    private int totalWeapons = 0;
    private int currentWeaponIndex = 0;
    private int totalEarnings = 0;
    private List<string> summaryLines = new List<string>();

    // Per-weapon State
    private WeaponData currentWeapon;
    private int currentRound = 0;
    private int hitCount = 0;
    private int comboCount = 0;

    // Per-round State
    private float barHalfWidth;
    private float indicatorPosX;
    private int direction = 1;
    private bool isRoundActive = false;
    private float currentTime;
    private bool isGameOver = false;

    void Start()
    {
        Debug.Log("=== BlacksmithManager Start ===");

        if (summaryPanel != null) summaryPanel.SetActive(false);

        GenerateWeaponSession();
        StartSession();
    }

    void GenerateWeaponSession()
    {
        totalWeapons = Random.Range(3, 7);
        weaponsThisSession.Clear();

        for (int i = 0; i < totalWeapons; i++)
        {
            int randomIndex = Random.Range(0, weaponDatabase.Count);
            weaponsThisSession.Add(weaponDatabase[randomIndex]);
        }

        Debug.Log($"Session: {totalWeapons} weapons");
    }

    void StartSession()
    {
        currentWeaponIndex = 0;
        totalEarnings = 0;
        summaryLines.Clear();
        isGameOver = false;

        barHalfWidth = (barArea.rect.width / 2f) - 20f;

        UpdateTotalEarningsUI();
        if (resultText != null) resultText.text = "";

        StartNextWeapon();
    }

    void StartNextWeapon()
    {
        if (currentWeaponIndex >= totalWeapons)
        {
            EndSession();
            return;
        }

        currentWeapon = weaponsThisSession[currentWeaponIndex];
        currentRound = 0;
        hitCount = 0;
        comboCount = 0;

        // เปลี่ยนสีอาวุธบนทั่ง
        if (weaponRenderer != null)
            weaponRenderer.material.SetColor(BaseColor, currentWeapon.metalColor);

        if (weaponNameText != null)
            weaponNameText.text = $"Forging: {currentWeapon.weaponName}";
        if (weaponCountText != null)
            weaponCountText.text = $"WEAPON: {currentWeaponIndex + 1}/{totalWeapons}";
        if (earningsText != null)
            earningsText.text = $"Value: {currentWeapon.basePrice}G";

        UpdateComboUI();
        if (hitText != null) hitText.text = "";
        if (resultText != null) resultText.text = "";

        Debug.Log($"Starting: {currentWeapon.weaponName}");
        StartNextRound();
    }

    void StartNextRound()
    {
        if (currentRound >= roundsPerWeapon)
        {
            FinishWeapon();
            return;
        }

        currentRound++;
        if (roundText != null)
            roundText.text = $"ROUND: {currentRound}/{roundsPerWeapon}";

        // Sweet Spot เล็กลงทุกรอบ
        float sweetWidth = Mathf.Max(
            minSweetSpotWidth,
            startSweetSpotWidth - (sweetSpotShrinkPerRound * (currentRound - 1))
        );
        sweetSpot.sizeDelta = new Vector2(sweetWidth, sweetSpot.sizeDelta.y);

        // สุ่มตำแหน่ง Sweet Spot
        float maxOffset = barHalfWidth - (sweetWidth / 2f);
        float sweetX = Random.Range(-maxOffset, maxOffset);
        sweetSpot.anchoredPosition = new Vector2(sweetX, sweetSpot.anchoredPosition.y);

        // Reset ตัวชี้
        indicatorPosX = -barHalfWidth;
        direction = 1;
        indicator.anchoredPosition = new Vector2(indicatorPosX, indicator.anchoredPosition.y);

        currentTime = timePerRound;
        isRoundActive = true;
        if (hitText != null) hitText.text = "";
    }

    void Update()
    {
        if (!isRoundActive || isGameOver) return;

        // เคลื่อนตัวชี้ PingPong
        indicatorPosX += direction * indicatorSpeed * Time.deltaTime;

        if (indicatorPosX >= barHalfWidth)
        {
            indicatorPosX = barHalfWidth;
            direction = -1;
        }
        else if (indicatorPosX <= -barHalfWidth)
        {
            indicatorPosX = -barHalfWidth;
            direction = 1;
        }

        indicator.anchoredPosition = new Vector2(indicatorPosX, indicator.anchoredPosition.y);

        // Timer
        currentTime -= Time.deltaTime;
        if (timerText != null)
        {
            timerText.text = $"Time: {Mathf.CeilToInt(currentTime)}s";
            timerText.color = currentTime <= 1f ? Color.red : Color.white;
        }

        if (currentTime <= 0f)
        {
            RegisterMiss();
            return;
        }

        // กด Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryHit();
        }
    }

    void TryHit()
    {
        if (!isRoundActive) return;

        float sweetMin = sweetSpot.anchoredPosition.x - (sweetSpot.sizeDelta.x / 2f);
        float sweetMax = sweetSpot.anchoredPosition.x + (sweetSpot.sizeDelta.x / 2f);
        bool isHit = indicatorPosX >= sweetMin && indicatorPosX <= sweetMax;

        if (isHit) RegisterHit();
        else RegisterMiss();
    }

    void RegisterHit()
    {
        isRoundActive = false;
        hitCount++;
        comboCount++;

        if (hitText != null)
        {
            hitText.text = "HIT!";
            hitText.color = Color.green;
        }

        UpdateComboUI();

        // เล่น Animation ค้อน
        if (hammerAnimation != null)
            hammerAnimation.PlaySwing();

        // เล่น Spark Effect
        if (sparkEffect != null)
            sparkEffect.PlaySparks();

        // Flash แสงเตา
        StartCoroutine(ForgeFlash());

        Debug.Log($"HIT! Round {currentRound} | hitCount: {hitCount}");
        StartCoroutine(NextRoundDelay());
    }

    void RegisterMiss()
    {
        isRoundActive = false;
        comboCount = 0;

        if (hitText != null)
        {
            hitText.text = "MISS!";
            hitText.color = Color.red;
        }

        UpdateComboUI();
        Debug.Log($"MISS! Round {currentRound}");
        StartCoroutine(NextRoundDelay());
    }

    // Flash แสงเตาตอน HIT
    IEnumerator ForgeFlash()
    {
        if (forgeGlowLight == null) yield break;

        float originalIntensity = forgeGlowLight.intensity;
        forgeGlowLight.intensity = 10f;  // สว่างวาบ

        yield return new WaitForSeconds(0.1f);

        forgeGlowLight.intensity = originalIntensity;
    }

    IEnumerator NextRoundDelay()
    {
        yield return new WaitForSeconds(0.8f);
        StartNextRound();
    }

    void FinishWeapon()
    {
        isRoundActive = false;

        float hitPercent = (float)hitCount / roundsPerWeapon;
        int earned = Mathf.RoundToInt(currentWeapon.basePrice * hitPercent);
        totalEarnings += earned;

        string grade = GetGrade(hitPercent);
        string summaryLine = $"{currentWeapon.weaponName}: {hitCount}/{roundsPerWeapon} HIT ({grade}) -> {earned}G";
        summaryLines.Add(summaryLine);

        // เปลี่ยนสีอาวุธให้เป็นสีเหล็กเย็น (เสร็จแล้ว)
        if (weaponRenderer != null)
            weaponRenderer.material.SetColor(BaseColor, new Color(0.4f, 0.4f, 0.45f));

        if (resultText != null)
        {
            resultText.text = $"{currentWeapon.weaponName} done!\n{hitCount}/{roundsPerWeapon} HIT = {earned}G ({grade})";
            resultText.color = hitPercent >= 0.8f ? Color.green :
                               hitPercent >= 0.5f ? Color.yellow : Color.red;
        }

        UpdateTotalEarningsUI();
        currentWeaponIndex++;
        StartCoroutine(NextWeaponDelay());
    }

    string GetGrade(float hitPercent)
    {
        if (hitPercent >= 1.0f) return "S - Perfect!";
        if (hitPercent >= 0.8f) return "A - Great";
        if (hitPercent >= 0.6f) return "B - Good";
        if (hitPercent >= 0.4f) return "C - OK";
        if (hitPercent >= 0.2f) return "D - Poor";
        return "F - Failed";
    }

    IEnumerator NextWeaponDelay()
    {
        yield return new WaitForSeconds(2f);
        StartNextWeapon();
    }

    void UpdateComboUI()
    {
        if (comboText != null)
            comboText.text = $"COMBO: {comboCount}x";
    }

    void UpdateTotalEarningsUI()
    {
        if (totalEarningsText != null)
            totalEarningsText.text = $"Total: {totalEarnings}G";
    }

    void EndSession()
    {
        isGameOver = true;
        isRoundActive = false;

        if (hitText != null) hitText.text = "";
        if (timerText != null) timerText.text = "";
        if (roundText != null) roundText.text = "";
        if (weaponNameText != null) weaponNameText.text = "Session Complete!";

        string summary = "=== SUMMARY ===\n\n";
        foreach (string line in summaryLines)
            summary += line + "\n";
        summary += $"\nTotal Earned: {totalEarnings}G";

        if (resultText != null)
        {
            resultText.text = $"All done!\nTotal: {totalEarnings}G";
            resultText.color = Color.yellow;
        }

        if (summaryPanel != null && summaryText != null)
        {
            summaryPanel.SetActive(true);
            summaryText.text = summary;
        }

        Debug.Log("=== Session Complete ===\n" + summary);
    }
}