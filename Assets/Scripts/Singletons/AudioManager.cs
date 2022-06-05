using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : Singleton<AudioManager>
{
	[SerializeField] private AudioHandler audioHandler;
	private AudioHandler AudioHandler { get
		{
			if (audioHandler == null)
				audioHandler = AudioHandler.Instance;
			return audioHandler;
		} 
	}
	
	public AudioMixer Audio;

	public Sound[] sounds;
	private Dictionary<AudioClip, SoundCoroutine> SoundCoroutineCache
		=> AudioHandler.SoundCoroutineCache;
	private SoundCoroutine speakerCoroutine;
	private string lastSpeaker = null;

	public void FadeIn(string soundPath, float duration)
	{
		AudioHandler.PlayFadedSound(GetSound(soundPath), duration, useCustomAudioData: true);
	}
	public void FadeOut(string soundPath, float duration)
	{
		string[] soundCategories = soundPath.Split('/');
		SoundCoroutineCache[GetSound(soundCategories[soundCategories.Length - 1])].Stop(duration);
	}
	private AudioClip GetSound(string soundPath)
	{
		AudioClip audioClip = Resources.Load<AudioClip>(soundPath);
		if (audioClip == null)
			throw new SoundNotFoundException($"{soundPath} does not lead to a sound" +
			"file!");
		return audioClip;
	}

	public void Play(string soundPath)
	{
		AudioHandler.PlaySound(GetSound(soundPath));
	}
	public override void initialize()
	{
		speakerCoroutine = new SoundCoroutine(this) { destroyOnFinish = false };
		//GameCommands.SwitchingScenes += (sender, args) => 
		//	{ speakerCoroutine.destroyOnFinish = true; speakerCoroutine.Stop(0.3f); };
	}


	public void PlayVoice(string name, float volume, AudioSource source, AudioClip clip)
	{
		if (name == null)
		{
			Debug.LogWarning($"character name is null! Is this intentional?");
			return;
		}
		speakerCoroutine.Stop(0.3f, false);
		if (lastSpeaker != name)
			lastSpeaker = name;
		speakerCoroutine.AudioSource = source;
		speakerCoroutine.AudioClip = new CustomAudioClip(clip) { volume = volume };
		speakerCoroutine.PlaySingle();
	}
	/*
	IEnumerator InterruptVoice(AudioSource source, float volume, AudioClip newClip)
	{
		float currentTime = 0;
		float start = volume;
		while (currentTime < 0.1f)
		{
			currentTime += Time.deltaTime;
			source.volume = Mathf.Lerp(start, 0, currentTime / 0.1f);
			yield return null;
		}
		if (source.volume == 0 && newClip != null)
		{
			source.Stop();
			source.clip = newClip;
			source.volume = start;
			source.Play();
		}
		yield break;
	}

	private void SafeStartCoroutine(String name, IEnumerator thread)
	{
		try
		{
			if (threads[name] == null)
			{
				threads[name] = thread;
				StartCoroutine(thread);
			}
			else
			{
				StopCoroutine(threads[name]);
				threads[name] = thread;
				StartCoroutine(thread);https://soundcloud.com/you/likes
			}
		}
		catch (KeyNotFoundException)
		{
			threads.Add(name, thread);
			StartCoroutine(thread);
		}

	}
	*/



	// This looks stupid but it's the simplest way to bind sliders from the UI
	public void setMasterVolume(float volume)
	{
		Audio.SetFloat("Master", Mathf.Log10(volume) * 20);
	}
	public void setSFXVolume(float volume)
	{
		Audio.SetFloat("SFX", Mathf.Log10(volume) * 20);
	}
	public void setMusicVolume(float volume)
	{
		Audio.SetFloat("Music", Mathf.Log10(volume) * 20);
	}
	public void setUIVolume(float volume)
	{
		Audio.SetFloat("UI", Mathf.Log10(volume) * 20);
	}
	public void setVoiceVolume(float volume)
	{
		Audio.SetFloat("Voice", Mathf.Log10(volume) * 20);
	}

}