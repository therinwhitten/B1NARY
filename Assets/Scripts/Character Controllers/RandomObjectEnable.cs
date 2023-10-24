using UnityEngine;
using System.Collections;

public class RandomObjectEnable : MonoBehaviour
{
    public GameObject objectToEnable;
    public float initialEnableDelay = 2.0f; // Delay for the first enable
    public float minEnableDelay = 5.0f;
    public float maxEnableDelay = 10.0f;
    public float minDisableDelay = 5.0f;
    public float maxDisableDelay = 10.0f;
    public bool enableDisableLoop = true; // Option to enable/disable loop
    private AudioSource audioSource;

    void Start()
    {
        audioSource = objectToEnable.GetComponent<AudioSource>(); // Fixed the typo
        StartCoroutine(EnableDisableLoop());
    }

    IEnumerator EnableDisableLoop()
    {
        // Initial enable delay
        yield return new WaitForSeconds(initialEnableDelay);

        while (true || enableDisableLoop)
        {
            float enableDelay = Random.Range(minEnableDelay, maxEnableDelay);
            yield return new WaitForSeconds(enableDelay);

            objectToEnable.SetActive(true);

            if (audioSource != null && audioSource.clip != null)
            {
                float clipDuration = audioSource.clip.length;
                yield return new WaitForSeconds(clipDuration);
            }

            float disableDelay = Random.Range(minDisableDelay, maxDisableDelay);
            yield return new WaitForSeconds(disableDelay);

            objectToEnable.SetActive(false);
        }
    }
}
