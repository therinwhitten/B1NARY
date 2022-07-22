using System;
using UnityEngine;

public class VoiceActorHandler
{
	public string LastSpeaker { get; private set; } = string.Empty;
	public AudioSource audioSource;

	private VoiceLine? _currentVoiceLine;
	public VoiceLine? CurrentVoiceLine
	{
		get => _currentVoiceLine;
		private set
		{
			if (_currentVoiceLine != null)
				_currentVoiceLine.Value.Dispose();
			_currentVoiceLine = value;
			audioSource.clip = _currentVoiceLine.Value.voiceLine;
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

	public void PlayVoice(string name, DialogueLine line, float voiceVolume, AudioSource voice)
	{
		CurrentVoiceLine = GetVoiceLine(line);
		if (!CurrentVoiceLine.HasValue)
		{
			StopVoice();
			return;
		}
		PlayVoice(name, voiceVolume, voice);
	}
	public void PlayVoice(string name, float volume, AudioSource source, VoiceLine? clip = null)
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
		if (clip.HasValue)
			CurrentVoiceLine = clip.Value;
		audioSource.volume = volume;
		audioSource.Play();
	}

	public VoiceLine? GetVoiceLine(DialogueLine line)
	{
		string filePath = $"Voice/{line.scriptName}/{line.index}";
		try
		{
			return new VoiceLine(filePath);
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