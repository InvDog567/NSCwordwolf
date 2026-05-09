using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class NightTimer : MonoBehaviour
{
    public TMP_Text timerText;

    public float timer = 30f;

    public string voteSceneName;

    void Update()
    {
        timer -= Time.deltaTime;

        timerText.text =
            Mathf.Ceil(timer).ToString();

        if (timer <= 0)
        {
            // NPC wolf auto kill
            GameManager.Instance
            .NPCWerewolfKill();

            // Go vote scene
            SceneManager.LoadScene(
                voteSceneName);
        }
    }
}