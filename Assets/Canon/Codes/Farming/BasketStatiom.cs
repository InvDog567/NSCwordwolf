using UnityEngine;

/// <summary>
/// Place 3 of these around the field — one per PlantType.
/// Press E near a station to pick up that basket (automatically returns the previous one).
/// The basket object teleports to the player hold point on pickup, returns to its station on swap.
/// </summary>
public class BasketStation : MonoBehaviour
{
    [Header("Station Settings")]
    public PlantType basketType;
    public float interactRange = 2f;

    [Header("Basket Object")]
    [Tooltip("The basket GameObject sitting at this station")]
    public GameObject basketObject;

    [Tooltip("The hold point on the player where the basket snaps to. Same transform for all 3 stations.")]
    public Transform playerHoldPoint;

    // Private

    private FarmingController _controller;
    private bool _promptVisible = false;
    private Vector3 _restPosition;
    private Quaternion _restRotation;


    void Awake()
    {
        // Store rest pose early — basket hasn't moved yet
        if (basketObject != null)
        {
            _restPosition = basketObject.transform.position;
            _restRotation = basketObject.transform.rotation;
        }
    }

    void Start()
    {
        // Singleton is guaranteed to exist by Start
        _controller = FarmingController.Instance;
        if (_controller == null)
            Debug.LogError($"[BasketStation] FarmingController.Instance is null! Make sure FarmingController is in the scene.", this);
        else
            _controller.RegisterStation(this);
    }

    void Update()
    {
        if (_controller == null || !_controller.IsMinigameActive) return;

        // Can't pick up the basket already held
        bool alreadyHeld = _controller.ActiveStation == this;
        if (alreadyHeld) return;

        float dist = Vector3.Distance(transform.position, _controller.PlayerTransform.position);
        bool inRange = dist <= interactRange;

        if (inRange != _promptVisible)
        {
            _promptVisible = inRange;
            _controller.SetStationPrompt(_promptVisible, $"[E]  Pick up {basketType} Basket", transform.position);
        }

        if (inRange && Input.GetKeyDown(KeyCode.E))
            _controller.SwapToStation(this);
    }

    /// <summary>Move the basket to the player's hold point.</summary>
    public void AttachToPlayer()
    {
        if (basketObject == null || playerHoldPoint == null) return;
        basketObject.transform.SetParent(playerHoldPoint);
        basketObject.transform.localPosition = Vector3.zero;
        basketObject.transform.localRotation = Quaternion.identity;
    }

    /// <summary>Return the basket to its resting position at the station.</summary>
    public void ReturnToStation()
    {
        if (basketObject == null) return;
        basketObject.transform.SetParent(null);
        basketObject.transform.position = _restPosition;
        basketObject.transform.rotation = _restRotation;
    }

    void OnDisable()
    {
        if (_promptVisible && _controller != null)
            _controller.SetStationPrompt(false, "", Vector3.zero);
    }
}