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
		public float Completion => audioSource != null ? (audioSource.time / audioSource.clip.length) : 0f;
		public float ClipLength => audioSource != null ? audioSource.clip.length : float.NaN;
		public float PlayedTime => audioSource != null ? audioSource.time : float.NaN;
		public bool IsPlaying => audioSource != null ? audioSource.isPlaying : false;
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