using System;
using UnityEngine;
using TMPro;

// ============================================================
//  PlayerWallet.cs
//  Centralized player money manager.
//
//  Usage from any script:
//      PlayerWallet.Instance.AddCoins(10);
//      PlayerWallet.Instance.RemoveCoins(5);   // returns false if broke
//      int balance = PlayerWallet.Instance.GetCoins();
//      bool canAfford = PlayerWallet.Instance.HasCoins(3);
//
//  The TMP_Text display updates automatically on every change.
//  Future AI/dialogue scripts should read GetCoins() — never read the UI.
// ============================================================

public class PlayerWallet : MonoBehaviour
{
    // --------------------------------------------------------
    // Singleton
    // --------------------------------------------------------
    public static PlayerWallet Instance { get; private set; }

    // --------------------------------------------------------
    // Inspector
    // --------------------------------------------------------
    [Header("Display")]
    [Tooltip("Drag a TMP_Text element from your HUD Canvas here.")]
    public TMP_Text coinDisplay;

    [Tooltip("Format string for the coin display. {0} is replaced with the coin amount.")]
    public string displayFormat = "Coins: {0}";

    [Header("Starting Balance")]
    [Tooltip("How many coins the player starts with.")]
    public int startingCoins = 0;

    // --------------------------------------------------------
    // Event — subscribe to get notified on any balance change
    // Usage: PlayerWallet.Instance.OnCoinsChanged += MyMethod;
    // --------------------------------------------------------
    public event Action OnCoinsChanged;

    // --------------------------------------------------------
    // Private state
    // --------------------------------------------------------
    private int _coins;

    // --------------------------------------------------------
    // Lifecycle
    // --------------------------------------------------------
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _coins = Mathf.Max(0, startingCoins);
        RefreshDisplay();
    }

    // --------------------------------------------------------
    // Public API
    // --------------------------------------------------------

    /// <summary>Returns the current coin balance.</summary>
    public int GetCoins() => _coins;

    /// <summary>Returns true if the player has at least <paramref name="amount"/> coins.</summary>
    public bool HasCoins(int amount) => _coins >= amount;

    /// <summary>
    /// Adds coins to the balance. Amount must be positive.
    /// Fires OnCoinsChanged and updates the display.
    /// </summary>
    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("[PlayerWallet] AddCoins called with non-positive amount: " + amount);
            return;
        }

        _coins += amount;
        Debug.Log($"[PlayerWallet] +{amount} coins → balance: {_coins}");

        RefreshDisplay();
        OnCoinsChanged?.Invoke();
    }

    /// <summary>
    /// Removes coins from the balance. Balance will never drop below 0.
    /// Returns <c>true</c> if the player had enough coins; <c>false</c> otherwise.
    /// </summary>
    public bool RemoveCoins(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("[PlayerWallet] RemoveCoins called with non-positive amount: " + amount);
            return false;
        }

        if (_coins < amount)
        {
            Debug.Log($"[PlayerWallet] Not enough coins. Need {amount}, have {_coins}.");
            return false;
        }

        _coins -= amount;
        Debug.Log($"[PlayerWallet] -{amount} coins → balance: {_coins}");

        RefreshDisplay();
        OnCoinsChanged?.Invoke();
        return true;
    }

    // --------------------------------------------------------
    // Internal
    // --------------------------------------------------------
    private void RefreshDisplay()
    {
        if (coinDisplay != null)
            coinDisplay.text = string.Format(displayFormat, _coins);
    }
}
