namespace B1NARY.CharacterManagement
{
	using System;
	using UnityEngine;
	using B1NARY.DesignPatterns;
	using B1NARY.Scripting;
	using B1NARY.Audio;
	using Live2D.Cubism.Framework.MouthMovement;
	using UnityEngine.Audio;
	using System.Xml.Linq;
	using System.IO;

	public class VoiceActorHandler : Multiton<VoiceActorHandler>, IAudioInfo
	{
		public static string GetResourceVoicePath(int index, ScriptHandler handler)
			=> $"Voice/{ScriptHandler.DocumentList.ToVisual(handler.document.ReadFile.FullName)}/{index}";
		public static AudioClip GetVoiceLine(int index, ScriptHandler handler)
		{
			string filePath = GetResourceVoicePath(index, handler);
			AudioClip clip = Resources.Load<AudioClip>(filePath);
			if (clip == null)
				throw new IOException($"Voiceline in resources path '{filePath}' " +
					"could not be retrieved.");
			return clip;
		}
		public static bool BlockPreviousSpeakersOnNextLine { get; set; } = true;

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
		public AudioMixerGroup CurrentGroup
		{
			get => audioSource.outputAudioMixerGroup;
			set => audioSource.outputAudioMixerGroup = value;
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
			currentVoiceLine = GetVoiceLine(line.Index, ScriptHandler.Instance);
			if (BlockPreviousSpeakersOnNextLine && CharacterManager.HasInstance)
				using (var enumerator = CharacterManager.Instance.CharactersInScene.Values.GetEnumerator())
					while (enumerator.MoveNext())
						enumerator.Current.controller.VoiceData.Stop();
			audioSource.clip = currentVoiceLine;
			audioSource.Play();
		}
	}
}