// Assets/Scripts/GameManager66.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager66 : MonoBehaviour
{
    [Header("=== 3D Liquid ===")]
    public Renderer liquidRenderer;         // ลาก "Liquid" Object มาใส่

    [Header("=== UI Glass (ถ้ายังมี) ===")]
    public Image glassImage;                // ถ้าไม่มีแล้วปล่อยว่างได้

    [Header("=== Slot UI ===")]
    public Image slot1Image;
    public Image slot2Image;

    [Header("=== UI Text ===")]
    public TextMeshProUGUI orderText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI mixHintText;
    public Button submitButton;

    [Header("=== Recipe UI ===")]
    public GameObject recipePanel;
    public TextMeshProUGUI recipeText;
    private bool isRecipeOpen = false;

    [Header("=== Settings ===")]
    public float timePerOrder = 20f;
    public int totalOrders = 5;

    // URP Property
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    // Database
    private List<DrinkData> drinkDatabase = new List<DrinkData>();
    private List<RecipeEntry> recipeList = new List<RecipeEntry>();

    private class RecipeEntry
    {
        public Color colorA;
        public Color colorB;
        public Color result;
    }

    // State
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

    void SetupRecipes()
    {
        Color red    = new Color(1f, 0.15f, 0.15f);
        Color yellow = new Color(1f, 0.85f, 0.2f);
        Color blue   = new Color(0.2f, 0.4f, 1f);
        Color white  = new Color(0.95f, 0.95f, 0.95f);

        AddRecipe(red, yellow, new Color(1f, 0.6f, 0f));
        AddRecipe(red, blue,   new Color(0.6f, 0f, 0.8f));
        AddRecipe(red, white,  new Color(1f, 0.4f, 0.6f));
        AddRecipe(yellow, blue,  new Color(0.2f, 0.8f, 0.2f));
        AddRecipe(yellow, white, new Color(1f, 1f, 0.6f));
        AddRecipe(blue, white,   new Color(0.6f, 0.75f, 1f));

        Debug.Log("Recipes setup: " + recipeList.Count);
    }

    void AddRecipe(Color a, Color b, Color result)
    {
        recipeList.Add(new RecipeEntry { colorA = a, colorB = b, result = result });
    }

    bool ColorsClose(Color a, Color b, float tolerance = 0.25f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    Color MixColors(Color a, Color b)
    {
        foreach (var recipe in recipeList)
        {
            bool matchForward  = ColorsClose(a, recipe.colorA) && ColorsClose(b, recipe.colorB);
            bool matchBackward = ColorsClose(a, recipe.colorB) && ColorsClose(b, recipe.colorA);

            if (matchForward || matchBackward)
            {
                Debug.Log("Recipe matched: " + recipe.result);
                return recipe.result;
            }
        }

        Debug.Log("No recipe found, averaging");
        return new Color((a.r + b.r) / 2f, (a.g + b.g) / 2f, (a.b + b.b) / 2f, 1f);
    }

    void SetupDatabase()
    {
        drinkDatabase = new List<DrinkData>
        {
            new DrinkData { drinkName = "Orange Juice",     drinkColor = new Color(1f, 0.6f, 0f) },
            new DrinkData { drinkName = "Grape Juice",      drinkColor = new Color(0.6f, 0f, 0.8f) },
            new DrinkData { drinkName = "Watermelon Juice", drinkColor = new Color(1f, 0.4f, 0.6f) },
            new DrinkData { drinkName = "Kiwi Smoothie",    drinkColor = new Color(0.2f, 0.8f, 0.2f) },
            new DrinkData { drinkName = "Lemonade",         drinkColor = new Color(1f, 1f, 0.6f) },
            new DrinkData { drinkName = "Blue Soda",        drinkColor = new Color(0.6f, 0.75f, 1f) },
        };

        Debug.Log("Database setup: " + drinkDatabase.Count + " drinks");
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

        Debug.Log("Order: " + currentOrder.drinkName);
    }

    void ResetSlots()
    {
        color1 = Color.clear;
        color2 = Color.clear;
        selectStep = 0;

        // Reset UI Glass
        if (glassImage != null)
            glassImage.color = Color.white;

        // Reset 3D Liquid
        SetLiquidColor(Color.white);

        // Reset Slots
        if (slot1Image != null) slot1Image.color = Color.grey;
        if (slot2Image != null) slot2Image.color = Color.grey;
        if (mixHintText != null) mixHintText.text = "Pick 2 colors to mix";
    }

    void Update()
    {
        // กด E เปิด/ปิด Recipe Panel
        if (Input.GetKeyDown(KeyCode.E))
        {
            isRecipeOpen = !isRecipeOpen;
            if (recipePanel != null)
                recipePanel.SetActive(isRecipeOpen);
        }

        if (!isPlaying) return;

        currentTime -= Time.deltaTime;
        timerText.text = $"Time: {Mathf.CeilToInt(currentTime)}s";
        timerText.color = currentTime <= 5f ? Color.red : Color.white;

        if (currentTime <= 0f)
        {
            isPlaying = false;
            ShowResult(false, "Time's Up!");
            StartCoroutine(NextOrderDelay());
        }
    }

    // เรียกจากขวด 3D (BartenderInteraction) หรือปุ่มสี UI
    public void SelectColor(Color color)
    {
        Debug.Log("SelectColor: " + color);

        if (!isPlaying)
        {
            Debug.LogWarning("isPlaying = false");
            return;
        }

        if (selectStep == 0)
        {
            color1 = color;
            if (slot1Image != null) slot1Image.color = color1;
            if (mixHintText != null) mixHintText.text = "Pick the 2nd color";
            selectStep = 1;
            Debug.Log("Color 1 selected: " + color1);
        }
        else if (selectStep == 1)
        {
            color2 = color;
            if (slot2Image != null) slot2Image.color = color2;

            Color mixed = MixColors(color1, color2);

            // เปลี่ยนสี UI Glass (ถ้ามี)
            if (glassImage != null)
                glassImage.color = mixed;

            // เปลี่ยนสี 3D Liquid
            SetLiquidColor(mixed);

            if (mixHintText != null) mixHintText.text = "Mixed! Press SERVE";
            selectStep = 2;

            Debug.Log("Color 2 selected: " + color2 + " | Mixed: " + mixed);
        }
        else
        {
            // กดใหม่หลังเลือกครบ 2 สี = Reset แล้วเริ่มใหม่
            color1 = color;
            color2 = Color.clear;
            if (slot1Image != null) slot1Image.color = color1;
            if (slot2Image != null) slot2Image.color = Color.grey;

            if (glassImage != null) glassImage.color = Color.white;
            SetLiquidColor(Color.white);

            if (mixHintText != null) mixHintText.text = "Pick the 2nd color";
            selectStep = 1;

            Debug.Log("Reset, Color 1 selected: " + color1);
        }
    }

    // เปลี่ยนสี 3D Liquid ผ่าน URP _BaseColor
    void SetLiquidColor(Color color)
    {
        if (liquidRenderer != null)
            liquidRenderer.material.SetColor(BaseColor, color);
    }

    public void SubmitDrink()
    {
        Debug.Log("SubmitDrink called | isPlaying=" + isPlaying + " | selectStep=" + selectStep);

        if (!isPlaying)
        {
            Debug.LogWarning("isPlaying = false");
            return;
        }

        if (selectStep < 2)
        {
            resultText.text = "Pick 2 colors first!";
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
            ShowResult(true, $"Correct! +{100 + bonus} pts");
        }
        else
        {
            ShowResult(false, "Wrong Mix!");
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

        // แสดงสีที่ถูกต้องทั้ง UI และ 3D
        if (glassImage != null)
            glassImage.color = currentOrder.drinkColor;
        SetLiquidColor(currentOrder.drinkColor);
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
        if (submitButton != null) submitButton.interactable = false;
        orderText.text = "Game Over!";
        resultText.text = $"Final Score: {currentScore}";
        resultText.color = Color.yellow;
        timerText.text = "";
    }
}