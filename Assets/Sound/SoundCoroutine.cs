using System;
using Random = System.Random;
using System.Collections;
using UnityEngine;

public class SoundCoroutine
{
	public readonly AudioMaster audioMaster;
	public readonly AudioSource audioSource;
	private Coroutine garbageCollection = null;
	public bool destroyOnFinish = true;

	public SoundCoroutine(AudioMaster audioMaster, CustomAudioClip clip)
	{
		this.audioMaster = audioMaster;
		audioSource = audioMaster.gameObject.AddComponent<AudioSource>();
		AudioClip = clip;
	}

	public CustomAudioClip AudioClip
	{
		get => new CustomAudioClip(audioSource.clip);
		set
		{
			audioSource.clip = value;
			var random = new Random();
			audioSource.pitch = (float)(value.pitch - (value.pitchVariance * 3)
			+ (random.NextDouble() * value.pitchVariance * 3));
			audioSource.volume = (float)(value.volume - value.volumeVariance
				+ (random.NextDouble() * value.volumeVariance));
		}
	}


	public void PlaySingle()
	{
		audioSource.Play();
		if (garbageCollection == null)
			garbageCollection = audioMaster.StartCoroutine(GarbageCollectionCoroutine());
	}
	public void PlayOneShot()
	{
		audioSource.PlayOneShot(audioSource.clip, audioSource.volume);
		if (garbageCollection == null)
			garbageCollection = audioMaster.StartCoroutine(GarbageCollectionCoroutine());
	}

	private IEnumerator GarbageCollectionCoroutine()
	{
		while (audioSource.isPlaying)
			yield return new WaitForFixedUpdate();
		FinishCoroutine();
	}
	private void FinishCoroutine()
	{
		Finished?.Invoke(this, EventArgs.Empty);
		if (destroyOnFinish)
			Stop();
	}
	public event EventHandler Finished;


	public void Stop()
	{
		audioSource.Stop();
		if (destroyOnFinish)
			OnDestroy();
	}

	private bool isDestroyed = false;
	private void OnDestroy()
	{
		audioSource.Stop();
		if (isDestroyed)
			return;
		GarbageCollection?.Invoke(this, audioSource.clip);
	}
	public event EventHandler<AudioClip> GarbageCollection;

	~SoundCoroutine() => OnDestroy();
}