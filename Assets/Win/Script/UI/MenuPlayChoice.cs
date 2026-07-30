using UnityEngine;

public class MenuPlayChoice : MonoBehaviour
{
    [Header("Choice UI")]
    [Tooltip("The canvas or panel containing the three play choices.")]
    public GameObject choiceCanvas;

    [Header("Continue")]
    [Tooltip("The scene that the normal Play button used to open.")]
    public string continueSceneName = "Role";

    private bool isContinuing;

    private void Awake()
    {
        if (choiceCanvas != null)
            choiceCanvas.SetActive(false);
    }

    public bool ShowChoices()
    {
        if (isContinuing)
            return true;

        if (choiceCanvas == null)
            return false;

        choiceCanvas.SetActive(true);
        return true;
    }

    public void ContinueGame()
    {
        if (isContinuing)
            return;

        if (string.IsNullOrWhiteSpace(continueSceneName))
        {
            Debug.LogError("MenuPlayChoice: Continue Scene Name is empty.");
            return;
        }

        isContinuing = true;

        if (choiceCanvas != null)
            choiceCanvas.SetActive(false);

        SceneLoader.LoadSceneWithLoadingScreen(continueSceneName);
    }
}
