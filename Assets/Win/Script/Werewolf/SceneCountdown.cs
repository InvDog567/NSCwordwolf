using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SceneCountdownTMP : MonoBehaviour
{
    public float countdownTime = 10f;
    public TMP_Text countdownText;
    public string nextSceneName;

    private float currentTime;

    void Start()
    {
        currentTime = countdownTime;
    }

    void Update()
    {
        currentTime -= Time.deltaTime;

        if (currentTime < 0)
            currentTime = 0;

        countdownText.text = Mathf.Ceil(currentTime).ToString();

        if (currentTime <= 0)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}