// Assets/Scripts/GameManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager66 : MonoBehaviour
{
    [Header("=== UI Glass ===")]
    public Image glassImage;
    public Image slot1Image;
    public Image slot2Image;

    [Header("=== UI Text ===")]
    public TextMeshProUGUI orderText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI mixHintText;
    public GameObject colorButtonsPanel;
    public Button submitButton;

    [Header("=== Recipe UI ===")]
    public GameObject recipePanel;
    public TextMeshProUGUI recipeText;
    private bool isRecipeOpen = false;

    [Header("=== Settings ===")]
    public float timePerOrder = 20f;
    public int totalOrders = 5;

    private List<DrinkData> drinkDatabase = new List<DrinkData>();

    // ===== ระบบสูตรผสมสีแบบใหม่ — ใช้ List + Tolerance =====
    private List<RecipeEntry> recipeList = new List<RecipeEntry>();

    private class RecipeEntry
    {
        public Color colorA;
        public Color colorB;
        public Color result;
    }

    private DrinkData currentOrder;
    private int currentScore = 0;
    private int currentOrderCount = 0;
    private float currentTime;
    private bool isPlaying = false;

    private Color color1 = Color.clear;
    private Color color2 = Color.clear;
    private int selectStep = 0;

    void Start()
    {
        Debug.Log("=== GameManager Start ===");

        CheckRef(glassImage, "glassImage");
        CheckRef(slot1Image, "slot1Image");
        CheckRef(slot2Image, "slot2Image");
        CheckRef(orderText, "orderText");
        CheckRef(scoreText, "scoreText");
        CheckRef(timerText, "timerText");
        CheckRef(resultText, "resultText");
        CheckRef(mixHintText, "mixHintText");
        CheckRef(colorButtonsPanel, "colorButtonsPanel");
        CheckRef(submitButton, "submitButton");
        CheckRef(recipePanel, "recipePanel");
        CheckRef(recipeText, "recipeText");

        SetupRecipes();

        if (recipeText != null)
        {
            recipeText.text =
                "Color mixing formula\n\n" +
                "Red + Yellow = Orange\n" +
                "Red + Blue = Purple\n" +
                "Red + White = Pink\n" +
                "Yellow + Blue = Green\n" +
                "Yellow + White = Light Yellow\n" +
                "Blue + White = Light Blue\n\n" +
                "Press [E] to close";
        }

        if (recipePanel != null)
            recipePanel.SetActive(false);

        SetupDatabase();
        StartGame();
    }

    void CheckRef(Object obj, string name)
    {
        if (obj == null)
            Debug.LogError("❌ " + name + " ยังไม่ได้ผูกใน Inspector!");
        else
            Debug.Log("✅ " + name + " OK");
    }

    // ===== ตั้งค่าสูตรผสมสี =====
    void SetupRecipes()
    {
        Color red = new Color(1f, 0.15f, 0.15f);
        Color yellow = new Color(1f, 1f, 0.2f);
        Color blue = new Color(0.2f, 0.3f, 1f);
        Color white = new Color(0.95f, 0.95f, 0.95f);

        AddRecipe(red, yellow, new Color(1f, 0.6f, 0f));        // ส้ม
        AddRecipe(red, blue, new Color(0.6f, 0f, 0.8f));        // ม่วง
        AddRecipe(red, white, new Color(1f, 0.4f, 0.6f));       // ชมพู
        AddRecipe(yellow, blue, new Color(0.2f, 0.8f, 0.2f));   // เขียว
        AddRecipe(yellow, white, new Color(1f, 1f, 0.6f));      // เหลืองอ่อน
        AddRecipe(blue, white, new Color(0.6f, 0.75f, 1f));     // ฟ้าอ่อน

        Debug.Log("✅ Recipes setup: " + recipeList.Count);
    }

    void AddRecipe(Color a, Color b, Color result)
    {
        recipeList.Add(new RecipeEntry { colorA = a, colorB = b, result = result });
    }

    // เช็คว่าสี 2 สีใกล้เคียงกันไหม (Tolerance กว้างพอรองรับค่าสีคลาดเคลื่อนจากปุ่ม)
    bool ColorsClose(Color a, Color b, float tolerance = 0.25f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    // ผสมสีตามสูตร ถ้าไม่เจอ = เฉลี่ยสี
    Color MixColors(Color a, Color b)
    {
        foreach (var recipe in recipeList)
        {
            bool matchForward = ColorsClose(a, recipe.colorA) && ColorsClose(b, recipe.colorB);
            bool matchBackward = ColorsClose(a, recipe.colorB) && ColorsClose(b, recipe.colorA);

            if (matchForward || matchBackward)
            {
                Debug.Log("✅ Recipe matched → " + recipe.result);
                return recipe.result;
            }
        }

        Debug.Log("⚠️ No matching recipe, averaging colors. A=" + a + " B=" + b);
        return new Color((a.r + b.r) / 2f, (a.g + b.g) / 2f, (a.b + b.b) / 2f, 1f);
    }

    // ===== Drink Database =====
    void SetupDatabase()
    {
        drinkDatabase = new List<DrinkData>
        {
            new DrinkData { drinkName = "Orange Juice",
                            drinkColor = new Color(1f, 0.6f, 0f) },
            new DrinkData { drinkName = "Grape Juice",
                            drinkColor = new Color(0.6f, 0f, 0.8f) },
            new DrinkData { drinkName = "Watermelon Juice",
                            drinkColor = new Color(1f, 0.4f, 0.6f) },
            new DrinkData { drinkName = "Kiwi Smoothie",
                            drinkColor = new Color(0.2f, 0.8f, 0.2f) },
            new DrinkData { drinkName = "Lemonade",
                            drinkColor = new Color(1f, 1f, 0.6f) },
            new DrinkData { drinkName = "Blue Soda",
                            drinkColor = new Color(0.6f, 0.75f, 1f) },
        };
        Debug.Log("✅ Database: " + drinkDatabase.Count + " drinks");
    }

    void StartGame()
    {
        currentScore = 0;
        currentOrderCount = 0;
        UpdateScoreUI();
        NextOrder();
    }

    public void NextOrder()
    {
        Debug.Log("=== NextOrder === count: " + currentOrderCount);

        if (currentOrderCount >= totalOrders)
        {
            EndGame();
            return;
        }

        int idx = Random.Range(0, drinkDatabase.Count);
        currentOrder = drinkDatabase[idx];

        orderText.text = $"Order #{currentOrderCount + 1}\n\"{currentOrder.drinkName}\"";
        resultText.text = "";

        ResetSlots();

        currentTime = timePerOrder;
        isPlaying = true;
        currentOrderCount++;

        Debug.Log($"✅ Order: {currentOrder.drinkName} | isPlaying = {isPlaying}");
    }

    void ResetSlots()
    {
        color1 = Color.clear;
        color2 = Color.clear;
        selectStep = 0;

        glassImage.color = Color.white;
        if (slot1Image != null) slot1Image.color = Color.grey;
        if (slot2Image != null) slot2Image.color = Color.grey;
        if (mixHintText != null) mixHintText.text = "Pick 2 colors to mix";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            isRecipeOpen = !isRecipeOpen;
            recipePanel.SetActive(isRecipeOpen);
            Debug.Log("Recipe Panel: " + isRecipeOpen);
        }

        if (!isPlaying) return;

        currentTime -= Time.deltaTime;
        timerText.text = $"Time: {Mathf.CeilToInt(currentTime)}s";
        timerText.color = currentTime <= 5f ? Color.red : Color.white;

        if (currentTime <= 0f)
        {
            isPlaying = false;
            ShowResult(false, "⏰ Time's Up!");
            StartCoroutine(NextOrderDelay());
        }
    }

    public void SelectColor(Color color)
    {
        Debug.Log("🔵 SelectColor() called: " + color);

        if (!isPlaying)
        {
            Debug.LogWarning("⚠️ ไม่ทำงานเพราะ isPlaying = false");
            return;
        }

        if (selectStep == 0)
        {
            color1 = color;
            if (slot1Image != null) slot1Image.color = color1;
            if (mixHintText != null) mixHintText.text = "Pick the 2nd color";
            selectStep = 1;
            Debug.Log("✅ เลือกสีที่ 1 แล้ว → selectStep = 1");
        }
        else if (selectStep == 1)
        {
            color2 = color;
            if (slot2Image != null) slot2Image.color = color2;

            Color mixed = MixColors(color1, color2);
            glassImage.color = mixed;
            if (mixHintText != null) mixHintText.text = "Mixed! Press SERVE";

            selectStep = 2;
            Debug.Log("✅ เลือกสีที่ 2 แล้ว → selectStep = 2, Mixed = " + mixed);
        }
        else
        {
            color1 = color;
            color2 = Color.clear;
            if (slot1Image != null) slot1Image.color = color1;
            if (slot2Image != null) slot2Image.color = Color.grey;
            glassImage.color = Color.white;
            if (mixHintText != null) mixHintText.text = "Pick the 2nd color";
            selectStep = 1;
            Debug.Log("🔄 Reset แล้วเลือกสีใหม่ → selectStep = 1");
        }
    }

    public void SubmitDrink()
    {
        Debug.Log("🟢 SubmitDrink() ถูกเรียกแล้ว!");
        Debug.Log("isPlaying = " + isPlaying + " | selectStep = " + selectStep);

        if (!isPlaying)
        {
            Debug.LogWarning("⚠️ หยุดที่ isPlaying = false");
            return;
        }

        if (selectStep < 2)
        {
            Debug.LogWarning("⚠️ หยุดที่ selectStep < 2 (เลือกสีไม่ครบ)");
            resultText.text = "⚠️ Pick 2 colors first!";
            resultText.color = Color.yellow;
            return;
        }

        isPlaying = false;

        Color mixedColor = MixColors(color1, color2);
        bool correct = IsColorMatch(mixedColor, currentOrder.drinkColor);

        Debug.Log($"Mixed: {mixedColor} | Target: {currentOrder.drinkColor} | Match: {correct}");

        if (correct)
        {
            int bonus = Mathf.RoundToInt(currentTime) * 10;
            currentScore += 100 + bonus;
            ShowResult(true, $"✅ Correct! +{100 + bonus} pts");
        }
        else
        {
            ShowResult(false, "❌ Wrong Mix!");
            StartCoroutine(ShowCorrectColorRoutine());
        }

        UpdateScoreUI();
        StartCoroutine(NextOrderDelay());
    }

    bool IsColorMatch(Color a, Color b, float tolerance = 0.2f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    void ShowResult(bool correct, string msg)
    {
        resultText.text = msg;
        resultText.color = correct ? Color.green : Color.red;
    }

    IEnumerator ShowCorrectColorRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        resultText.text += $"\nCorrect: {currentOrder.drinkName}";
        glassImage.color = currentOrder.drinkColor;
    }

    IEnumerator NextOrderDelay()
    {
        yield return new WaitForSeconds(2.5f);
        NextOrder();
    }

    void UpdateScoreUI()
    {
        scoreText.text = $"Score: {currentScore}";
    }

    void EndGame()
    {
        isPlaying = false;
        colorButtonsPanel.SetActive(false);
        submitButton.interactable = false;
        orderText.text = "🎉 Game Over!";
        resultText.text = $"Final Score: {currentScore}";
        resultText.color = Color.yellow;
        timerText.text = "";
    }
}