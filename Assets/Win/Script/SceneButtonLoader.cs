using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneKeyLoader : MonoBehaviour
{
    [Header("Type Key Here")]
    public string keyToPress = "e";

    [Header("Scene To Load")]
    public string sceneName;

    void Update()
    {
        if (Input.GetKeyDown(keyToPress))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}