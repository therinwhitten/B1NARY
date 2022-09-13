namespace B1NARY.Audio
{
	using System;
	using UnityEngine;
	using B1NARY.Scripting.Experimental;
	using System.Collections.Generic;

	public class VoiceActorHandler
	{
		private AudioSource audioSource;
		private AudioClip currentVoiceLine;
		public VoiceActorHandler(GameObject parent)
		{
			audioSource = parent.AddComponent<AudioSource>();
		}
		public void Stop()
		{
			if (audioSource != null)
				audioSource.Stop();
		}
		public void Play(ScriptLine line)
		{
			string filePath = $"Voice/{line.ScriptDocument}/{line.Index}";
			currentVoiceLine = Resources.Load<AudioClip>(filePath);
			Stop();
			audioSource.clip = currentVoiceLine;
			audioSource.Play();
		}
	}
}