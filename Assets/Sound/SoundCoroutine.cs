using System;
using Random = System.Random;
using System.Collections;
using UnityEngine;

public class SoundCoroutine
{
	public readonly AudioSource audioSource;
	public readonly AudioMaster audioMaster;
	private Coroutine coroutine = null;
	public bool destroyOnFinish = false, hasPlayed = false;

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
		if (hasPlayed)
			throw new Exception();
		hasPlayed = true;
		audioSource.Play();
		if (coroutine == null)
			coroutine = audioMaster.StartCoroutine(CloseCoroutineAwaiter());
	}
	public void PlayShot()
	{
		if (hasPlayed)
			throw new Exception();
		audioSource.PlayOneShot(audioSource.clip, audioSource.volume);
		if (coroutine == null)
			coroutine = audioMaster.StartCoroutine(CloseCoroutineAwaiter());
	}
	private IEnumerator CloseCoroutineAwaiter()
		{
			// The casts are here to tell the primitive C# compiler to force them
			// - not to be an anonymous delegates that generates a YieldInstruction
			Func<YieldInstruction> classCreator = audioSource.clip.length > 5 ? 
				(Func<YieldInstruction>)(() => new WaitForSeconds(0.1f * audioSource.clip.length)) :
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
		UnityEngine.Object.Destroy(audioSource);
	}
	~SoundCoroutine() => OnDestroy();
}