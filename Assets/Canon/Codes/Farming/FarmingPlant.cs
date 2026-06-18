using UnityEngine;

/// <summary>
/// Attach to each plant GameObject in the field.
/// Shows an [E] prompt when the player is close and holding the matching basket.
/// Calls back to FarmingController when picked up.
/// </summary>
public class FarmingPlant : MonoBehaviour
{
    [Header("Plant Settings")]
    public PlantType plantType;

    [Tooltip("How close the player must be to interact")]
    public float interactRange = 2f;

    [Header("Visuals (optional)")]
    [Tooltip("Swap these out to show different meshes/sprites per type")]
    public GameObject cropVisual;
    public GameObject wiltedVisual;
    public GameObject weedVisual;

    // Private

    private FarmingController _controller;
    private bool _promptVisible = false;
    private bool _pickedUp = false;

    void Start()
    {
        _controller = FarmingController.Instance;
        _controller.RegisterPlant(this);
        RefreshVisual();
    }

    void Update()
    {
        if (_pickedUp || _controller == null) return;
        if (!_controller.IsMinigameActive) return;

        // Only interactable if player holds our matching basket
        bool canInteract = _controller.HeldBasket == plantType;

        if (canInteract)
        {
            float dist = Vector3.Distance(transform.position, _controller.PlayerTransform.position);
            bool inRange = dist <= interactRange;

            if (inRange != _promptVisible)
            {
                _promptVisible = inRange;
                _controller.SetPlantPrompt(_promptVisible, transform.position);
            }

            if (inRange && Input.GetKeyDown(KeyCode.E))
                PickUp();
        }
        else if (_promptVisible)
        {
            _promptVisible = false;
            _controller.SetPlantPrompt(false, Vector3.zero);
        }
    }

    private void PickUp()
    {
        _pickedUp = true;
        _promptVisible = false;
        _controller.SetPlantPrompt(false, Vector3.zero);
        _controller.OnPlantPickedUp(this);
        gameObject.SetActive(false);
    }

    // Assign type at runtime (used by field spawner)

    public void SetType(PlantType type)
    {
        plantType = type;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (cropVisual) cropVisual.SetActive(plantType == PlantType.Crop);
        if (wiltedVisual) wiltedVisual.SetActive(plantType == PlantType.WiltedCrop);
        if (weedVisual) weedVisual.SetActive(plantType == PlantType.Weed);
    }

    void OnDisable()
    {
        if (_promptVisible && _controller != null)
            _controller.SetPlantPrompt(false, Vector3.zero);
    }
}