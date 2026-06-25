using System.Collections;
using UnityEngine;

public class DeskInteractable : MonoBehaviour
{
    [Header("Camera Anchors")]
    public Transform deskCamAnchor;
    public float camLerpSpeed = 5f;

    [Header("Stamps")]
    public StampInteractable passStamp;
    public StampInteractable denyStamp;

    [Header("Player References")]
    public MonoBehaviour fpsController;
    public MonoBehaviour fpsLook;
    public Transform playerCamera;

    [Header("Interaction")]
    public float interactRange = 3f;
    public KeyCode interactKey = KeyCode.E;

    bool isAtDesk = false;
    bool isAnimating = false;
    bool wasLooking = false;

    Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        SetStampsActive(false);
    }

    void Update()
    {
        if (isAtDesk || isAnimating) return;

        bool looking = IsPlayerLookingAtDesk();

        if (looking && !wasLooking)
            InteractPromptUI.Instance.Show("Press E to inspect documents");
        else if (!looking && wasLooking)
            InteractPromptUI.Instance.Hide();

        wasLooking = looking;

        if (looking && Input.GetKeyDown(interactKey))
            EnterDeskMode();
    }

    bool IsPlayerLookingAtDesk()
    {
        if (mainCam == null) return false;

        Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            return hit.collider.transform == transform ||
                   hit.collider.transform.IsChildOf(transform);
        }

        return false;
    }

    void EnterDeskMode()
    {
        isAtDesk = true;
        isAnimating = true;
        wasLooking = false;

        InteractPromptUI.Instance.Hide();

        if (fpsController != null) fpsController.enabled = false;
        if (fpsLook != null) fpsLook.enabled = false;

        StartCoroutine(LerpCamToAnchor(deskCamAnchor, () =>
        {
            isAnimating = false;
            DocumentMinigameManager.Instance.StartMinigame();
        }));
    }

    public void ExitDeskMode()
    {
        isAtDesk = false;
        isAnimating = true;

        SetStampsActive(false);
        InteractPromptUI.Instance.Hide();

        StartCoroutine(LerpCamToAnchor(playerCamera, () =>
        {
            isAnimating = false;

            if (fpsController != null) fpsController.enabled = true;
            if (fpsLook != null) fpsLook.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }));
    }

    IEnumerator LerpCamToAnchor(Transform anchor, System.Action onDone = null)
    {
        if (mainCam == null || anchor == null)
            yield break;

        float t = 0f;

        Vector3 startPos = mainCam.transform.position;
        Quaternion startRot = mainCam.transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime * camLerpSpeed;

            mainCam.transform.position = Vector3.Lerp(startPos, anchor.position, t);
            mainCam.transform.rotation = Quaternion.Lerp(startRot, anchor.rotation, t);

            yield return null;
        }

        mainCam.transform.position = anchor.position;
        mainCam.transform.rotation = anchor.rotation;

        onDone?.Invoke();
    }

    public void SetStampsActive(bool active)
    {
        if (passStamp != null) passStamp.SetInteractable(active);
        if (denyStamp != null) denyStamp.SetInteractable(active);
    }
}