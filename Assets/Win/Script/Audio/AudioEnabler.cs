using UnityEngine;

public class AudioEnabler : MonoBehaviour
{
    void Awake()
    {
        AudioListener.volume = 1f;
    }
}