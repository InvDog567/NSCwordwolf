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
        Werewolf
    }

    [Header("UI")]
    public TMP_Text roleText;

    [Header("Random Effect")]
    public float shuffleDuration = 3f;
    public float shuffleSpeed = 0.1f;

    [Header("After Role Reveal")]
    public float revealDelay = 3f;
    public string nextSceneName;

    [Header("Current Role")]
    public Role currentRole;

    private string[] roleNames = { "Villager", "Seer", "Werewolf" };

    void Awake()
    {
        // Keep this object between scenes
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

    IEnumerator RandomizeRole()
    {
        float timer = 0f;

        // Fake random swapping effect
        while (timer < shuffleDuration)
        {
            int randomIndex = Random.Range(0, roleNames.Length);

            roleText.text = roleNames[randomIndex];

            switch (roleNames[randomIndex])
            {
                case "Villager":
                    roleText.color = Color.green;
                    break;

                case "Seer":
                    roleText.color = Color.blue;
                    break;

                case "Werewolf":
                    roleText.color = Color.red;
                    break;
            }

            yield return new WaitForSeconds(shuffleSpeed);

            timer += shuffleSpeed;
        }

        // Final role
        currentRole = (Role)Random.Range(0, 3);

        roleText.text = currentRole.ToString();

        switch (currentRole)
        {
            case Role.Villager:
                roleText.color = Color.green;
                break;

            case Role.Seer:
                roleText.color = Color.blue;
                break;

            case Role.Werewolf:
                roleText.color = Color.red;
                break;
        }

        Debug.Log("Player role is: " + currentRole);

        // Wait before changing scene
        yield return new WaitForSeconds(revealDelay);

        SceneManager.LoadScene(nextSceneName);
    }

    public bool HasNightAbility()
    {
        return currentRole != Role.Villager;
    }
}