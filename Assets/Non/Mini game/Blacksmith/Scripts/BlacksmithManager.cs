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
    public TextMeshProUGUI weaponNameText;    // ชื่ออาวุธปัจจุบัน
    public TextMeshProUGUI weaponCountText;   // WEAPON: 1/4
    public TextMeshProUGUI roundText;         // ROUND: 1/5
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI earningsText;      // เงินที่ได้อาวุธนี้
    public TextMeshProUGUI totalEarningsText; // เงินรวมทั้งหมด
    public TextMeshProUGUI resultText;        // ผลลัพธ์ตอนจบเกม
    public GameObject summaryPanel;           // Panel แสดงสรุปตอนจบ
    public TextMeshProUGUI summaryText;       // รายละเอียดสรุปแต่ละอาวุธ

    [Header("=== Settings ===")]
    public float timePerRound = 3f;
    public int roundsPerWeapon = 5;
    public float startSweetSpotWidth = 160f;
    public float sweetSpotShrinkPerRound = 15f;
    public float minSweetSpotWidth = 40f;
    public float indicatorSpeed = 300f;

    // Weapon Database
    private struct WeaponData
    {
        public string weaponName;
        public int basePrice;
    }

    private List<WeaponData> weaponDatabase = new List<WeaponData>
    {
        new WeaponData { weaponName = "Sword",   basePrice = 100 },
        new WeaponData { weaponName = "Axe",     basePrice = 80  },
        new WeaponData { weaponName = "Spear",   basePrice = 90  },
        new WeaponData { weaponName = "Shield",  basePrice = 70  },
        new WeaponData { weaponName = "Dagger",  basePrice = 60  },
        new WeaponData { weaponName = "Hammer",  basePrice = 85  },
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
    private int hitCount = 0;        // จำนวน HIT ในอาวุธนี้
    private int comboCount = 0;
    private float currentSweetSpotWidth;

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
        CheckRef(barArea, "barArea");
        CheckRef(indicator, "indicator");
        CheckRef(sweetSpot, "sweetSpot");
        CheckRef(hitText, "hitText");
        CheckRef(weaponNameText, "weaponNameText");
        CheckRef(weaponCountText, "weaponCountText");
        CheckRef(roundText, "roundText");
        CheckRef(timerText, "timerText");
        CheckRef(resultText, "resultText");

        if (summaryPanel != null) summaryPanel.SetActive(false);

        GenerateWeaponSession();
        StartSession();
    }

    void CheckRef(Object obj, string name)
    {
        if (obj == null)
            Debug.LogError("[Missing] " + name);
        else
            Debug.Log("[OK] " + name);
    }

    // สุ่มจำนวนและชนิดอาวุธสำหรับ Session นี้
    void GenerateWeaponSession()
    {
        totalWeapons = Random.Range(3, 7); // สุ่ม 3-6 ชิ้น
        weaponsThisSession.Clear();

        // สุ่มชนิดอาวุธ (อนุญาตให้ซ้ำได้)
        for (int i = 0; i < totalWeapons; i++)
        {
            int randomIndex = Random.Range(0, weaponDatabase.Count);
            weaponsThisSession.Add(weaponDatabase[randomIndex]);
        }

        Debug.Log($"Session: {totalWeapons} weapons");
        foreach (var w in weaponsThisSession)
            Debug.Log($"  - {w.weaponName} ({w.basePrice}G)");
    }

    void StartSession()
    {
        currentWeaponIndex = 0;
        totalEarnings = 0;
        summaryLines.Clear();
        isGameOver = false;

        barHalfWidth = (barArea.rect.width / 2f) - 20f;

        UpdateTotalEarningsUI();
        resultText.text = "";

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
        currentSweetSpotWidth = startSweetSpotWidth;

        weaponNameText.text = $"Forging: {currentWeapon.weaponName}";
        weaponCountText.text = $"WEAPON: {currentWeaponIndex + 1}/{totalWeapons}";

        if (earningsText != null)
            earningsText.text = $"Value: {currentWeapon.basePrice}G";

        UpdateComboUI();
        hitText.text = "";
        resultText.text = "";

        Debug.Log($"Starting weapon {currentWeaponIndex + 1}/{totalWeapons}: {currentWeapon.weaponName}");

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
        roundText.text = $"ROUND: {currentRound}/{roundsPerWeapon}";

        // Sweet Spot เล็กลงทุกรอบ
        currentSweetSpotWidth = Mathf.Max(
            minSweetSpotWidth,
            startSweetSpotWidth - (sweetSpotShrinkPerRound * (currentRound - 1))
        );
        sweetSpot.sizeDelta = new Vector2(currentSweetSpotWidth, sweetSpot.sizeDelta.y);

        // สุ่มตำแหน่ง Sweet Spot
        float maxOffset = barHalfWidth - (currentSweetSpotWidth / 2f);
        float sweetSpotX = Random.Range(-maxOffset, maxOffset);
        sweetSpot.anchoredPosition = new Vector2(sweetSpotX, sweetSpot.anchoredPosition.y);

        // Reset ตัวชี้
        indicatorPosX = -barHalfWidth;
        direction = 1;
        indicator.anchoredPosition = new Vector2(indicatorPosX, indicator.anchoredPosition.y);

        currentTime = timePerRound;
        isRoundActive = true;
        hitText.text = "";

        Debug.Log($"Round {currentRound}/{roundsPerWeapon} | SweetSpot width: {currentSweetSpotWidth}");
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

        // นับเวลา
        currentTime -= Time.deltaTime;
        timerText.text = $"Time: {Mathf.CeilToInt(currentTime)}s";
        timerText.color = currentTime <= 1f ? Color.red : Color.white;

        if (currentTime <= 0f)
        {
            RegisterMiss();
            return;
        }

        // กด Space ตี
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryHit();
        }
    }

    void TryHit()
    {
        if (!isRoundActive) return;

        float sweetSpotMin = sweetSpot.anchoredPosition.x - (sweetSpot.sizeDelta.x / 2f);
        float sweetSpotMax = sweetSpot.anchoredPosition.x + (sweetSpot.sizeDelta.x / 2f);
        bool isHit = indicatorPosX >= sweetSpotMin && indicatorPosX <= sweetSpotMax;

        if (isHit)
            RegisterHit();
        else
            RegisterMiss();
    }

    void RegisterHit()
    {
        isRoundActive = false;
        hitCount++;
        comboCount++;

        hitText.text = "HIT!";
        hitText.color = Color.green;

        UpdateComboUI();
        Debug.Log($"HIT! Round {currentRound} | hitCount: {hitCount}");

        StartCoroutine(NextRoundDelay());
    }

    void RegisterMiss()
    {
        isRoundActive = false;
        comboCount = 0;

        hitText.text = "MISS!";
        hitText.color = Color.red;

        UpdateComboUI();
        Debug.Log($"MISS! Round {currentRound} | hitCount: {hitCount}");

        StartCoroutine(NextRoundDelay());
    }

    IEnumerator NextRoundDelay()
    {
        yield return new WaitForSeconds(0.8f);
        StartNextRound();
    }

    // จบอาวุธชิ้นนี้ → คำนวณเงินที่ได้
    void FinishWeapon()
    {
        isRoundActive = false;

        float hitPercent = (float)hitCount / roundsPerWeapon;
        int earned = Mathf.RoundToInt(currentWeapon.basePrice * hitPercent);
        totalEarnings += earned;

        // สร้างข้อความสรุปสำหรับอาวุธนี้
        string grade = GetGrade(hitPercent);
        string summaryLine = $"{currentWeapon.weaponName}: {hitCount}/{roundsPerWeapon} HIT ({grade}) → {earned}G";
        summaryLines.Add(summaryLine);

        // แสดงผลชั่วคราวก่อนไปอาวุธต่อไป
        resultText.text = $"{currentWeapon.weaponName} done!\n{hitCount}/{roundsPerWeapon} HIT = {earned}G ({grade})";
        resultText.color = hitPercent >= 0.8f ? Color.green :
                           hitPercent >= 0.5f ? Color.yellow : Color.red;

        UpdateTotalEarningsUI();

        Debug.Log($"Weapon finished: {currentWeapon.weaponName} | {hitCount}/{roundsPerWeapon} HIT = {earned}G ({hitPercent*100:F0}%)");

        currentWeaponIndex++;
        StartCoroutine(NextWeaponDelay());
    }

    string GetGrade(float hitPercent)
    {
        if (hitPercent >= 1.0f)  return "S - Perfect!";
        if (hitPercent >= 0.8f)  return "A - Great";
        if (hitPercent >= 0.6f)  return "B - Good";
        if (hitPercent >= 0.4f)  return "C - OK";
        if (hitPercent >= 0.2f)  return "D - Poor";
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

    // จบทุกอาวุธ → แสดงสรุป
    void EndSession()
    {
        isGameOver = true;
        isRoundActive = false;

        hitText.text = "";
        timerText.text = "";
        roundText.text = "";
        weaponNameText.text = "Session Complete!";

        // สร้างข้อความสรุปทั้งหมด
        string summary = "=== SUMMARY ===\n\n";
        foreach (string line in summaryLines)
            summary += line + "\n";
        summary += $"\nTotal Earned: {totalEarnings}G";

        resultText.text = $"All done! Total: {totalEarnings}G";
        resultText.color = Color.yellow;

        // แสดง Summary Panel ถ้ามี
        if (summaryPanel != null && summaryText != null)
        {
            summaryPanel.SetActive(true);
            summaryText.text = summary;
        }

        Debug.Log("=== Session Complete ===");
        Debug.Log(summary);
    }
}