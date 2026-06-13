using UnityEngine;
public class BGMDebug : MonoBehaviour
{
    private AudioSource audioSource;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        Debug.Log("BGM Awake - isPlaying: " + audioSource.isPlaying + " | volume: " + AudioListener.volume);
    }
    
    private void Start()
    {
        Debug.Log("BGM Start - isPlaying: " + audioSource.isPlaying + " | volume: " + AudioListener.volume);
    }
}