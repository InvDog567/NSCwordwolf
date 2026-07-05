// Assets/Scripts/HerbalistManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HerbalistManager : MonoBehaviour
{
    [Header("=== UI References ===")]
    public Image currentHerbDisplay;         // แสดงสมุนไพรชิ้นปัจจุบัน (Image กลางจอ)
    public TextMeshProUGUI currentHerbNameText; // ชื่อสมุนไพรปัจจุบัน
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI progressText;     // SORTED: 0/20
    public TextMeshProUGUI remainingText;    // Remaining: 20
    public TextMeshProUGUI resultText;

    [Header("=== Basket Buttons ===")]
    public Button mintBasketButton;
    public Button lavenderBasketButton;
    public Button marigoldBasketButton;

    [Header("=== Settings ===")]
    public float totalTimeLimit = 90f;       // เพิ่มเวลาให้พอกับจำนวนที่เพิ่มขึ้น
    public int totalHerbsToSort = 20;        // เพิ่มจาก 12 → 20
    public int scorePerCorrect = 50;
    public int scorePenaltyWrong = 20;       // หักคะแนนถ้าใส่ผิดตะกร้า

    // Database
    private List<HerbData> herbDatabase = new List<HerbData>
    {
        new HerbData { herbName = "Mint",     basketType = "Mint",     herbColor = new Color(0.4f, 0.85f, 0.5f) },
        new HerbData { herbName = "Lavender", basketType = "Lavender", herbColor = new Color(0.6f, 0.4f, 0.85f) },
        new HerbData { herbName = "Marigold", basketType = "Marigold", herbColor = new Color(0.95f, 0.7f, 0.1f) },
    };

    // State
    private Queue<HerbData> herbQueue = new Queue<HerbData>(); // คิวสมุนไพรทั้งหมด
    private HerbData currentHerb;
    private int sortedCount = 0;
    private int score = 0;
    private float currentTime;
    private bool isGameOver = false;

    void Start()
    {
        Debug.Log("=== HerbalistManager Start ===");
        CheckRef(currentHerbDisplay, "currentHerbDisplay");
        CheckRef(currentHerbNameText, "currentHerbNameText");
        CheckRef(timerText, "timerText");
        CheckRef(scoreText, "scoreText");
        CheckRef(feedbackText, "feedbackText");
        CheckRef(progressText, "progressText");
        CheckRef(resultText, "resultText");
        CheckRef(mintBasketButton, "mintBasketButton");
        CheckRef(lavenderBasketButton, "lavenderBasketButton");
        CheckRef(marigoldBasketButton, "marigoldBasketButton");

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

        GenerateHerbQueue();
        ShowNextHerb();
    }

    // สร้างคิวสมุนไพรสุ่ม 20 ชิ้น
    void GenerateHerbQueue()
    {
        herbQueue.Clear();

        List<HerbData> tempList = new List<HerbData>();
        for (int i = 0; i < totalHerbsToSort; i++)
        {
            HerbData randomHerb = herbDatabase[Random.Range(0, herbDatabase.Count)];
            tempList.Add(randomHerb);
        }

        // Shuffle (ไม่ให้ชนิดเดิมออกมาติดกันเยอะเกินไป)
        for (int i = tempList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            HerbData temp = tempList[i];
            tempList[i] = tempList[j];
            tempList[j] = temp;
        }

        foreach (HerbData herb in tempList)
            herbQueue.Enqueue(herb);

        Debug.Log($"Generated {herbQueue.Count} herbs in queue");
    }

    // แสดงสมุนไพรชิ้นถัดไปในคิว
    void ShowNextHerb()
    {
        if (herbQueue.Count == 0)
        {
            EndGame(true);
            return;
        }

        currentHerb = herbQueue.Dequeue();

        // อัปเดต UI แสดงสมุนไพรปัจจุบัน
        if (currentHerbDisplay != null)
            currentHerbDisplay.color = currentHerb.herbColor;

        if (currentHerbNameText != null)
            currentHerbNameText.text = currentHerb.herbName;

        feedbackText.text = "";
        UpdateRemainingUI();

        Debug.Log($"Current herb: {currentHerb.herbName} | Remaining: {herbQueue.Count}");
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

    // เรียกจากปุ่มตะกร้าแต่ละใบ (ผูกใน Inspector)
    public void OnMintBasketPressed()
    {
        TrySort("Mint");
    }

    public void OnLavenderBasketPressed()
    {
        TrySort("Lavender");
    }

    public void OnMarigoldBasketPressed()
    {
        TrySort("Marigold");
    }

    void TrySort(string chosenBasket)
    {
        if (isGameOver) return;

        Debug.Log($"กด: {chosenBasket} | สมุนไพรปัจจุบัน: {currentHerb.herbName}");

        if (chosenBasket == currentHerb.basketType)
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

        UpdateScoreUI();
        UpdateProgressUI();

        Debug.Log($"Correct! Sorted: {sortedCount}/{totalHerbsToSort}");

        // แสดง Feedback แล้วเอาชิ้นถัดไปขึ้นมา
        StartCoroutine(NextHerbDelay());
    }

    void RegisterWrong()
    {
        feedbackText.text = "Wrong Basket!";
        feedbackText.color = Color.red;

        // หักคะแนน แต่ไม่ให้ติดลบ
        score = Mathf.Max(0, score - scorePenaltyWrong);
        UpdateScoreUI();

        Debug.Log($"Wrong! -{scorePenaltyWrong} pts | Herb stays: {currentHerb.herbName}");

        // ใส่ชิ้นนี้กลับเข้าคิว (ให้ลองใหม่ทีหลัง)
        herbQueue.Enqueue(currentHerb);

        // ดึงชิ้นถัดไปขึ้นมาแทน
        StartCoroutine(NextHerbDelay());
    }

    IEnumerator NextHerbDelay()
    {
        // ปิดปุ่มชั่วคราว ป้องกันกดซ้ำ
        SetBasketButtonsInteractable(false);
        yield return new WaitForSeconds(0.6f);
        SetBasketButtonsInteractable(true);

        ShowNextHerb();
    }

    void SetBasketButtonsInteractable(bool interactable)
    {
        if (mintBasketButton != null)     mintBasketButton.interactable = interactable;
        if (lavenderBasketButton != null) lavenderBasketButton.interactable = interactable;
        if (marigoldBasketButton != null) marigoldBasketButton.interactable = interactable;
    }

    void UpdateScoreUI()
    {
        scoreText.text = $"SCORE: {score}";
    }

    void UpdateProgressUI()
    {
        progressText.text = $"SORTED: {sortedCount}/{totalHerbsToSort}";
    }

    void UpdateRemainingUI()
    {
        if (remainingText != null)
            remainingText.text = $"Remaining: {herbQueue.Count + 1}";
    }

    void EndGame(bool success)
    {
        isGameOver = true;
        feedbackText.text = "";
        timerText.text = "";

        SetBasketButtonsInteractable(false);

        if (success)
        {
            resultText.text = $"All Herbs Sorted!\nFinal Score: {score}";
            resultText.color = Color.yellow;
        }
        else
        {
            resultText.text = $"Time's Up!\nSorted: {sortedCount}/{totalHerbsToSort}\nScore: {score}";
            resultText.color = new Color(1f, 0.6f, 0f);
        }

        Debug.Log($"=== Game Over === Success: {success} | Score: {score}");
    }
}