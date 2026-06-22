// Assets/Scripts/DoctorManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DoctorManager : MonoBehaviour
{
    [Header("=== UI References ===")]
    public TextMeshProUGUI patientNameText;     // ชื่อผู้ป่วยปัจจุบัน
    public TextMeshProUGUI prescriptionText;     // รายการยาที่ต้องส่ง (พร้อม Highlight ขวดถัดไป)
    public TextMeshProUGUI timerText;            // เวลาที่เหลือต่อขวด
    public TextMeshProUGUI feedbackText;         // ขึ้น Correct! / Wrong!
    public TextMeshProUGUI patientCountText;     // PATIENT: 1/5
    public TextMeshProUGUI resultText;           // ผลลัพธ์ตอนจบเกม

    [Header("=== Settings ===")]
    public float timePerBottle = 4f;             // เวลาจำกัดต่อขวด
    public int totalPatients = 5;                // จำนวนผู้ป่วยทั้งหมด

    // Database ยาทั้งหมดที่มีในร้าน
    private List<string> allMedicineNames = new List<string>
    {
        "Red Tonic", "Blue Serum", "Green Elixir", "Yellow Potion", "Purple Mixture"
    };

    // Database ใบสั่งยาของผู้ป่วยแต่ละคน
    private List<PrescriptionData> prescriptionDatabase = new List<PrescriptionData>();

    // State
    private PrescriptionData currentPrescription;
    private int currentPatientIndex = 0;
    private int currentMedicineStep = 0;   // ตอนนี้ต้องส่งยาตัวที่เท่าไหร่ใน List (0, 1, 2)
    private float currentTime;
    private bool isRoundActive = false;
    private bool isGameOver = false;

    void Start()
    {
        Debug.Log("=== DoctorManager Start ===");
        CheckRef(patientNameText, "patientNameText");
        CheckRef(prescriptionText, "prescriptionText");
        CheckRef(timerText, "timerText");
        CheckRef(feedbackText, "feedbackText");
        CheckRef(patientCountText, "patientCountText");
        CheckRef(resultText, "resultText");

        SetupPrescriptions();
        StartGame();
    }

    void CheckRef(Object obj, string name)
    {
        if (obj == null)
            Debug.LogError("[Missing] " + name + " ยังไม่ได้ผูกใน Inspector!");
        else
            Debug.Log("[OK] " + name);
    }

    void SetupPrescriptions()
    {
        prescriptionDatabase = new List<PrescriptionData>
        {
            new PrescriptionData { patientName = "Jack",
                requiredMedicines = new List<string> { "Red Tonic", "Blue Serum", "Green Elixir" } },

            new PrescriptionData { patientName = "Emma",
                requiredMedicines = new List<string> { "Yellow Potion", "Red Tonic", "Purple Mixture" } },

            new PrescriptionData { patientName = "Liam",
                requiredMedicines = new List<string> { "Green Elixir", "Green Elixir", "Blue Serum" } },

            new PrescriptionData { patientName = "Sophia",
                requiredMedicines = new List<string> { "Purple Mixture", "Yellow Potion", "Red Tonic" } },

            new PrescriptionData { patientName = "Noah",
                requiredMedicines = new List<string> { "Blue Serum", "Green Elixir", "Yellow Potion" } },
        };

        Debug.Log("Prescriptions setup: " + prescriptionDatabase.Count);
    }

    void StartGame()
    {
        currentPatientIndex = 0;
        isGameOver = false;
        resultText.text = "";
        NextPatient();
    }

    void NextPatient()
    {
        if (currentPatientIndex >= totalPatients || currentPatientIndex >= prescriptionDatabase.Count)
        {
            EndGame();
            return;
        }

        currentPrescription = prescriptionDatabase[currentPatientIndex];
        currentMedicineStep = 0;

        patientNameText.text = $"Patient: {currentPrescription.patientName}";
        patientCountText.text = $"PATIENT: {currentPatientIndex + 1}/{totalPatients}";

        UpdatePrescriptionDisplay();
        StartBottleTimer();

        currentPatientIndex++;

        Debug.Log($"New Patient: {currentPrescription.patientName} | Needs: {string.Join(", ", currentPrescription.requiredMedicines)}");
    }

    // แสดงรายการยา พร้อม Highlight ขวดที่ต้องส่งตอนนี้
    void UpdatePrescriptionDisplay()
    {
        string display = "Prescription:\n";

        for (int i = 0; i < currentPrescription.requiredMedicines.Count; i++)
        {
            string medicineName = currentPrescription.requiredMedicines[i];

            if (i < currentMedicineStep)
            {
                display += $"<color=#888888>{i + 1}. {medicineName} (Done)</color>\n";
            }
            else if (i == currentMedicineStep)
            {
                display += $"<color=#FFD700>-> {i + 1}. {medicineName}</color>\n"; // Highlight สีทอง
            }
            else
            {
                display += $"{i + 1}. {medicineName}\n";
            }
        }

        prescriptionText.text = display;
    }

    void StartBottleTimer()
    {
        currentTime = timePerBottle;
        isRoundActive = true;
        feedbackText.text = "";
    }

    void Update()
    {
        if (!isRoundActive || isGameOver) return;

        currentTime -= Time.deltaTime;
        timerText.text = $"Time: {Mathf.CeilToInt(currentTime)}s";
        timerText.color = currentTime <= 1f ? Color.red : Color.white;

        if (currentTime <= 0f)
        {
            Debug.Log("หมดเวลา → MISS");
            RegisterMiss();
        }
    }

    // เรียกจาก MedicineBottle ตอนคลิกขวด
    public void TrySelectMedicine(string clickedMedicineName)
    {
        if (!isRoundActive || isGameOver) return;

        string requiredMedicine = currentPrescription.requiredMedicines[currentMedicineStep];

        Debug.Log($"คลิก: {clickedMedicineName} | ต้องการ: {requiredMedicine}");

        if (clickedMedicineName == requiredMedicine)
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
        currentMedicineStep++;

        feedbackText.text = "Correct!";
        feedbackText.color = Color.green;

        Debug.Log($"Correct! Step = {currentMedicineStep}/{currentPrescription.requiredMedicines.Count}");

        UpdatePrescriptionDisplay();

        if (currentMedicineStep >= currentPrescription.requiredMedicines.Count)
        {
            // ส่งยาครบทุกขวดของผู้ป่วยคนนี้แล้ว
            StartCoroutine(NextPatientDelay());
        }
        else
        {
            StartCoroutine(NextBottleDelay());
        }
    }

    void RegisterMiss()
    {
        isRoundActive = false;

        feedbackText.text = "Wrong!";
        feedbackText.color = Color.red;

        Debug.Log("Wrong! Moving to next bottle anyway (step ยังคงเดิม ให้ลองใหม่)");

        // ตัวเลือก: ให้ข้ามไปขวดต่อไปเลย หรือให้ลองขวดเดิมใหม่
        // ตอนนี้ตั้งให้ "ลองขวดเดิมใหม่" (ไม่ขยับ currentMedicineStep)
        StartCoroutine(NextBottleDelay());
    }

    IEnumerator NextBottleDelay()
    {
        yield return new WaitForSeconds(0.8f);
        StartBottleTimer();
    }

    IEnumerator NextPatientDelay()
    {
        feedbackText.text = "Patient Treated!";
        feedbackText.color = Color.cyan;
        yield return new WaitForSeconds(1.2f);
        NextPatient();
    }

    void EndGame()
    {
        isGameOver = true;
        isRoundActive = false;
        feedbackText.text = "";
        timerText.text = "";
        patientNameText.text = "";
        prescriptionText.text = "";

        resultText.text = "All Patients Treated!\nGreat Job, Doctor!";
        resultText.color = Color.yellow;

        Debug.Log("=== Game Over === All patients treated!");
    }
}
