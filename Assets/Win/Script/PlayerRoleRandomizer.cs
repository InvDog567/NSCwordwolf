using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
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
        Jailer
    }

    [Header("UI")]
    public TMP_Text roleText;

    [Header("Random Effect")]
    public float shuffleDuration = 3f;
    public float shuffleSpeed = 0.1f;

    [Header("After Role Reveal")]
    public float revealDelay = 3f;

    [Header("Scenes")]
    public string daySceneName;

    [Header("Current Role")]
    public Role currentRole;

    [Header("Force Role (Testing Only)")]
    public bool forceRole = false;
    public Role forcedRole = Role.Villager;

    [HideInInspector]
    public bool roleReady = false;

    private string[] roleNames =
    {
        "Villager",
        "Seer",
        "Werewolf",
        "Gunner",
        "Doctor",
        "Jailer"
    };

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
        StartCoroutine(RandomizeRole());
    }

    Color GetRoleColor(string roleName)
    {
        switch (roleName)
        {
            case "Villager":
                return Color.green;
            case "Seer":
                return Color.blue;
            case "Werewolf":
                return Color.red;
            case "Gunner":
                return new Color(248f / 255f, 255f / 255f, 0f);
            case "Doctor":
                return new Color(0f, 255f / 255f, 208f / 255f);
            case "Jailer":
                return new Color(166f / 255f, 0f, 255f / 255f);
            default:
                return Color.white;
        }
    }

    IEnumerator RandomizeRole()
    {
        if (forceRole)
        {
            // Skip shuffle, show forced role immediately
            currentRole = forcedRole;
            string forcedName = currentRole.ToString();
            roleText.text = forcedName;
            roleText.color = GetRoleColor(forcedName);
            Debug.Log("FORCED role: " + currentRole);
        }
        else
        {
            float timer = 0f;

            while (timer < shuffleDuration)
            {
                string randomName =
                    roleNames[Random.Range(0, roleNames.Length)];

                roleText.text = randomName;
                roleText.color = GetRoleColor(randomName);

                yield return new WaitForSeconds(shuffleSpeed);
                timer += shuffleSpeed;
            }

            currentRole = (Role)Random.Range(0, roleNames.Length);
            string finalName = currentRole.ToString();
            roleText.text = finalName;
            roleText.color = GetRoleColor(finalName);
            Debug.Log("Player role is: " + currentRole);
        }

        roleReady = true;

        if (GameManager.Instance != null)
            GameManager.Instance.AssignRoles();

        yield return new WaitForSeconds(revealDelay);

        SceneManager.LoadScene(daySceneName);
    }

    public bool HasNightAbility()
    {
        return currentRole == Role.Werewolf ||
               currentRole == Role.Seer ||
               currentRole == Role.Doctor ||
               currentRole == Role.Jailer;
    }
}