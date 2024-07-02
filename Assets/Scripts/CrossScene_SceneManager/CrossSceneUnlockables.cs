using UnityEngine;
using UnityEngine.Audio;

public class CrossSceneUnlockables : MonoBehaviour
{
    public AudioSource notificationSoundSource;
    public AudioClip notificationSoundClip;
    public AudioMixerGroup mixerGroup;

    public void ShowNotification()
    {
        // Play notification sound
        if (notificationSoundSource != null && notificationSoundClip != null)
        {
            notificationSoundSource.outputAudioMixerGroup = mixerGroup;
            notificationSoundSource.PlayOneShot(notificationSoundClip);
        }
    }
}
