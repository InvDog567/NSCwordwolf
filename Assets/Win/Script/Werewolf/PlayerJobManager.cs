using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class PlayerJobManager : MonoBehaviour
{
    public static PlayerJobManager Instance { get; private set; }

    public enum Job
    {
        None,
        Clerk,
        Herbalist,
        Farming,
        Doctor,
        Carpenter,
        Fishing,
        Woodcutter,
        Blacksmith,
        Bartender
    }


    [Header("Current Assigned Job")]
    public Job currentJob = Job.None;

    [Header("Forced Job (Testing Only)")]
    public bool forceJob = false;
    public Job forcedJob = Job.None;

    [Header("UI (For Job Randomization Screen)")]
    public Image jobImage;
    public Sprite[] jobSprites; // Sprites matching the jobs (Clerk=0, Herbalist=1, Farming=2, etc.)

    [Header("Randomization Settings")]
    public float shuffleDuration = 3f;
    public float fastSpeed = 0.05f;
    public float slowSpeed = 0.3f;
    public float revealDelay = 3f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip tickSound;
    public AudioClip revealSound;

    [Header("Next Scene to Load")]
    public string daySceneName = "Day";

    private Vector3 originalScale;

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
        }
    }

    private void Start()
    {
        if (jobImage != null)
            originalScale = jobImage.rectTransform.localScale;
    }

    public void StartJobRandomizer()
    {
        if (forceJob)
        {
            StartCoroutine(ForceJobRoutine());
        }
        else
        {
            StartCoroutine(ShuffleJobRoutine());
        }
    }

    private Sprite GetSprite(Job job)
    {
        if (jobSprites == null || jobSprites.Length == 0) return null;
        int idx = (int)job - 1;
        if (idx >= 0 && idx < jobSprites.Length)
            return jobSprites[idx];
        return null;
    }

    private void SetImage(Job job)
    {
        if (jobImage != null)
            jobImage.sprite = GetSprite(job);
    }

    private void PlayTick()
    {
        if (audioSource != null && tickSound != null)
            audioSource.PlayOneShot(tickSound);
    }

    private void PlayReveal()
    {
        if (audioSource != null && revealSound != null)
            audioSource.PlayOneShot(revealSound);
    }

    private IEnumerator ShuffleJobRoutine()
    {
        float elapsed = 0f;
        float stepTimer = 0f;

        while (elapsed < shuffleDuration)
        {
            elapsed += Time.deltaTime;
            stepTimer += Time.deltaTime;

            float progress = elapsed / shuffleDuration;
            float currentSpeed = Mathf.Lerp(fastSpeed, slowSpeed, Mathf.SmoothStep(0f, 1f, progress));

            if (stepTimer >= currentSpeed)
            {
                stepTimer = 0f;
                Job randomJob = (Job)Random.Range(1, 10); // 1 to 9 (exclusive of 10)
                SetImage(randomJob);
                PlayTick();
            }

            yield return null;
        }

        // Lander
        currentJob = (Job)Random.Range(1, 10);
        SetImage(currentJob);
        PlayTick();
        PlayReveal();

        yield return new WaitForSeconds(revealDelay);

        SceneManager.LoadScene(daySceneName);
    }

    private IEnumerator ForceJobRoutine()
    {
        currentJob = forcedJob;
        SetImage(currentJob);
        PlayReveal();

        yield return new WaitForSeconds(revealDelay);

        SceneManager.LoadScene(daySceneName);
    }

    public bool IsDayTime()
    {
        return FindFirstObjectByType<DayAbility>() != null || SceneManager.GetActiveScene().name.ToLower().Contains("day");
    }

    public bool CanPlayMinigame(Job requiredJob)
    {
        return currentJob == requiredJob && IsDayTime();
    }
}
