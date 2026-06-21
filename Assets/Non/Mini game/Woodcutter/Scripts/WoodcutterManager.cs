// Assets/Scripts/WoodcutterManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class WoodcutterManager : MonoBehaviour
{
    [Header("=== UI References ===")]
    public TextMeshProUGUI keyPromptText;    // ตัวอักษรปุ่มที่ต้องกด (Z/X/C/V)
    public TextMeshProUGUI hitText;          // ขึ้น HIT! / MISS!
    public TextMeshProUGUI comboText;        // COMBO: x
    public TextMeshProUGUI scoreText;        // SCORE: 
    public TextMeshProUGUI resultText;       // ผลลัพธ์ตอนจบเกม
    public Image progressBarFill;            // Progress Bar สีเขียว (Image Type: Filled)
    public Image keyPromptCircle;            // วงกลม Highlight รอบตัวอักษร (ไม่บังคับ)

    [Header("=== Settings ===")]
    public int totalHitsNeeded = 10;         // จำนวนครั้งที่ต้องกดถูกให้ไม้ล้ม
    public float timePerPrompt = 1.5f;       // เวลาจำกัดต่อปุ่ม
    public int scorePerHit = 100;            // คะแนนต่อครั้งที่ HIT

    // Key mapping
    private KeyCode[] possibleKeys = { KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V };
    private string[] possibleKeyNames = { "Z", "X", "C", "V" };

    // State
    private KeyCode currentKey;
    private int currentHits = 0;
    private int comboCount = 0;
    private int score = 0;
    private float currentTime;
    private bool isRoundActive = false;
    private bool isGameOver = false;

    void Start()
    {
        Debug.Log("=== WoodcutterManager Start ===");
        CheckRef(keyPromptText, "keyPromptText");
        CheckRef(hitText, "hitText");
        CheckRef(comboText, "comboText");
        CheckRef(scoreText, "scoreText");
        CheckRef(resultText, "resultText");
        CheckRef(progressBarFill, "progressBarFill");

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
        currentHits = 0;
        comboCount = 0;
        score = 0;
        isGameOver = false;

        UpdateComboUI();
        UpdateScoreUI();
        UpdateProgressBar();
        resultText.text = "";

        NextPrompt();
    }

    void NextPrompt()
    {
        if (isGameOver) return;

        // สุ่มปุ่มใหม่ (กันไม่ให้ซ้ำปุ่มก่อนหน้าติดกัน 2 ครั้ง — เพิ่มความสนุก)
        int randomIndex = Random.Range(0, possibleKeys.Length);
        currentKey = possibleKeys[randomIndex];

        keyPromptText.text = possibleKeyNames[randomIndex];
        hitText.text = "";

        currentTime = timePerPrompt;
        isRoundActive = true;

        Debug.Log($"🪓 New Prompt: {possibleKeyNames[randomIndex]} | Time: {timePerPrompt}s");
    }

    void Update()
    {
        if (!isRoundActive || isGameOver) return;

        // นับเวลาถอยหลัง
        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            Debug.Log("⏰ หมดเวลา → MISS");
            RegisterMiss();
            return;
        }

        // เช็คว่ามีการกดปุ่มไหนใน Z/X/C/V บ้าง
        foreach (KeyCode key in possibleKeys)
        {
            if (Input.GetKeyDown(key))
            {
                CheckKeyPress(key);
                break;
            }
        }
    }

    void CheckKeyPress(KeyCode pressedKey)
    {
        if (!isRoundActive) return;

        Debug.Log($"กดปุ่ม: {pressedKey} | ต้องกด: {currentKey}");

        if (pressedKey == currentKey)
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
        currentHits++;
        score += scorePerHit + (comboCount * 5); // Bonus ตาม Combo

        hitText.text = "HIT! 🪓";
        hitText.color = Color.green;

        UpdateComboUI();
        UpdateScoreUI();
        UpdateProgressBar();

        Debug.Log($"✅ HIT! Combo = {comboCount} | Hits = {currentHits}/{totalHitsNeeded}");

        if (currentHits >= totalHitsNeeded)
        {
            EndGame();
        }
        else
        {
            StartCoroutine(NextPromptDelay());
        }
    }

    void RegisterMiss()
    {
        isRoundActive = false;
        comboCount = 0;   // Combo รีเซ็ต

        hitText.text = "MISS! ❌";
        hitText.color = Color.red;

        UpdateComboUI();

        Debug.Log("❌ MISS! Combo reset to 0");

        StartCoroutine(NextPromptDelay());
    }

    void UpdateComboUI()
    {
        comboText.text = $"COMBO: x{comboCount}";
    }

    void UpdateScoreUI()
    {
        scoreText.text = $"SCORE: {score:N0}";
    }

    void UpdateProgressBar()
    {
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = (float)currentHits / totalHitsNeeded;
        }
    }

    IEnumerator NextPromptDelay()
    {
        yield return new WaitForSeconds(0.5f);
        NextPrompt();
    }

    void EndGame()
    {
        isGameOver = true;
        isRoundActive = false;
        keyPromptText.text = "";
        hitText.text = "";

        resultText.text = $"🌳 Tree Down! 🎉\nFinal Score: {score:N0}";
        resultText.color = Color.yellow;

        Debug.Log($"=== Game Over === Tree Down! Final Score: {score}");
    }
}
