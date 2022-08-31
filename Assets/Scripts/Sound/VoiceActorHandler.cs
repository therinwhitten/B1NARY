namespace B1NARY.Audio
{
	using System;
	using UnityEngine;
	using B1NARY.Scripting.Experimental;

	public class VoiceActorHandler
	{
		public string LastSpeaker { get; private set; } = string.Empty;
		public AudioSource audioSource;

		private AudioClip currentVoiceLine;

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
			currentVoiceLine = GetVoiceLine(line);
			if (currentVoiceLine != null)
			{
				StopVoice();
				return;
			}
			PlayVoice(name, voiceVolume, voice);
		}
		public void PlayVoice(string name, float volume, AudioSource source, AudioClip clip = null)
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
				currentVoiceLine = clip;
			audioSource.volume = volume;
			audioSource.Play();
		}

		public AudioClip GetVoiceLine(ScriptLine line)
		{
			string filePath = $"Voice/{line.ScriptDocument}/{line.Index}";
			AudioClip output = Resources.Load<AudioClip>(filePath);
			return output;
		}


		~VoiceActorHandler()
		{
			GameObject.Destroy(audioSource);
		}
	}
}