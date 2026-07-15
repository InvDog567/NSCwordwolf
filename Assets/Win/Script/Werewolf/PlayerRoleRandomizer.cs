using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.UI;
using System.Collections;

public class PlayerRoleRandomizer : MonoBehaviour
{
    public static PlayerRoleRandomizer Instance;

    public enum Role
    {
        Villager,
        Seer,
        Werewolf,
        Gunner,
        Doctor,
        Jailer,
        Arsonist,
        Witch,
        Vigilante
    }

    [Header("UI")]
    public Image roleImage;

    [Header("Role Sprites (Villager Seer Werewolf Gunner Doctor Jailer Arsonist Witch Vigilante)")]
    public Sprite[] roleSprites;

    [Header("Random Effect")]
    public float shuffleDuration = 3f;
    public float fastSpeed = 0.05f;
    public float slowSpeed = 0.3f;

    [Header("Reveal Effect")]
    public float revealScaleSize = 1.4f;
    public float revealScaleDuration = 0.3f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip tickSound;
    public AudioClip revealSound;

    [Header("After Role Reveal")]
    public float revealDelay = 3f;

    [Header("Timeline")]
    public PlayableDirector timeline;

    [Header("Scenes")]
    public string daySceneName;
    public string jobRandomizerSceneName;

    [Header("Current Role")]
    public Role currentRole;

    [Header("Force Role (Testing Only)")]
    public bool forceRole = false;
    public Role forcedRole = Role.Villager;

    [HideInInspector]
    public bool roleReady = false;

    private int totalRoles = 9;
    private Vector3 originalScale;

    void Awake()
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

    void Start()
    {
        if (timeline != null)
            timeline.playOnAwake = false;

        if (roleImage != null)
            originalScale = roleImage.rectTransform.localScale;

        if (forceRole)
            StartCoroutine(ForceRoleRoutine());
        else
            StartCoroutine(ShuffleRoutine());
    }

    Sprite GetSprite(Role role)
    {
        int idx = (int)role;
        if (roleSprites != null && idx < roleSprites.Length)
            return roleSprites[idx];
        return null;
    }

    void SetImage(Role role)
    {
        if (roleImage != null)
            roleImage.sprite = GetSprite(role);
    }

    void PlayTick()
    {
        if (audioSource != null && tickSound != null)
            audioSource.PlayOneShot(tickSound);
    }

    void PlayReveal()
    {
        if (audioSource != null && revealSound != null)
            audioSource.PlayOneShot(revealSound);
    }

    IEnumerator ShuffleRoutine()
{
    float elapsed = 0f;
    float stepTimer = 0f;

    while (elapsed < shuffleDuration)
    {
        elapsed += Time.deltaTime;
        stepTimer += Time.deltaTime;

        float progress = elapsed / shuffleDuration;
        float currentSpeed = Mathf.Lerp(
            fastSpeed, slowSpeed,
            Mathf.SmoothStep(0f, 1f, progress));

        if (stepTimer >= currentSpeed)
        {
            stepTimer = 0f;
            Role randomRole = (Role)Random.Range(0, totalRoles);
            SetImage(randomRole);
            PlayTick();
        }

        yield return null;
    }

    // Extra slow ticks after main shuffle ends
    // so it visibly crawls to a stop
    float slowElapsed = 0f;
    float slowDuration = 2f;
    float extraStepTimer = 0f;

    while (slowElapsed < slowDuration)
    {
        slowElapsed += Time.deltaTime;
        extraStepTimer += Time.deltaTime;

        // Speed goes from slowSpeed all the way to near zero
        float slowProgress = slowElapsed / slowDuration;
        float crawlSpeed = Mathf.Lerp(
            slowSpeed, 2f,
            Mathf.SmoothStep(0f, 1f, slowProgress));

        if (extraStepTimer >= crawlSpeed)
        {
            extraStepTimer = 0f;
            Role randomRole = (Role)Random.Range(0, totalRoles);
            SetImage(randomRole);
            PlayTick();
        }

        yield return null;
    }

    // Land on final role
    currentRole = (Role)Random.Range(0, totalRoles);
    SetImage(currentRole);
    PlayTick();

    // Wait 2 seconds before reveal effect
    yield return new WaitForSeconds(2f);

    PlayReveal();
    StartCoroutine(PunchScale());

    Debug.Log("Player role is: " + currentRole);

    roleReady = true;

    if (GameManager.Instance != null)
        GameManager.Instance.AssignRoles();

    yield return new WaitForSeconds(revealDelay);

    if (timeline != null)
    {
        timeline.Play();
        while (timeline.state == PlayState.Playing)
            yield return null;
    }

    if (!string.IsNullOrEmpty(jobRandomizerSceneName))
    {
        SceneManager.LoadScene(jobRandomizerSceneName);
    }
    else
    {
        SceneManager.LoadScene(daySceneName);
    }
}

    IEnumerator PunchScale()
{
    if (roleImage == null) yield break;

    Vector3 bigScale = originalScale * revealScaleSize;
    float t = 0f;

    // Scale up only, stay there
    while (t < revealScaleDuration)
    {
        t += Time.deltaTime;
        float progress = t / revealScaleDuration;
        roleImage.rectTransform.localScale =
            Vector3.Lerp(originalScale, bigScale,
                Mathf.SmoothStep(0f, 1f, progress));
        yield return null;
    }

    // Lock at big scale permanently
    roleImage.rectTransform.localScale = bigScale;
}

    IEnumerator ForceRoleRoutine()
    {
        currentRole = forcedRole;
        SetImage(currentRole);
        PlayReveal();

        StartCoroutine(PunchScale());

        Debug.Log("FORCED role: " + currentRole);

        roleReady = true;

        if (GameManager.Instance != null)
            GameManager.Instance.AssignRoles();

        yield return new WaitForSeconds(revealDelay);

        if (timeline != null)
        {
            timeline.Play();
            while (timeline.state == PlayState.Playing)
                yield return null;
        }

        if (!string.IsNullOrEmpty(jobRandomizerSceneName))
        {
            SceneManager.LoadScene(jobRandomizerSceneName);
        }
        else
        {
            SceneManager.LoadScene(daySceneName);
        }
    }

    public bool HasNightAbility()
    {
        return currentRole == Role.Werewolf ||
               currentRole == Role.Seer ||
               currentRole == Role.Doctor ||
               currentRole == Role.Jailer ||
               currentRole == Role.Arsonist ||
               currentRole == Role.Witch;
    }
}