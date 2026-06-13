using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class NightTimer : MonoBehaviour
{
    public TMP_Text timerText;
    public float timer = 30f;
    public string voteSceneName;
    public string loseSceneName;

    private bool transitioning = false;

    void Update()
    {
        if (transitioning) return;

        timer -= Time.deltaTime;
        timerText.text = Mathf.Ceil(timer).ToString();

        if (timer <= 0)
        {
            transitioning = true;
            GameManager.Instance.NPCWerewolfKill();

            if (GameManager.Instance.playerKilledByWolf)
            {
                SceneManager.LoadScene(loseSceneName);
            }
            else
            {
                SceneManager.LoadScene(voteSceneName);
            }
        }
    }
}