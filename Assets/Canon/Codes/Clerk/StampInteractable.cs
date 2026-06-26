using System.Collections;
using UnityEngine;

public class StampInteractable : MonoBehaviour
{
    [Header("Settings")]
    public bool isPassStamp = true;
    public float interactRange = 10f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Stamp Animation")]
    public float stampDropDistance = 0.3f;
    public float stampSpeed = 10f;

    bool interactable = false;
    bool isAnimating = false;
    bool wasLooking = false;

    Vector3 restPosition;
    Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        restPosition = transform.position;
    }

    void Update()
    {
        if (!interactable || isAnimating) return;

        bool looking = IsMouseOverObject();

        if (looking && !wasLooking)
            InteractPromptUI.Instance.Show(isPassStamp ? "Press E to Approve" : "Press E to Reject");
        else if (!looking && wasLooking)
            InteractPromptUI.Instance.Hide();

        wasLooking = looking;

        if (looking && Input.GetKeyDown(interactKey))
            StartCoroutine(DoStamp());
    }

    bool IsMouseOverObject()
    {
        if (mainCam == null) return false;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            return hit.collider.gameObject == gameObject ||
                   hit.collider.transform.IsChildOf(transform);
        }

        return false;
    }

    IEnumerator DoStamp()
    {
        isAnimating = true;
        wasLooking = false;
        InteractPromptUI.Instance.Hide();

        Vector3 downPos = restPosition + Vector3.down * stampDropDistance;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * stampSpeed;
            transform.position = Vector3.Lerp(restPosition, downPos, t);
            yield return null;
        }

        DocumentMinigameManager.Instance.OnStamp(isPassStamp);

        yield return new WaitForSeconds(0.15f);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * stampSpeed;
            transform.position = Vector3.Lerp(downPos, restPosition, t);
            yield return null;
        }

        transform.position = restPosition;
        isAnimating = false;
    }

    public void SetInteractable(bool active)
    {
        interactable = active;

        if (!active)
        {
            wasLooking = false;
            InteractPromptUI.Instance.Hide();
        }
    }
}