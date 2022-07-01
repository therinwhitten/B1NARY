using UnityEngine;
using System;
using System.Collections;

public class VoiceActorHandler
{
	public MonoBehaviour CoroutineStarter { get; set; }
	public SoundCoroutine SpeakerCoroutine { get; private set; } = null;
	public string LastSpeaker { get; private set; }

	public VoiceActorHandler(MonoBehaviour coroutineStarter)
	{
		CoroutineStarter = coroutineStarter;
		SwitchSceneCheck(null, EventArgs.Empty);
	}

	public void StopVoice()
	{
		if (SpeakerCoroutine.AudioSource != null)
			SpeakerCoroutine.Stop(false);
	}

	public void PlayVoice(string name, float volume, AudioSource source, AudioClip clip)
	{
		if (name == null)
		{
			Debug.LogError($"character name is unreadable! Stopping.");
			return;
		}
		if (source != null)
		{
			if (SpeakerCoroutine.AudioSource != null)
				if (SpeakerCoroutine.IsPlaying)
				{
					SpeakerCoroutine.Stop();
				}
			SpeakerCoroutine.AudioSource = source;
		}
		LastSpeaker = name;
		if (SpeakerCoroutine.AudioSource == null)
			return;
		SpeakerCoroutine.AudioClip = new CustomAudioClip(clip)
		{ volume = volume };
		SpeakerCoroutine.PlaySingle();
	}

	private void SwitchSceneCheck(object sender, EventArgs args)
	{
		SpeakerCoroutine = new SoundCoroutine(CoroutineStarter, string.Empty)
		{ destroyOnFinish = false, DeleteCoroutineOnSwap = false };
		GameCommands.SwitchingScenes += SwitchSceneCheck;
	}
}