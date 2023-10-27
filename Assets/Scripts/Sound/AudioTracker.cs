namespace B1NARY.Audio
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using UnityEngine;
	using UnityEngine.Audio;

	public sealed class AudioTracker : IDisposable, IAudioInfo
	{
		public readonly AudioSource audioSource;

		[SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<Pending>")]
		public bool IsPlaying
		{
			get => audioSource?.isPlaying == true;
			set
			{
				if (IsPlaying == value)
					return;
				if (value)
					Start();
				else
					Stop();
			}
		}
		public CustomAudioClip CustomAudioClip { get; }
		public string ClipName => audioSource.clip.name;
		public float Volume
		{
			get => audioSource.volume;
			set => audioSource.volume = value;
		}
		public float Pitch
		{
			get => audioSource.pitch;
			set => audioSource.pitch = value;
		}
		public bool Loop
		{
			get => audioSource.loop;
			set => audioSource.loop = value;
		}
		public AudioMixerGroup CurrentGroup
		{
			get => audioSource.outputAudioMixerGroup;
			set => audioSource.outputAudioMixerGroup = value;
		}
		public TimeSpan PlayedSeconds
		{
			get => TimeSpan.FromSeconds(audioSource.time);
			set => audioSource.time = (float)value.TotalSeconds;
		}
		public TimeSpan TotalSeconds => TimeSpan.FromSeconds(audioSource.clip.length);
		public string TimeInfo => $"{audioSource.time:N2} : {audioSource.clip.length:N2}";
		public string SceneName { get; }
		public bool canAutoDispose = true;

		public AudioTracker(CustomAudioClip customAudioClip)
		{
			audioSource = AudioController.Instance.gameObject.AddComponent<AudioSource>();
			customAudioClip.ApplyTo(audioSource);
			CustomAudioClip = customAudioClip;
			SceneName = SceneManager.ActiveScene.name;
		}
		public AudioTracker(CustomAudioClip customAudioClip, AudioSource source)
		{
			audioSource = source;
			customAudioClip.ApplyTo(audioSource);
			CustomAudioClip = customAudioClip;
			SceneName = SceneManager.ActiveScene.name;
		}
		public AudioTracker Start(float fadeIn = 0f)
		{
			if (fadeIn < 0f)
				fadeIn = 0f;
			if (fadeIn > Time.fixedDeltaTime)
			{
				float desiredVolume = audioSource.volume;
				audioSource.volume = 0f;
				AudioController.Instance.fadeSounds.Add(() =>
				{
					float volume = audioSource.volume;
					volume += Time.fixedDeltaTime / fadeIn;
					if (volume >= desiredVolume)
					{
						audioSource.volume = desiredVolume;
						return false;
					}
					audioSource.volume = volume;
					return true;
				});
			}
			audioSource.Play();
			return this;
		}


		public void Stop(float fadeOut = 0f)
		{
			if (fadeOut < 0f)
				fadeOut = 0f;
			if (fadeOut > Time.fixedDeltaTime)
			{
				AudioController.Instance.fadeSounds.Add(() =>
				{
					float volume = audioSource.volume;
					volume -= Time.fixedDeltaTime / fadeOut;
					audioSource.volume = volume;
					return volume > 0f;
				});
				return;
			}
			audioSource.Stop();
		}
		public void Dispose()
		{
			Stop();
		}

		public override string ToString()
		{
			return $"AudioTracker ({ClipName})";
		}
	}
}