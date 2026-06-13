using UnityEngine;
using UnityEngine.Playables;

public class PlayButtonTimeline : MonoBehaviour
{
    [SerializeField] private PlayableDirector fadeTimeline;
    [SerializeField] private GameObject fadeObject;

    public void OnPlayClicked()
    {
        if (fadeObject != null)
            fadeObject.SetActive(true);

        if (fadeTimeline != null)
            fadeTimeline.Play();
    }
}