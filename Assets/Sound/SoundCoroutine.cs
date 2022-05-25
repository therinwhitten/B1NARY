using System;
using Random = System.Random;
using System.Collections;
using UnityEngine;

public class SoundCoroutine
{
	// Dependencies
	public readonly AudioSource audioSource;
	public readonly AudioMaster audioMaster;

	// Data
	private Coroutine coroutine = null;
	public bool destroyOnFinish = false;
	private bool hasPlayed = false;
	public bool HasPlayed => hasPlayed;
	public event EventHandler GarbageCollection;

	public SoundCoroutine(AudioListener audioListener, AudioClip audioClip, AudioMaster audioMaster)
		: this(audioListener, (CustomAudioClip)audioClip, audioMaster) {}
	public SoundCoroutine
		(AudioListener audioListener, CustomAudioClip audioClip, AudioMaster audioMaster)
	{
		audioSource = audioListener.gameObject.AddComponent<AudioSource>();
		this.audioMaster = audioMaster;
		AudioClip = audioClip;
	}

	public CustomAudioClip AudioClip
	{
		get
		{
			var clip = (CustomAudioClip)audioSource.clip;
			clip.volume = audioSource.volume;
			clip.pitch = audioSource.pitch;
			return clip;
		}
		set
		{
			audioSource.clip = value;

			// This needs testing!
			var random = new Random();
			audioSource.pitch = (float)(value.pitch - (value.pitchVariance * 3) 
				+ (random.NextDouble() * value.pitchVariance * 3));
			audioSource.volume = (float)(value.volume - value.volumeVariance
				+ (random.NextDouble() * value.volumeVariance));
		}
	}

	public void PlaySingle()
	{
		if (hasPlayed && destroyOnFinish)
			throw new ArgumentNullException("Cannot call since it has " + 
			"already been called and destroyed!");
		hasPlayed = true;
		audioSource.Play();
		if (coroutine == null)
			coroutine = audioMaster.StartCoroutine(CloseCoroutineAwaiter());
	}
	public void PlayShot()
	{
		if (hasPlayed && destroyOnFinish)
			throw new ArgumentNullException("Cannot call since it has " + 
			"already been called and destroyed!");
		audioSource.PlayOneShot(audioSource.clip, audioSource.volume);
		if (coroutine == null)
			coroutine = audioMaster.StartCoroutine(CloseCoroutineAwaiter());
	}
	private IEnumerator CloseCoroutineAwaiter()
		{
			// The casts are here to tell the primitive C# compiler to force them
			// - not to be an anonymous delegates that generates a YieldInstruction
			float seconds = audioSource.clip.length > 5 ? 0.1f * audioSource.clip.length : 0;
			Func<YieldInstruction> classCreator = seconds != 0 ? 
				(Func<YieldInstruction>)(() => new WaitForSeconds(seconds)) :
				(Func<YieldInstruction>)(() => new WaitForEndOfFrame());
			while (audioSource.isPlaying)
				yield return classCreator.Invoke();
			Stop();
		}

	public void Stop()
	{
		audioMaster.StopCoroutine(coroutine);
		coroutine = null;
		if (destroyOnFinish)
			OnDestroy();
	}
	private void OnDestroy()
	{
		hasPlayed = true;
		destroyOnFinish = true; // just in case
		audioSource = null;
		audioMaster = null;
		GarbageCollection?.Invoke(this, EventArgs.Empty);
		UnityEngine.Object.Destroy(audioSource);
	}
	~SoundCoroutine() => OnDestroy();
}