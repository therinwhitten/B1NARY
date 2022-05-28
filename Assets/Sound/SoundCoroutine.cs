using System;
using Random = UnityEngine.Random;
using System.Collections;
using UnityEngine;

public class SoundCoroutine
{
	public static float ApplyRandomValue(float input, float varianceMult)
	{
		if (varianceMult == 0)
			return input;
		float baseValue = input * varianceMult,
					subValue = input - baseValue;
		return baseValue + (Random.value * subValue);
	}


	public readonly AudioHandler audioMaster;
	public readonly AudioSource audioSource;
	private Coroutine garbageCollection = null;
	public bool destroyOnFinish = true;

	public SoundCoroutine(AudioHandler audioMaster, CustomAudioClip clip)
	{
		this.audioMaster = audioMaster;
		audioSource = audioMaster.gameObject.AddComponent<AudioSource>();
		AudioClip = clip;
	}

	public CustomAudioClip AudioClip
	{
		get => new CustomAudioClip(audioSource.clip) 
		{
			volume = audioSource.volume,
			pitch = audioSource.pitch,
			loop = audioSource.loop 
		};
		set
		{
			// customizability with this seems weird
			audioSource.clip = value;
			audioSource.pitch = ApplyRandomValue(value.pitch, value.pitchVariance);
			audioSource.volume = ApplyRandomValue(value.volume, value.volumeVariance);
			audioSource.loop = value.loop;
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
		Func<YieldInstruction> yield = audioSource.clip.length > 5 ? 
			(Func<YieldInstruction>)
				(() => new WaitForSeconds(0.1f * audioSource.clip.length))
			: (Func<YieldInstruction>)(() => new WaitForEndOfFrame());
		while (audioSource.isPlaying)
			yield return yield();
		FinishCoroutine();
	}
	private void FinishCoroutine()
	{
		Finished?.Invoke(this, EventArgs.Empty);
		if (destroyOnFinish)
			Stop();
	}
	public event EventHandler Finished;


	public void Stop(bool destroy = true)
	{
		audioSource.Stop();
		if (destroyOnFinish && destroy)
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