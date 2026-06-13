using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameEndHandler : MonoBehaviour
{
    [Header("Settings")]
    public float delayBeforeMenu = 5f;
    public string mainMenuSceneName;

    [Header("UI")]
    public TMP_Text countdownText;

    void Start()
    {
        StartCoroutine(ReturnToMenu());
    }

    IEnumerator ReturnToMenu()
    {
        float timer = delayBeforeMenu;

        while (timer > 0)
        {
            if (countdownText != null)
                countdownText.text =
                    "Returning to menu in " +
                    Mathf.Ceil(timer).ToString() + "...";

            timer -= Time.deltaTime;
            yield return null;
        }

        ClearAllData();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void ClearAllData()
    {
        if (PlayerRoleRandomizer.Instance != null)
            Destroy(PlayerRoleRandomizer.Instance.gameObject);

        if (GameManager.Instance != null)
            Destroy(GameManager.Instance.gameObject);
    }
}