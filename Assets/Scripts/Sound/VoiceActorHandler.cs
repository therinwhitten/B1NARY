using UnityEngine;
using System;
using System.Collections;

public class VoiceActorHandler : MonoBehaviour
{
	public SoundCoroutine SpeakerCoroutine { get; private set; } = null;
	public string LastSpeaker { get; private set; }

	private void Start()
	{
		SwitchSceneCheck();
	}

	public void StopVoice()
	{
		if (SpeakerCoroutine.AudioSource != null)
			SpeakerCoroutine.Stop();
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

	private void SwitchSceneCheck()
	{
		if (SpeakerCoroutine != null)
		{
			SpeakerCoroutine.DeleteCoroutineOnSwap = true;
			SpeakerCoroutine.destroyOnFinish = true;
			SpeakerCoroutine.Stop();
			SpeakerCoroutine = null;
		}
		SpeakerCoroutine = new SoundCoroutine(this, string.Empty)
		{ destroyOnFinish = false, DeleteCoroutineOnSwap = false };
		GameCommands.SwitchingScenes += SwitchSceneCheck;
	}
}