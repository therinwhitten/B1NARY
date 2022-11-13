namespace B1NARY.Audio
{
	using System;
	using UnityEngine;
	using B1NARY.DesignPatterns;
	using B1NARY.Scripting;
	using System.Collections.Generic;
	using Live2D.Cubism.Framework.MouthMovement;

	public class VoiceActorHandler : Multiton<VoiceActorHandler>, IAudioInfo
	{
		private AudioSource audioSource;
		private AudioClip currentVoiceLine;
		public bool IsPlaying
		{
			get => audioSource != null ? audioSource.isPlaying : false;
			set 
			{
				if (audioSource != null && audioSource.isPlaying != value)
					if (value)
						audioSource.Play();
					else
						audioSource.Stop();
			}
		}

		public string ClipName => audioSource != null ? audioSource.clip.name : string.Empty;
		public float Volume 
		{ 
			get => audioSource != null ? audioSource.volume : float.NaN;
			set { if (audioSource != null) audioSource.volume = value; }
		}
		float IAudioInfo.Pitch 
		{ 
			get => audioSource != null ? audioSource.pitch : float.NaN;
			set { if (audioSource != null) audioSource.pitch = value; } 
		}
		bool IAudioInfo.Loop
		{
			get => audioSource != null ? audioSource.loop : false;
			set { if (audioSource != null) audioSource.loop = value; }
		}
		public TimeSpan PlayedSeconds => audioSource != null ? TimeSpan.FromSeconds(audioSource.time) : TimeSpan.Zero;
		public TimeSpan TotalSeconds => audioSource != null ? TimeSpan.FromSeconds(audioSource.clip.length) : TimeSpan.Zero;

		private void Awake()
		{
			audioSource = GetSource();

			AudioSource GetSource()
			{
				if (gameObject.TryGetComponent<AudioSource>(out var audioSource))
					return audioSource;
				AudioSource output;
				if (gameObject.TryGetComponent<CubismAudioMouthInput>(out var cubismAudioMouthInput) && cubismAudioMouthInput.AudioInput != null)
					return cubismAudioMouthInput.AudioInput;
				output = gameObject.AddComponent<AudioSource>();
				if (cubismAudioMouthInput != null)
					cubismAudioMouthInput.AudioInput = output;
				return output;
			}
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
			using (IEnumerator<VoiceActorHandler> enumerator = GetEnumerator())
				while (enumerator.MoveNext())
					enumerator.Current.Stop();
			audioSource.clip = currentVoiceLine;
			audioSource.Play();
		}
	}
}