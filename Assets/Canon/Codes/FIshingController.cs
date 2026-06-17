using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FishingController : MonoBehaviour
{
    public bool IsFishing { get; private set; } = false;

    [Header("Bite Timing")]
    public float minBiteTime = 5f;
    public float maxBiteTime = 25f;

    [Header("References")]
    public FishingUI fishingUI;
    public FishingMinigame minigame;

    private Coroutine _fishingRoutine;

    private bool _lockPosition;
    private Vector3 _lockedPosition;

    [Header("Minigame Objects")]
    public GameObject[] minigameObjects;

    void Awake()
    {
        enabled = true;
    }

    void LateUpdate()
    {
        if (_lockPosition)
        {
            transform.position = _lockedPosition;
        }
    }

    public void SetPromptVisible(bool visible)
    {
        if (fishingUI != null)
            fishingUI.ShowPrompt(visible);
    }

    public void StartFishing()
    {
        if (IsFishing) return;

        IsFishing = true;

        if (fishingUI != null)
            fishingUI.ShowPrompt(false);

        LockMovement(true);

        _fishingRoutine = StartCoroutine(FishingRoutine());
    }

    private void SetMinigameObjects(bool active)
    {
        if (minigameObjects == null) return;

        foreach (GameObject obj in minigameObjects)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }

    private IEnumerator FishingRoutine()
    {
        fishingUI.ShowWaiting(true);
        SetMinigameObjects(true);

        float waitTime = Random.Range(minBiteTime, maxBiteTime);
        yield return new WaitForSeconds(waitTime);

        fishingUI.ShowWaiting(false);

        fishingUI.ShowBiteAlert(true);
        yield return new WaitForSeconds(0.6f);
        fishingUI.ShowBiteAlert(false);

        bool caught = false;
        yield return minigame.RunMinigame(result => caught = result);

        SetMinigameObjects(false);

        fishingUI.ShowResult(caught);
        yield return new WaitForSeconds(1.5f);
        fishingUI.HideResult();

        EndFishing();
    }

    private void EndFishing()
    {
        IsFishing = false;

        LockMovement(false);

        _fishingRoutine = null;
    }

    private void LockMovement(bool locked)
    {
        _lockPosition = locked;

        if (locked)
            _lockedPosition = transform.position;
    }

    private void OnDisable()
    {
        if (_fishingRoutine != null)
            StopCoroutine(_fishingRoutine);

        IsFishing = false;
        LockMovement(false);
        SetMinigameObjects(false);

        if (fishingUI != null)
        {
            fishingUI.ShowWaiting(false);
            fishingUI.ShowBiteAlert(false);
            fishingUI.HideResult();
        }
    }
}