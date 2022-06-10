using UnityEngine;
using System;
using System.Collections;

public class VoiceActorHandler : SingletonNew<VoiceActorHandler>
{
	private SoundCoroutine speakerCoroutine = null;
	private string lastSpeaker = null;

	protected override void SingletonStart()
	{
		speakerCoroutine = new SoundCoroutine(this) { destroyOnFinish = false, DeleteCoroutineOnSwap = false };
	}

	public void StopVoice()
	{
		StopAllCoroutines();
		speakerCoroutine.Stop(false);
	}

	public void PlayVoice(string name, float volume, AudioSource source, AudioClip clip)
	{
		if (name == null)
		{
			Debug.LogWarning($"character name is unreadable! Is this intentional?");
			return;
		}
		if (source != null)
		{
			if (speakerCoroutine.IsPlaying)
			{
				StopAllCoroutines(); // Just in case
				speakerCoroutine.Stop();
			}
			speakerCoroutine.AudioSource = source;
		}
		lastSpeaker = name;
		if (speakerCoroutine.AudioSource == null)
			return;
		speakerCoroutine.AudioClip = new CustomAudioClip(clip)
		{ volume = volume };
		speakerCoroutine.PlaySingle();
	}
}