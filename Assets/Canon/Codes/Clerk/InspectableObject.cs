using System.Collections;
using UnityEngine;

public class InspectableObject : MonoBehaviour
{
    [Header("Object To Move")]
    public Transform objectRoot;

    [Header("Inspect Position")]
    public Transform inspectAnchor;
    public float lerpSpeed = 8f;

    [Header("Interaction")]
    public float interactRange = 10f;
    public KeyCode interactKey = KeyCode.E;
    public string inspectPrompt = "Press E to read";
    public string returnPrompt = "Press E to put down";

    bool isInspecting = false;
    bool isAnimating = false;
    bool wasLooking = false;

    Vector3 restPosition;
    Quaternion restRotation;
    Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;

        if (objectRoot == null)
            objectRoot = transform;

        restPosition = objectRoot.position;
        restRotation = objectRoot.rotation;
    }

    void Update()
    {
        if (isAnimating) return;

        if (isInspecting)
        {
            objectRoot.position = Vector3.Lerp(objectRoot.position, inspectAnchor.position, Time.deltaTime * lerpSpeed);
            objectRoot.rotation = Quaternion.Lerp(objectRoot.rotation, inspectAnchor.rotation, Time.deltaTime * lerpSpeed);

            if (!wasLooking)
            {
                InteractPromptUI.Instance.Show(returnPrompt);
                wasLooking = true;
            }

            if (Input.GetKeyDown(interactKey))
                StartCoroutine(ReturnToRest());
        }
        else
        {
            bool looking = IsMouseOverObject();

            if (looking && !wasLooking)
                InteractPromptUI.Instance.Show(inspectPrompt);
            else if (!looking && wasLooking)
                InteractPromptUI.Instance.Hide();

            wasLooking = looking;

            if (looking && Input.GetKeyDown(interactKey))
                StartCoroutine(BringClose());
        }
    }

    bool IsMouseOverObject()
    {
        if (mainCam == null) return false;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            return hit.collider.transform == transform ||
                   hit.collider.transform.IsChildOf(transform) ||
                   hit.collider.transform == objectRoot ||
                   hit.collider.transform.IsChildOf(objectRoot);
        }

        return false;
    }

    IEnumerator BringClose()
    {
        isAnimating = true;
        isInspecting = true;
        wasLooking = false;
        InteractPromptUI.Instance.Hide();

        float t = 0f;
        Vector3 startPos = objectRoot.position;
        Quaternion startRot = objectRoot.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime * lerpSpeed;
            objectRoot.position = Vector3.Lerp(startPos, inspectAnchor.position, t);
            objectRoot.rotation = Quaternion.Lerp(startRot, inspectAnchor.rotation, t);
            yield return null;
        }

        objectRoot.position = inspectAnchor.position;
        objectRoot.rotation = inspectAnchor.rotation;

        isAnimating = false;
    }

    IEnumerator ReturnToRest()
    {
        isAnimating = true;
        isInspecting = false;
        wasLooking = false;
        InteractPromptUI.Instance.Hide();

        float t = 0f;
        Vector3 startPos = objectRoot.position;
        Quaternion startRot = objectRoot.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime * lerpSpeed;
            objectRoot.position = Vector3.Lerp(startPos, restPosition, t);
            objectRoot.rotation = Quaternion.Lerp(startRot, restRotation, t);
            yield return null;
        }

        objectRoot.position = restPosition;
        objectRoot.rotation = restRotation;

        isAnimating = false;
    }

    public void ForceReturn()
    {
        StopAllCoroutines();
        StartCoroutine(ReturnToRest());
    }
}