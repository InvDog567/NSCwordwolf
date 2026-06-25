using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DocumentMinigameManager : MonoBehaviour
{
    [Header("Settings")]
    [Range(1, 20)]
    public int customerCount = 7;
    [Range(0f, 1f)]
    public float fakeRatio = 0.4f;

    [Header("References")]
    public DeskInteractable desk;
    public InspectableObject documentObject;
    public InspectableObject ledgerObject;
    public DocumentDisplayUI documentDisplay;
    public LedgerDisplayUI ledgerDisplay;
    public DeskHUDUI hudUI;
    public ResultsScreenUI resultsScreen;

    [Header("Customer")]
    [Tooltip("Drag your existing placeholder character here. It lives in the scene and gets moved around.")]
    public GameObject customerPlaceholder;
    public Transform customerSpawnPoint;
    public Transform customerDeskPoint;
    public Transform customerExitPoint;
    public float customerMoveSpeed = 2f;

    public UnityEvent onMinigameStart;
    public UnityEvent onMinigameEnd;

    Queue<DocumentData> customerQueue = new Queue<DocumentData>();
    DocumentData currentDoc;
    int correctCount = 0;
    int wrongCount = 0;
    int currentIndex = 0;

    public static DocumentMinigameManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Make sure HUD and customer are hidden at game start
        if (hudUI != null) hudUI.gameObject.SetActive(false);
        if (customerPlaceholder != null) customerPlaceholder.SetActive(false);
    }

    public void StartMinigame()
    {
        correctCount = 0;
        wrongCount = 0;
        currentIndex = 0;

        // Show HUD now that minigame is running
        if (hudUI != null) hudUI.gameObject.SetActive(true);

        // Unlock cursor so player can look around the desk
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        BuildQueue();
        onMinigameStart?.Invoke();
        StartCoroutine(ShowNextCustomer());
    }

    void BuildQueue()
    {
        customerQueue.Clear();

        List<bool> fakeFlags = new List<bool>();
        int fakeCount = Mathf.RoundToInt(customerCount * fakeRatio);

        for (int i = 0; i < fakeCount; i++) fakeFlags.Add(true);
        for (int i = fakeCount; i < customerCount; i++) fakeFlags.Add(false);

        // Fisher-Yates shuffle
        for (int i = fakeFlags.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            bool tmp = fakeFlags[i];
            fakeFlags[i] = fakeFlags[j];
            fakeFlags[j] = tmp;
        }

        foreach (bool isFake in fakeFlags)
            customerQueue.Enqueue(DocumentGenerator.Generate(isFake));
    }

    IEnumerator ShowNextCustomer()
    {
        if (customerQueue.Count == 0)
        {
            EndMinigame();
            yield break;
        }

        currentDoc = customerQueue.Dequeue();
        currentIndex++;

        // Move customer in from spawn point
        yield return StartCoroutine(MoveCustomer(customerSpawnPoint.position, customerDeskPoint.position));

        // Populate document and ledger
        documentDisplay.Populate(currentDoc);
        ledgerDisplay.Populate(currentDoc);
        hudUI.UpdateHUD(currentIndex, customerCount, correctCount, wrongCount);

        // Enable stamps now that doc is presented
        desk.SetStampsActive(true);
    }

    public void OnStamp(bool playerChosePass)
    {
        desk.SetStampsActive(false);

        bool correct = (playerChosePass == currentDoc.isValid);
        if (correct) correctCount++;
        else wrongCount++;

        hudUI.ShowFeedback(correct);
        hudUI.UpdateHUD(currentIndex, customerCount, correctCount, wrongCount);

        StartCoroutine(AfterStamp());
    }

    IEnumerator AfterStamp()
    {
        yield return new WaitForSeconds(1.2f);

        yield return StartCoroutine(MoveCustomer(customerDeskPoint.position, customerExitPoint.position));

        documentObject.ForceReturn();
        ledgerObject.ForceReturn();

        yield return new WaitForSeconds(0.3f);
        StartCoroutine(ShowNextCustomer());
    }

    IEnumerator MoveCustomer(Vector3 from, Vector3 to)
    {
        if (customerPlaceholder == null) yield break;

        customerPlaceholder.SetActive(true);
        customerPlaceholder.transform.position = from;

        // Face the desk (roughly toward player camera)
        Vector3 dir = (to - from);
        if (dir != Vector3.zero)
            customerPlaceholder.transform.rotation = Quaternion.LookRotation(dir);

        float dist = Vector3.Distance(from, to);
        float duration = dist / customerMoveSpeed;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            customerPlaceholder.transform.position = Vector3.Lerp(from, to, t / duration);
            yield return null;
        }

        customerPlaceholder.transform.position = to;

        // Hide after reaching exit
        if (to == customerExitPoint.position)
            customerPlaceholder.SetActive(false);
    }

    void EndMinigame()
    {
        // Hide HUD
        if (hudUI != null) hudUI.gameObject.SetActive(false);

        onMinigameEnd?.Invoke();
        desk.ExitDeskMode();
        resultsScreen.Show(correctCount, wrongCount, customerCount);
    }
}