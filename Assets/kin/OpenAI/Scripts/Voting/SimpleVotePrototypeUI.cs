// Assets/kin/OpenAI/Scripts/Voting/SimpleVotePrototypeUI.cs

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimpleVotePrototypeUI : MonoBehaviour
{
    private static readonly Color BackdropColor = new Color(0.04f, 0.05f, 0.07f, 0.86f);
    private static readonly Color PanelColor = new Color(0.12f, 0.14f, 0.17f, 1f);
    private static readonly Color ButtonColor = new Color(0.20f, 0.24f, 0.28f, 1f);
    private static readonly Color AccentColor = new Color(0.16f, 0.62f, 0.55f, 1f);

    private SimpleVoteManager _voteManager;
    private GameObject _votePanel;
    private Transform _candidateContainer;
    private TMP_Text _resultText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateForPrototypeScene()
    {
        if (!string.Equals(SceneManager.GetActiveScene().name, "t2", System.StringComparison.OrdinalIgnoreCase))
            return;

        if (FindFirstObjectByType<SimpleVotePrototypeUI>() != null)
            return;

        new GameObject("Kin Vote Prototype").AddComponent<SimpleVotePrototypeUI>();
    }

    private void Awake()
    {
        CreateCandidates();

        _voteManager = gameObject.AddComponent<SimpleVoteManager>();
        _voteManager.AutoFindCandidates();

        EnsureEventSystem();
        BuildInterface();
        _voteManager.ConfigureUI(_votePanel, _resultText);
    }

    private void Start()
    {
        BuildCandidateButtons();
        _votePanel.SetActive(false);
    }

    private void CreateCandidates()
    {
        PlayerRole[] roles = FindObjectsByType<PlayerRole>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (PlayerRole role in roles)
        {
            if (role.isPlayer)
                continue;

            SimpleVoteCandidate candidate = role.GetComponent<SimpleVoteCandidate>();
            if (candidate == null)
                candidate = role.gameObject.AddComponent<SimpleVoteCandidate>();

            candidate.Configure(role.gameObject.name, role);
        }
    }

    private void BuildInterface()
    {
        GameObject canvasObject = new GameObject("Kin Vote Canvas", typeof(Canvas), typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Button openButton = CreateButton(canvasObject.transform, "Vote", AccentColor);
        SetRect(openButton.GetComponent<RectTransform>(), new Vector2(150f, 48f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-95f, -48f));

        _votePanel = CreateImage(canvasObject.transform, "Vote Panel", BackdropColor);
        Stretch(_votePanel.GetComponent<RectTransform>());

        GameObject dialog = CreateImage(_votePanel.transform, "Vote Dialog", PanelColor);
        SetRect(dialog.GetComponent<RectTransform>(), new Vector2(560f, 620f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);

        TMP_Text title = CreateText(dialog.transform, "Village Vote", 32f, FontStyles.Bold,
            TextAlignmentOptions.Center);
        SetRect(title.rectTransform, new Vector2(460f, 52f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -48f));

        TMP_Text subtitle = CreateText(dialog.transform, "Choose one villager to eliminate.", 19f,
            FontStyles.Normal, TextAlignmentOptions.Center);
        subtitle.color = new Color(0.75f, 0.78f, 0.82f, 1f);
        SetRect(subtitle.rectTransform, new Vector2(460f, 34f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -90f));

        Button closeButton = CreateButton(dialog.transform, "X", ButtonColor);
        SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(42f, 42f),
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -30f));

        GameObject candidateArea = new GameObject("Candidates", typeof(RectTransform),
            typeof(VerticalLayoutGroup));
        candidateArea.transform.SetParent(dialog.transform, false);
        RectTransform candidateRect = candidateArea.GetComponent<RectTransform>();
        SetRect(candidateRect, new Vector2(460f, 310f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -280f));

        VerticalLayoutGroup layout = candidateArea.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        _candidateContainer = candidateArea.transform;

        _resultText = CreateText(dialog.transform, string.Empty, 18f, FontStyles.Normal,
            TextAlignmentOptions.TopLeft);
        _resultText.color = new Color(0.88f, 0.90f, 0.93f, 1f);
        SetRect(_resultText.rectTransform, new Vector2(460f, 130f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 82f));

        openButton.onClick.AddListener(_voteManager.OpenVotePanel);
        closeButton.onClick.AddListener(_voteManager.CloseVotePanel);
    }

    private void BuildCandidateButtons()
    {
        foreach (SimpleVoteCandidate candidate in _voteManager.Candidates)
        {
            if (candidate == null || candidate.IsEliminated)
                continue;

            Button button = CreateButton(_candidateContainer, candidate.CandidateName, ButtonColor);
            LayoutElement layout = button.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 58f;
            button.gameObject.AddComponent<SimpleVoteButton>().Setup(_voteManager, candidate);
        }
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        new GameObject("Kin Vote EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private static GameObject CreateImage(Transform parent, string objectName, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        imageObject.GetComponent<Image>().color = color;
        return imageObject;
    }

    private static Button CreateButton(Transform parent, string text, Color color)
    {
        GameObject buttonObject = CreateImage(parent, $"{text} Button", color);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();

        TMP_Text label = CreateText(buttonObject.transform, text, 20f, FontStyles.Bold,
            TextAlignmentOptions.Center);
        Stretch(label.rectTransform);
        return button;
    }

    private static TMP_Text CreateText(Transform parent, string text, float size, FontStyles style,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TMP_Text label = textObject.GetComponent<TMP_Text>();
        label.text = text;
        label.font = TMP_Settings.defaultFontAsset;
        label.fontSize = size;
        label.fontStyle = style;
        label.alignment = alignment;
        label.color = Color.white;
        label.enableWordWrapping = true;
        label.raycastTarget = false;
        return label;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetRect(RectTransform rect, Vector2 size, Vector2 anchorMin,
        Vector2 anchorMax, Vector2 position)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }
}
