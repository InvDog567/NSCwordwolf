using UnityEngine;
public class ButtonSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clickSound;
    
    public void PlayClickSound()
    {
        audioSource.PlayOneShot(clickSound);
    }
}