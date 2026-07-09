// Assets/Scripts/MiniGameManager.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class MiniGameManager : MonoBehaviour
{
    // ===== Singleton =====
    public static MiniGameManager Instance { get; private set; }

    [Header("=== UI References (MiniGameSelector Scene) ===")]
    public TextMeshProUGUI miniGameNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI countdownText;
    public Image themeColorPanel;

    [Header("=== Scene Names ===")]
    public string villageSceneName = "Village";

    // ===== Mini Game Database =====
    [System.Serializable]
    public class MiniGameData
    {
        public string miniGameName;
        public string sceneName;
        public string spawnPointName;
        public string description;
        public Color themeColor;
    }

    public List<MiniGameData> miniGameDatabase = new List<MiniGameData>
    {
        new MiniGameData {
            miniGameName = "Bartender",
            sceneName = "BartenderScene",
            spawnPointName = "SpawnPoint_Bartender",
            description = "Mix drinks by matching colors!\nSelect 2 colors to blend.",
            themeColor = new Color(0.8f, 0.3f, 0.3f)
        },
        new MiniGameData {
            miniGameName = "Blacksmith",
            sceneName = "BlacksmithScene",
            spawnPointName = "SpawnPoint_Blacksmith",
            description = "Forge weapons with perfect timing!\nHit the sweet spot.",
            themeColor = new Color(0.6f, 0.4f, 0.2f)
        },
        new MiniGameData {
            miniGameName = "Woodcutter",
            sceneName = "WoodcutterScene",
            spawnPointName = "SpawnPoint_Woodcutter",
            description = "Chop wood to the rhythm!\nPress Z/X/C/V at the right moment.",
            themeColor = new Color(0.3f, 0.6f, 0.2f)
        },
        new MiniGameData {
            miniGameName = "Carpenter",
            sceneName = "CarpenterScene",
            spawnPointName = "SpawnPoint_Carpenter",
            description = "Carve wood into weapon shapes!\nFollow the blueprint.",
            themeColor = new Color(0.7f, 0.5f, 0.2f)
        },
        new MiniGameData {
            miniGameName = "Doctor",
            sceneName = "DoctorScene",
            spawnPointName = "SpawnPoint_Doctor",
            description = "Prepare medicine for patients!\nSelect the right bottles in order.",
            themeColor = new Color(0.3f, 0.7f, 0.7f)
        },
        new MiniGameData {
            miniGameName = "Herbalist",
            sceneName = "HerbalistScene",
            spawnPointName = "SpawnPoint_Herbalist",
            description = "Sort herbs into the right baskets!\nIdentify each herb quickly.",
            themeColor = new Color(0.3f, 0.7f, 0.3f)
        },
    };

    // ===== State =====
    public MiniGameData SelectedMiniGame { get; private set; }

    void Awake()
    {
        // Singleton + DontDestroyOnLoad
        // ทำให้ข้อมูลไม่หายตอนเปลี่ยน Scene
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("=== MiniGameManager Initialized ===");
    }

    void Start()
    {
        // สุ่มทันทีตอนเริ่ม
        RollMiniGame();
    }

    // ===== สุ่มมินิเกม =====
    public void RollMiniGame()
    {
        int randomIndex = Random.Range(0, miniGameDatabase.Count);
        SelectedMiniGame = miniGameDatabase[randomIndex];

        Debug.Log($"Selected MiniGame: {SelectedMiniGame.miniGameName}");

        UpdateSelectorUI();
    }

    // ===== อัปเดต UI ใน Selector Scene =====
    void UpdateSelectorUI()
    {
        if (miniGameNameText != null)
            miniGameNameText.text = SelectedMiniGame.miniGameName;

        if (descriptionText != null)
            descriptionText.text = SelectedMiniGame.description;

        if (themeColorPanel != null)
            themeColorPanel.color = SelectedMiniGame.themeColor;

        if (countdownText != null)
            countdownText.text = "";
    }

    // ===== ปุ่ม PLAY NOW → ไปมินิเกมทันที =====
    public void OnPlayNowPressed()
    {
        if (SelectedMiniGame == null)
        {
            Debug.LogError("ยังไม่ได้สุ่มมินิเกม!");
            return;
        }

        Debug.Log($"Loading: {SelectedMiniGame.sceneName}");
        SceneManager.LoadScene(SelectedMiniGame.sceneName);
    }

    // ===== ปุ่ม GO TO VILLAGE → ไปหมู่บ้าน Player จะ Spawn ตรงจุดที่ถูกต้อง =====
    public void OnGoToVillagePressed()
    {
        if (SelectedMiniGame == null)
        {
            Debug.LogError("ยังไม่ได้สุ่มมินิเกม!");
            return;
        }

        Debug.Log($"Going to Village | SpawnPoint: {SelectedMiniGame.spawnPointName}");
        SceneManager.LoadScene(villageSceneName);
    }

    // ===== ปุ่ม REROLL → สุ่มใหม่ =====
    public void OnRerollPressed()
    {
        RollMiniGame();
        Debug.Log("Rerolled!");
    }

    // ===== กลับไปหน้า Selector =====
    public void GoToSelector()
    {
        SceneManager.LoadScene("MiniGameSelectorScene");
    }
}
