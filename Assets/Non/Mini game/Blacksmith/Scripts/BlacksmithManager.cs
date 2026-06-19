// Assets/Scripts/BlacksmithManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BlacksmithManager : MonoBehaviour
{
    [Header("=== UI References ===")]
    public RectTransform barArea;        // พื้นที่ทั้งหมดของ Timing Bar (ใช้หาความกว้าง)
    public RectTransform indicator;      // ตัวชี้ที่วิ่งซ้าย-ขวา
    public RectTransform sweetSpot;      // โซนสีเขียว (Sweet Spot)
    public TextMeshProUGUI hitText;      // ขึ้นคำว่า HIT! / MISS!
    public TextMeshProUGUI comboText;    // COMBO: x
    public TextMeshProUGUI roundText;    // ROUND: 1/5
    public TextMeshProUGUI timerText;    // เวลาที่เหลือต่อรอบ
    public TextMeshProUGUI resultText;   // ผลลัพธ์ตอนจบเกม
    public Image sweetSpotImage;         // สำหรับเปลี่ยนสีตอน Feedback (ไม่บังคับ)

    [Header("=== Settings ===")]
    public int totalRounds = 5;          // จำนวนรอบที่ต้องตีสำเร็จ
    public float timePerRound = 3f;      // เวลาต่อรอบ (วินาที)
    public float indicatorSpeed = 300f;  // ความเร็วตัวชี้ (pixel/sec)
    public float startSweetSpotWidth = 160f;  // ความกว้าง Sweet Spot รอบแรก
    public float sweetSpotShrinkPerRound = 20f; // หดลงต่อรอบ (pixel)
    public float minSweetSpotWidth = 40f;       // ความกว้างต่ำสุด (ไม่ให้หดจนหายไป)

    // State
    private float barHalfWidth;          // ครึ่งหนึ่งของความกว้าง Bar (ใช้คำนวณ PingPong)
    private float indicatorPosX;         // ตำแหน่ง X ปัจจุบันของตัวชี้ (-halfWidth ถึง +halfWidth)
    private int direction = 1;           // ทิศทางวิ่ง 1 = ขวา, -1 = ซ้าย
    private bool isRoundActive = false;
    private int currentRound = 0;
    private int comboCount = 0;
    private float currentTime;
    private float currentSweetSpotWidth;

    void Start()
    {
        Debug.Log("=== BlacksmithManager Start ===");
        CheckRef(barArea, "barArea");
        CheckRef(indicator, "indicator");
        CheckRef(sweetSpot, "sweetSpot");
        CheckRef(hitText, "hitText");
        CheckRef(comboText, "comboText");
        CheckRef(roundText, "roundText");
        CheckRef(timerText, "timerText");
        CheckRef(resultText, "resultText");

        StartGame();
    }

    void CheckRef(Object obj, string name)
    {
        if (obj == null)
            Debug.LogError("❌ " + name + " ยังไม่ได้ผูกใน Inspector!");
        else
            Debug.Log("✅ " + name + " OK");
    }

    void StartGame()
    {
        currentRound = 0;
        comboCount = 0;
        currentSweetSpotWidth = startSweetSpotWidth;

        // คำนวณความกว้างพื้นที่ Bar (ครึ่งหนึ่ง สำหรับ PingPong ซ้าย-ขวา)
        barHalfWidth = (barArea.rect.width / 2f) - 20f; // เผื่อขอบไว้ 20px

        resultText.text = "";
        UpdateComboUI();
        StartRound();
    }

    void StartRound()
    {
        if (currentRound >= totalRounds)
        {
            EndGame();
            return;
        }

        currentRound++;
        roundText.text = $"ROUND: {currentRound}/{totalRounds}";

        // เซ็ตความกว้าง Sweet Spot ของรอบนี้ (หดลงทุกรอบ แต่ไม่ต่ำกว่า min)
        currentSweetSpotWidth = Mathf.Max(
            minSweetSpotWidth,
            startSweetSpotWidth - (sweetSpotShrinkPerRound * (currentRound - 1))
        );
        sweetSpot.sizeDelta = new Vector2(currentSweetSpotWidth, sweetSpot.sizeDelta.y);

        // สุ่มตำแหน่ง Sweet Spot ใหม่ในแต่ละรอบ (ในช่วงที่ไม่ติดขอบ)
        float maxOffset = barHalfWidth - (currentSweetSpotWidth / 2f);
        float sweetSpotX = Random.Range(-maxOffset, maxOffset);
        sweetSpot.anchoredPosition = new Vector2(sweetSpotX, sweetSpot.anchoredPosition.y);

        // Reset ตัวชี้ให้เริ่มจากซ้ายสุด วิ่งไปทางขวา
        indicatorPosX = -barHalfWidth;
        direction = 1;
        indicator.anchoredPosition = new Vector2(indicatorPosX, indicator.anchoredPosition.y);

        currentTime = timePerRound;
        isRoundActive = true;
        hitText.text = "";

        Debug.Log($"✅ Round {currentRound} | SweetSpotWidth = {currentSweetSpotWidth} | SweetSpotX = {sweetSpotX}");
    }

    void Update()
    {
        if (!isRoundActive) return;

        // เคลื่อนตัวชี้ซ้าย-ขวาต่อเนื่อง (PingPong)
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

        // นับเวลาต่อรอบ
        currentTime -= Time.deltaTime;
        timerText.text = $"Time: {Mathf.CeilToInt(currentTime)}s";
        timerText.color = currentTime <= 1f ? Color.red : Color.white;

        if (currentTime <= 0f)
        {
            Debug.Log("⏰ หมดเวลา → นับเป็นตีผิด");
            RegisterMiss();
        }

        // กด Spacebar เพื่อตี
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryHit();
        }
    }

    void TryHit()
    {
        if (!isRoundActive) return;

        // เช็คว่าตัวชี้อยู่ในขอบเขต Sweet Spot ไหม
        float sweetSpotMin = sweetSpot.anchoredPosition.x - (sweetSpot.sizeDelta.x / 2f);
        float sweetSpotMax = sweetSpot.anchoredPosition.x + (sweetSpot.sizeDelta.x / 2f);

        bool isHit = indicatorPosX >= sweetSpotMin && indicatorPosX <= sweetSpotMax;

        Debug.Log($"กดตี! indicatorX={indicatorPosX} | sweetSpot=[{sweetSpotMin}, {sweetSpotMax}] | Hit={isHit}");

        if (isHit)
        {
            RegisterHit();
        }
        else
        {
            RegisterMiss();
        }
    }

    void RegisterHit()
    {
        isRoundActive = false;
        comboCount++;

        hitText.text = "HIT! 🔥";
        hitText.color = Color.green;

        UpdateComboUI();

        Debug.Log($"✅ HIT! Combo = {comboCount}");

        StartCoroutine(NextRoundDelay());
    }

    void RegisterMiss()
    {
        isRoundActive = false;
        comboCount = 0;   // Combo รีเซ็ตเป็น 0

        hitText.text = "MISS! ❌";
        hitText.color = Color.red;

        UpdateComboUI();

        Debug.Log("❌ MISS! Combo reset to 0");

        StartCoroutine(NextRoundDelay());
    }

    void UpdateComboUI()
    {
        comboText.text = $"COMBO: {comboCount}x";
    }

    IEnumerator NextRoundDelay()
    {
        yield return new WaitForSeconds(1f);
        StartRound();
    }

    void EndGame()
    {
        isRoundActive = false;
        hitText.text = "";
        timerText.text = "";
        resultText.text = "🔨 Weapon Forged! 🎉";
        resultText.color = Color.yellow;

        Debug.Log("=== Game Over === Weapon Forged!");
    }
}