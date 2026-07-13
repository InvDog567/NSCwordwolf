using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class NightTimer : MonoBehaviour
{
    public TMP_Text timerText;
    public string arsonistKilledSceneName;
    public float timer = 30f;
    public string voteSceneName;
    public string loseSceneName;
    public string wolfKilledSceneName;

    private bool transitioning = false;

    void Update()
    {
        if (transitioning) return;

        timer -= Time.deltaTime;
        timerText.text = Mathf.Ceil(timer).ToString();

        if (timer <= 0)
{
    transitioning = true;

    if (NPCRoleLogic.Instance != null)
        NPCRoleLogic.Instance.RunNightActions();

    GameManager.Instance.NPCWerewolfKill();

    if (GameManager.Instance.playerKilledByArsonist)
    {
        SceneManager.LoadScene(arsonistKilledSceneName);
    }
    else if (GameManager.Instance.playerKilledByWolf)
    {
        SceneManager.LoadScene(wolfKilledSceneName);
    }
    else
    {
        SceneManager.LoadScene(voteSceneName);
    }
}
    }
}