using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [Header("Loading UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private Image loadingProgressBar;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Scene")]
    [Tooltip("Used when LoadConfiguredScene() is called.")]
    [SerializeField] private string targetSceneName = "GameScene";

    [Header("Loading Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Tooltip("The loading screen stays visible for at least this long.")]
    [SerializeField] private float minimumLoadingTime = 3.5f;

    [SerializeField] private bool useProgressBar = true;

    [Tooltip("When enabled, the progress bar fills based on minimumLoadingTime.")]
    [SerializeField] private bool useFakeProgress = true;

    [SerializeField] private bool animateLoadingDots = true;

    private bool isLoading;

    private void Awake()
    {
        ResetLoadingUI();
    }

    /// <summary>
    /// Loads the scene assigned to Target Scene Name in the Inspector.
    /// This is convenient for a Button's OnClick event.
    /// </summary>
    public void LoadConfiguredScene()
    {
        LoadScene(targetSceneName);
    }

    /// <summary>
    /// Loads a scene using a scene name supplied by another script
    /// or a Button's OnClick string parameter.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isLoading)
            return;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("SceneLoader: No scene name was provided.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"SceneLoader: Scene '{sceneName}' could not be loaded. " +
                "Check its name and make sure it is included in Build Profiles."
            );

            return;
        }

        isLoading = true;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        ShowLoadingUI();

        // Fade the complete loading UI onto the screen.
        yield return FadeCanvasGroup(0f, 1f);

        // Give Unity a frame to render the loading screen.
        yield return null;

        float timer = 0f;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        // Unity reaches 0.9 when the scene is loaded and waiting for activation.
        while (operation.progress < 0.9f || timer < minimumLoadingTime)
        {
            timer += Time.unscaledDeltaTime;

            float realProgress =
                Mathf.Clamp01(operation.progress / 0.9f);

            float timeProgress = minimumLoadingTime > 0f
                ? Mathf.Clamp01(timer / minimumLoadingTime)
                : 1f;

            float displayedProgress;

            if (useFakeProgress)
            {
                // Fills smoothly according to the minimum loading time.
                displayedProgress = timeProgress;
            }
            else
            {
                // Respects both the real load and minimum loading duration.
                displayedProgress = Mathf.Min(
                    realProgress,
                    timeProgress
                );
            }

            UpdateLoadingUI(displayedProgress);

            yield return null;
        }

        UpdateLoadingUI(1f);

        // Wait one frame at 100% before changing scenes.
        yield return null;

        operation.allowSceneActivation = true;
    }

    private void ShowLoadingUI()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.interactable = false;
            loadingCanvasGroup.blocksRaycasts = true;
        }

        if (loadingProgressBar != null)
        {
            loadingProgressBar.gameObject.SetActive(useProgressBar);
            loadingProgressBar.fillAmount = 0f;
        }

        if (loadingText != null)
            loadingText.text = "Loading";
    }

    private void UpdateLoadingUI(float progress)
    {
        if (useProgressBar && loadingProgressBar != null)
            loadingProgressBar.fillAmount = progress;

        if (loadingText != null)
        {
            loadingText.text = animateLoadingDots
                ? GetAnimatedLoadingText()
                : "Loading";
        }
    }

    private IEnumerator FadeCanvasGroup(float from, float to)
    {
        if (loadingCanvasGroup == null)
            yield break;

        if (fadeDuration <= 0f)
        {
            loadingCanvasGroup.alpha = to;
            yield break;
        }

        float timer = 0f;
        loadingCanvasGroup.alpha = from;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                timer / fadeDuration
            );

            loadingCanvasGroup.alpha = Mathf.Lerp(
                from,
                to,
                progress
            );

            yield return null;
        }

        loadingCanvasGroup.alpha = to;
    }

    private string GetAnimatedLoadingText()
    {
        int dotCount =
            Mathf.FloorToInt(Time.unscaledTime * 3f) % 4;

        switch (dotCount)
        {
            case 1:
                return "Loading.";

            case 2:
                return "Loading..";

            case 3:
                return "Loading...";

            default:
                return "Loading";
        }
    }

    private void ResetLoadingUI()
    {
        isLoading = false;

        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.interactable = false;
            loadingCanvasGroup.blocksRaycasts = true;
        }

        if (loadingProgressBar != null)
            loadingProgressBar.fillAmount = 0f;

        if (loadingText != null)
            loadingText.text = "Loading";

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    private void OnValidate()
    {
        fadeDuration = Mathf.Max(0f, fadeDuration);
        minimumLoadingTime = Mathf.Max(0f, minimumLoadingTime);
    }
}