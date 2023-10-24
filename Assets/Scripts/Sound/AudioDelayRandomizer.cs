using System.Collections;
using UnityEngine;

public class AudioDelayRandomizer : MonoBehaviour
{
    public AudioClip[] audioClips;
    private AudioSource audioSource;
    public float minDelay = 1.0f;
    public float maxDelay = 3.0f;
    private float nextClipTime;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlayRandomClipWithDelay());
    }

    IEnumerator PlayRandomClipWithDelay()
    {
        while (true)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            if (audioClips != null && audioClips.Length > 0)
            {
                int randomIndex = Random.Range(0, audioClips.Length);
                audioSource.clip = audioClips[randomIndex];
                if (audioSource != null)
                {
                    audioSource.Play();
                    yield return new WaitForSeconds(audioSource.clip.length);
                }
            }
        }
    }

    // This method clears the audio clip and stops playback.
    public void Disable()
    {
        if (audioSource != null)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }
        gameObject.SetActive(false);
    }

    // This method allows you to choose a new random audio clip when re-enabling the object.
    public void EnableWithNewClip()
    {
        if (audioClips != null && audioClips.Length > 0)
        {
            int randomIndex = Random.Range(0, audioClips.Length);
            if (audioSource != null)
            {
                audioSource.clip = audioClips[randomIndex];
                audioSource.Play();
            }
            gameObject.SetActive(true);
        }
    }

    private void OnEnable()
    {
        EnableWithNewClip();
    }
}
