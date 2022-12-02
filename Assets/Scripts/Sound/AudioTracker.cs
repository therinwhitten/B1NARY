﻿namespace B1NARY.Audio
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Audio;

	public sealed class AudioTracker : IDisposable, IAudioInfo
	{
		//TODO: add custom audio data to regular audiosources!
		public static void ApplyCustomSoundData(AudioSource audioSource, CustomAudioClip clip)
		{
			audioSource.clip = clip.clip;
			audioSource.outputAudioMixerGroup = clip.audioMixerGroup;
			audioSource.loop = clip.loop;
			audioSource.volume = clip.FinalVolume;
			audioSource.pitch = clip.FinalPitch;
		}

		public event Action FinishedPlaying;
		public event Action Disposing;

		private readonly AudioSource audioSource;
		private readonly MonoBehaviour monoBehaviour;

		private CoroutineWrapper garbageCollector;
		
		public CustomAudioClip CustomAudioClip { get; private set; }
		public bool IsPlaying
		{
			get => audioSource.isPlaying;
			set
			{
				if (IsPlaying == value)
					return;
				if (value)
				{
					CreateAutoDisposableCoroutine = true;
					PlaySingle(CustomAudioClip);
					return;
				}
				Stop();
			}
		}
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

		public bool CreateAutoDisposableCoroutine { get; private set; }
		public TimeSpan PlayedSeconds => TimeSpan.FromSeconds(audioSource.time);
		public TimeSpan TotalSeconds => TimeSpan.FromSeconds(audioSource.clip.length);
		public float CompletionPercent => audioSource.time / audioSource.clip.length;
		public string TimeInfo => $"{audioSource.time:N2} : {audioSource.clip.length:N2}";
		public AudioTracker(MonoBehaviour monoBehaviour)
		{
			CreateAutoDisposableCoroutine = true;
			this.monoBehaviour = monoBehaviour;
			audioSource = monoBehaviour.gameObject.AddComponent<AudioSource>();
		}
		public AudioTracker(MonoBehaviour monoBehaviour, AudioSource audioSource)
		{
			CreateAutoDisposableCoroutine = true;
			this.monoBehaviour = monoBehaviour;
			this.audioSource = audioSource;
		}

		public void PlaySingle(CustomAudioClip audioClip)
		{
			if (!CoroutineWrapper.IsNotRunningOrNull(garbageCollector))
				throw new InvalidOperationException("It is already being played!");
			CustomAudioClip = audioClip;
			ApplyCustomSoundData(audioSource, audioClip);
			if (CreateAutoDisposableCoroutine)
				garbageCollector = new CoroutineWrapper(monoBehaviour, WaitUntil()).Start();
			audioSource.Play();
		}
		public void PlaySingle(CustomAudioClip audioClip, float fadeIn)
		{
			if (!CoroutineWrapper.IsNotRunningOrNull(garbageCollector))
				throw new InvalidOperationException("It is already being played!");
			CustomAudioClip = audioClip;
			ApplyCustomSoundData(audioSource, audioClip);
			if (CreateAutoDisposableCoroutine)
				garbageCollector = new CoroutineWrapper(monoBehaviour, WaitUntil()).Start();
			float finalVolume = audioSource.volume;
			audioSource.volume = 0f;
			monoBehaviour.ChangeFloat(new Ref<float>(() => audioSource.volume, 
				volume => audioSource.volume = volume), finalVolume, fadeIn);
			audioSource.Play();
		}
		
		public void PlayOneShot(AudioClip audioClip, float volume)
		{
			audioSource.PlayOneShot(audioClip, volume);
		}


		private void StopOneShots()
		{
			//audioSource.Stop();
		}
		public void Stop()
		{
			if (!CoroutineWrapper.IsNotRunningOrNull(garbageCollector))
				garbageCollector.Stop();
			StopOneShots();
		}
		public void Stop(float fadeOut)
		{
			Debug.Log("H");
			monoBehaviour.ChangeFloat(new Ref<float>(() => audioSource.volume,
				volume => audioSource.volume = volume), 0, fadeOut, () => { if (!CoroutineWrapper.IsNotRunningOrNull(garbageCollector)) garbageCollector.Stop(); });
			StopOneShots();
		}

		public void Dispose()
		{
			if (audioSource != null)
				GameObject.Destroy(audioSource);
			Disposing?.Invoke();
		}
		public IEnumerator WaitUntil()
		{
			yield return new WaitUntil(() => !audioSource.isPlaying);
			FinishedPlaying?.Invoke();
		}
	}
}