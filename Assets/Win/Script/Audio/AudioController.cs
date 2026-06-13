using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour
{
    [Header("Delayed Audio")]
    public bool useDelayedAudio = true;
    public AudioSource delayedAudioSource;
    public float delaySeconds = 5f;

    [Header("Random Chance Audio")]
    public bool useChanceAudio = true;

    [Tooltip("Audio sources that can be randomly chosen")]
    public AudioSource[] chanceAudioSources;

    [Tooltip("Starting chance percentage")]
    public float startingChance = 0f;

    [Tooltip("Chance increase every second")]
    public float chanceIncreasePerSecond = 1f;

    private float currentChance;
    private bool hasPlayed = false;

    void Start()
    {
        if (useDelayedAudio && delayedAudioSource != null)
        {
            StartCoroutine(PlayDelayedAudio());
        }

        if (useChanceAudio && chanceAudioSources.Length > 0)
        {
            currentChance = startingChance;
            StartCoroutine(TryPlayChanceAudio());
        }
    }

    IEnumerator PlayDelayedAudio()
    {
        yield return new WaitForSeconds(delaySeconds);
        delayedAudioSource.Play();
    }

    IEnumerator TryPlayChanceAudio()
    {
        while (!hasPlayed)
        {
            yield return new WaitForSeconds(1f);

            if (Random.Range(0f, 100f) < currentChance)
            {
                int randomIndex = Random.Range(0, chanceAudioSources.Length);

                if (chanceAudioSources[randomIndex] != null)
                {
                    chanceAudioSources[randomIndex].Play();
                }

                hasPlayed = true;
            }
            else
            {
                currentChance += chanceIncreasePerSecond;
                currentChance = Mathf.Clamp(currentChance, 0f, 100f);
            }
        }
    }
}