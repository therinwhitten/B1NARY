using UnityEngine;
using System;
using System.Collections;

public class VoiceActorHandler
{
	public MonoBehaviour CoroutineStarter { get; set; }
	private SoundCoroutine speakerCoroutine = null;
	public string LastSpeaker { get; private set; }

	public VoiceActorHandler(MonoBehaviour coroutineStarter)
	{
		CoroutineStarter = coroutineStarter;
		SwitchSceneCheck(null, EventArgs.Empty);
	}

	public void StopVoice()
	{
		if (speakerCoroutine.AudioSource != null)
			speakerCoroutine.Stop(false);
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
			if (speakerCoroutine.AudioSource != null)
				if (speakerCoroutine.IsPlaying)
				{
					speakerCoroutine.Stop();
				}
			speakerCoroutine.AudioSource = source;
		}
		LastSpeaker = name;
		if (speakerCoroutine.AudioSource == null)
			return;
		speakerCoroutine.AudioClip = new CustomAudioClip(clip)
		{ volume = volume };
		speakerCoroutine.PlaySingle();
	}

	private void SwitchSceneCheck(object sender, EventArgs args)
	{
		speakerCoroutine = new SoundCoroutine(CoroutineStarter)
		{ destroyOnFinish = false, DeleteCoroutineOnSwap = false };
		GameCommands.SwitchingScenes += SwitchSceneCheck;
	}
}