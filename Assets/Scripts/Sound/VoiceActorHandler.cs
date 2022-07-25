using System;
using UnityEngine;

public class VoiceActorHandler
{
	public string LastSpeaker { get; private set; } = string.Empty;
	public AudioSource audioSource;

	private ResourcesAsset<AudioClip> _currentVoiceLine;
	public ResourcesAsset<AudioClip> CurrentVoiceLine
	{
		get => _currentVoiceLine;
		private set
		{
			_currentVoiceLine = value;
			audioSource.clip = _currentVoiceLine;
		}
	}

	public VoiceActorHandler()
	{

	}

	public void StopVoice()
	{
		if (audioSource != null)
			audioSource.Stop();
	}

	public void PlayVoice(string name, ScriptLine line, float voiceVolume, AudioSource voice)
	{
		CurrentVoiceLine = GetVoiceLine(line);
		if (CurrentVoiceLine != null)
		{
			StopVoice();
			return;
		}
		PlayVoice(name, voiceVolume, voice);
	}
	public void PlayVoice(string name, float volume, AudioSource source, ResourcesAsset<AudioClip> clip = null)
	{
		if (name == null)
		{
			Debug.LogError($"character name is unreadable! Stopping.");
			return;
		}
		StopVoice();
		LastSpeaker = name;
		if (audioSource != source && source != null)
		{
			GameObject.Destroy(audioSource);
			audioSource = source;
		}
		if (clip != null)
			CurrentVoiceLine = clip;
		audioSource.volume = volume;
		audioSource.Play();
	}

	public ResourcesAsset<AudioClip> GetVoiceLine(ScriptLine line)
	{
		string filePath = $"Voice/{line.ScriptDocument}/{line.Index}";
		try
		{
			return new ResourcesAsset<AudioClip>(filePath);
		}
		catch (InvalidOperationException ex)
		{
			Debug.LogError(ex.Message);
			return null;
		}
	}


	~VoiceActorHandler()
	{
		GameObject.Destroy(audioSource);
	}
}