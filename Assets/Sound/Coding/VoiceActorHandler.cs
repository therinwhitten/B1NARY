using UnityEngine;
using System;
using System.Collections;

public class VoiceActorHandler : SingletonNew<VoiceActorHandler>
{
	private SoundCoroutine speakerCoroutine = null;
	private string lastSpeaker = null;

	protected override void SingletonStart()
	{
		speakerCoroutine = new SoundCoroutine(this) { destroyOnFinish = false };
	}

	public void PlayVoice(string name, float volume, AudioSource source, AudioClip clip)
	{
		if (name == null)
		{
			Debug.LogWarning($"character name is unreadable! Is this intentional?");
			return;
		}
		canPlay = true;
		// if (speakerCoroutine.IsPlaying)
		// {
		// 	speakerCoroutine.Stop(0.2f, false);
		// 	StartCoroutine(Delay());
		// }
		// else
			PlayNew(name, volume, source, clip);

		// IEnumerator Delay()
		// {
		// 	yield return new WaitForSeconds(0.2f);
		// 	PlayNew(name, volume, source, clip);
		// }
	}

	// I hope i find a better way than having global variables here.
	private bool canPlay = false;
	private void PlayNew(string name, float volume, AudioSource source, AudioClip clip)
	{
		if (!canPlay)
			return;
		if (speakerCoroutine.IsPlaying)
			speakerCoroutine.Stop(false);

		if (lastSpeaker != name)
			lastSpeaker = name;
		if (speakerCoroutine.AudioSource != source)
			speakerCoroutine.AudioSource = source;
		speakerCoroutine.AudioClip = new CustomAudioClip(clip) 
			{ volume = volume };
		speakerCoroutine.PlaySingle();
	}
}