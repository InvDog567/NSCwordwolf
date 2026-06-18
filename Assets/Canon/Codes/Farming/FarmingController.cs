using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Singleton controller for the farming minigame.
/// Attach to the Player (or a persistent manager object).
/// Can be disabled in the Editor — auto-enables on Play.
/// </summary>
public class FarmingController : MonoBehaviour
{
    // Singleton
    public static FarmingController Instance { get; private set; }

    // Public state
    public bool IsMinigameActive { get; private set; } = false;

    /// <summary>The station whose basket is currently held. Null = none.</summary>
    public BasketStation ActiveStation { get; private set; } = null;

    /// <summary>Convenience — what type is in the held basket. Null if none.</summary>
    public PlantType? HeldBasket => ActiveStation?.basketType;

    public Transform PlayerTransform => _playerTransform;

    // Inspector

    [Header("References")]
    public Transform playerTransform;
    public MonoBehaviour playerMovement;    // FPS movement — never disabled during farming

    [Header("UI")]
    public GameObject promptPanel;
    public TextMeshProUGUI promptText;

    [Tooltip("Shows how many plants are left on the field")]
    public TextMeshProUGUI counterText;

    [Tooltip("Shows which basket is currently held")]
    public TextMeshProUGUI heldBasketText;

    [Header("Completion")]
    public GameObject completionPanel;
    public float completionDisplayTime = 3f;

    // Private

    private Transform _playerTransform;
    private List<FarmingPlant> _allPlants = new List<FarmingPlant>();
    private List<BasketStation> _stations = new List<BasketStation>();
    private int _remainingCount = 0;

    //  Unity lifecycle

    void Awake()
    {
        enabled = true;
        Instance = this;
        _playerTransform = playerTransform != null ? playerTransform : transform;
    }

    void Start()
    {
        SetPromptVisible(false);
        if (completionPanel) completionPanel.SetActive(false);
        UpdateCounterUI();
        UpdateHeldBasketUI();
    }

    //  Registration

    public void RegisterPlant(FarmingPlant plant)
    {
        _allPlants.Add(plant);
        _remainingCount++;
        UpdateCounterUI();
    }

    public void RegisterStation(BasketStation station)
    {
        if (!_stations.Contains(station))
            _stations.Add(station);
    }

    //  Minigame start

    public void StartMinigame()
    {
        if (IsMinigameActive) return;
        IsMinigameActive = true;
        Debug.Log(" FarmingMinigame Started");
        UpdateCounterUI();
    }

    //  Called by BasketStation

    public void SwapToStation(BasketStation newStation)
    {
        // Return the previous basket to its station
        if (ActiveStation != null)
            ActiveStation.ReturnToStation();

        // Pick up the new one
        ActiveStation = newStation;
        ActiveStation.AttachToPlayer();

        UpdateHeldBasketUI();
    }

    //  Called by FarmingPlant

    public void OnPlantPickedUp(FarmingPlant plant)
    {
        _remainingCount--;
        UpdateCounterUI();
        UpdateHeldBasketUI();

        if (_remainingCount <= 0)
            StartCoroutine(ShowCompletion());
    }

    //  Prompts

    public void SetPlantPrompt(bool visible, Vector3 worldPos)
    {
        if (visible) ShowPrompt("[E]  Pick up");
        else SetPromptVisible(false);
    }

    public void SetStationPrompt(bool visible, string label, Vector3 worldPos)
    {
        if (visible) ShowPrompt(label);
        else SetPromptVisible(false);
    }

    //  UI

    private void ShowPrompt(string label)
    {
        if (promptText) promptText.text = label;
        SetPromptVisible(true);
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptPanel) promptPanel.SetActive(visible);
    }

    private void UpdateCounterUI()
    {
        if (counterText)
            counterText.text = $"Remaining: {_remainingCount}";
    }

    private void UpdateHeldBasketUI()
    {
        if (heldBasketText)
            heldBasketText.text = HeldBasket.HasValue ? $"{HeldBasket} Basket" : "No basket";
    }

    //  Completion

    private System.Collections.IEnumerator ShowCompletion()
    {
        IsMinigameActive = false;
        SetPromptVisible(false);

        // Return whatever basket is held
        if (ActiveStation != null)
        {
            ActiveStation.ReturnToStation();
            ActiveStation = null;
        }

        if (completionPanel) completionPanel.SetActive(true);
        yield return new WaitForSeconds(completionDisplayTime);
        if (completionPanel) completionPanel.SetActive(false);
    }
}