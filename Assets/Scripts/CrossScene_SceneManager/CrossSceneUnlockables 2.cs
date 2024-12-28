using B1NARY.DataPersistence;
using UnityEngine;
using UnityEngine.Audio;

public class CrossSceneUnlockables : MonoBehaviour
{
	public AudioSource notificationSoundSource;
	public AudioClip notificationSoundClip;
	public AudioMixerGroup mixerGroup;

	/*
	private void OnEnable()
	{
		CollectibleCollection.UnlockedUnlockableEvent += ShowNotification;
	}
	private void OnDisable()
	{
		CollectibleCollection.UnlockedUnlockableEvent -= ShowNotification;
	}

	public void ShowNotification(string type, string flagName, bool alreadyContains)
	{
		if (alreadyContains)
			return;

		// Play notification sound
		if (notificationSoundSource != null && notificationSoundClip != null)
		{
			notificationSoundSource.outputAudioMixerGroup = mixerGroup;
			notificationSoundSource.PlayOneShot(notificationSoundClip);
		}
	}
	*/
}
