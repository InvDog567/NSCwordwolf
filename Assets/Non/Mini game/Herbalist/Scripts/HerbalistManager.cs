// Assets/Scripts/HerbalistManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HerbalistManager : MonoBehaviour
{
    [Header("=== UI References ===")]
    public Transform herbPilePanel;          // Panel กลางที่มีดอกไม้กองอยู่ (Parent ของ HerbItem ทั้งหมด)
    public GameObject herbItemPrefab;        // Prefab ของดอกไม้ 1 ชิ้น (ทำใน Step ถัดไป)
    public TextMeshProUGUI timerText;        // เวลาที่เหลือรวม
    public TextMeshProUGUI scoreText;        // คะแนนสะสม
    public TextMeshProUGUI feedbackText;     // ขึ้น Correct! / Wrong!
    public TextMeshProUGUI progressText;     // SORTED: 0/12
    public TextMeshProUGUI resultText;       // ผลลัพธ์ตอนจบเกม

    [Header("=== Settings ===")]
    public float totalTimeLimit = 60f;       // เวลารวมทั้งเกม
    public int totalHerbsToSort = 12;        // จำนวนสมุนไพรทั้งหมดที่ต้องคัด
    public int scorePerCorrect = 50;

    // Database สมุนไพรที่เป็นไปได้ (ต้องตรงกับชื่อ BasketSlot ที่สร้างใน Scene)
    private List<HerbData> herbDatabase = new List<HerbData>
    {
        new HerbData { herbName = "Mint",     basketType = "Mint",     herbColor = new Color(0.4f, 0.85f, 0.5f) },
        new HerbData { herbName = "Lavender", basketType = "Lavender", herbColor = new Color(0.6f, 0.4f, 0.85f) },
        new HerbData { herbName = "Marigold", basketType = "Marigold", herbColor = new Color(0.95f, 0.7f, 0.1f) },
    };

    // State
    private List<HerbItem> activeHerbItems = new List<HerbItem>();
    private HerbItem selectedHerb = null;
    private int sortedCount = 0;
    private int score = 0;
    private float currentTime;
    private bool isGameOver = false;

    void Start()
    {
        Debug.Log("=== HerbalistManager Start ===");
        CheckRef(herbPilePanel, "herbPilePanel");
        CheckRef(herbItemPrefab, "herbItemPrefab");
        CheckRef(timerText, "timerText");
        CheckRef(scoreText, "scoreText");
        CheckRef(feedbackText, "feedbackText");
        CheckRef(progressText, "progressText");
        CheckRef(resultText, "resultText");

        StartGame();
    }

    void CheckRef(Object obj, string name)
    {
        if (obj == null)
            Debug.LogError("[Missing] " + name + " ยังไม่ได้ผูกใน Inspector!");
        else
            Debug.Log("[OK] " + name);
    }

    void StartGame()
    {
        sortedCount = 0;
        score = 0;
        isGameOver = false;
        currentTime = totalTimeLimit;

        resultText.text = "";
        feedbackText.text = "";
        UpdateScoreUI();
        UpdateProgressUI();

        SpawnAllHerbs();
    }

    void SpawnAllHerbs()
    {
        for (int i = 0; i < totalHerbsToSort; i++)
        {
            HerbData randomHerb = herbDatabase[Random.Range(0, herbDatabase.Count)];
            SpawnHerb(randomHerb);
        }

        Debug.Log($"Spawned {totalHerbsToSort} herbs");
    }

    void SpawnHerb(HerbData data)
    {
        GameObject newHerb = Instantiate(herbItemPrefab, herbPilePanel);
        newHerb.name = "Herb_" + data.herbName + "_" + activeHerbItems.Count;

        Image img = newHerb.GetComponent<Image>();
        if (img != null) img.color = data.herbColor;

        HerbItem herbItem = newHerb.GetComponent<HerbItem>();
        herbItem.herbName = data.herbName;

        // ใส่ Label ชื่อถ้ามี Text ลูก (ไม่บังคับ)
        TextMeshProUGUI label = newHerb.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = data.herbName;

        activeHerbItems.Add(herbItem);
    }

    void Update()
    {
        if (isGameOver) return;

        currentTime -= Time.deltaTime;
        timerText.text = $"Time: {Mathf.CeilToInt(currentTime)}s";
        timerText.color = currentTime <= 10f ? Color.red : Color.white;

        if (currentTime <= 0f)
        {
            EndGame(false);
        }
    }

    // เรียกจาก HerbItem ตอนคลิกเลือกดอกไม้
    public void SelectHerb(HerbItem herb)
    {
        if (isGameOver) return;

        // ถ้าคลิกชิ้นเดิมที่เลือกอยู่แล้ว = ยกเลิกการเลือก
        if (selectedHerb == herb)
        {
            herb.SetSelected(false);
            selectedHerb = null;
            Debug.Log("ยกเลิกการเลือก");
            return;
        }

        // ยกเลิกตัวที่เลือกไว้ก่อนหน้า (ถ้ามี)
        if (selectedHerb != null)
        {
            selectedHerb.SetSelected(false);
        }

        selectedHerb = herb;
        selectedHerb.SetSelected(true);

        Debug.Log($"เลือกสมุนไพร: {herb.herbName}");
    }

    // เรียกจาก BasketSlot ตอนคลิกตะกร้า
    public void TryDropIntoBasket(string basketType)
    {
        if (isGameOver) return;

        if (selectedHerb == null)
        {
            feedbackText.text = "Select an herb first!";
            feedbackText.color = Color.yellow;
            Debug.LogWarning("ยังไม่ได้เลือกสมุนไพร");
            return;
        }

        Debug.Log($"ใส่ {selectedHerb.herbName} ลงตะกร้า {basketType}");

        if (selectedHerb.herbName == basketType)
        {
            RegisterCorrect();
        }
        else
        {
            RegisterWrong();
        }
    }

    void RegisterCorrect()
    {
        feedbackText.text = "Correct!";
        feedbackText.color = Color.green;

        score += scorePerCorrect;
        sortedCount++;

        activeHerbItems.Remove(selectedHerb);
        Destroy(selectedHerb.gameObject);
        selectedHerb = null;

        UpdateScoreUI();
        UpdateProgressUI();

        Debug.Log($"Correct! Sorted: {sortedCount}/{totalHerbsToSort}");

        if (sortedCount >= totalHerbsToSort)
        {
            EndGame(true);
        }
    }

    void RegisterWrong()
    {
        feedbackText.text = "Wrong Basket!";
        feedbackText.color = Color.red;

        // ยกเลิกการเลือก แต่ดอกไม้ยังอยู่ในกอง ให้ลองใหม่
        selectedHerb.SetSelected(false);
        selectedHerb = null;

        Debug.Log("Wrong basket! Herb stays in pile");
    }

    void UpdateScoreUI()
    {
        scoreText.text = $"SCORE: {score}";
    }

    void UpdateProgressUI()
    {
        progressText.text = $"SORTED: {sortedCount}/{totalHerbsToSort}";
    }

    void EndGame(bool success)
    {
        isGameOver = true;
        feedbackText.text = "";
        timerText.text = "";

        if (success)
        {
            resultText.text = $"All Herbs Sorted!\nFinal Score: {score}";
            resultText.color = Color.yellow;
        }
        else
        {
            resultText.text = $"Time's Up!\nSorted: {sortedCount}/{totalHerbsToSort}\nScore: {score}";
            resultText.color = new Color(1f, 0.6f, 0f); // สีส้ม (แก้จาก Color.orange ที่ไม่มีจริง)
        }

        Debug.Log($"=== Game Over === Success: {success} | Score: {score}");
    }
}