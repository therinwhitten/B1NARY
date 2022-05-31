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


	public readonly MonoBehaviour monoBehaviour;
	private AudioSource audioSource;
	public AudioSource AudioSource
	{
		get => audioSource;
		set
		{
			UnityEngine.Object.Destroy(audioSource);
			audioSource = value;
		}
	}
	private Coroutine garbageCollection = null;
	public bool destroyOnFinish = true;

	public SoundCoroutine(MonoBehaviour monoBehaviour, CustomAudioClip? clip = null)
	{
		this.monoBehaviour = monoBehaviour;
		audioSource = monoBehaviour.gameObject.AddComponent<AudioSource>();
		if (clip != null)
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
			garbageCollection = monoBehaviour.StartCoroutine(GarbageCollectionCoroutine());
	}

	public void PlaySingle(float fadeInSeconds)
	{
		float targetValue = audioSource.volume;
		audioSource.volume = 0;
		FadeController.Instance.ChangeFloat
			(
			new Ref<float>(() => audioSource.volume, (var) => audioSource.volume = var), 
			targetValue, 
			fadeInSeconds);
		audioSource.Play();
		if (garbageCollection == null)
			garbageCollection = monoBehaviour.StartCoroutine(GarbageCollectionCoroutine());
	}

	public void PlayOneShot()
	{
		audioSource.PlayOneShot(audioSource.clip, audioSource.volume);
		if (garbageCollection == null)
			garbageCollection = monoBehaviour.StartCoroutine(GarbageCollectionCoroutine());
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

	public void Stop(float fadeOutSeconds, bool destroy = true)
	{
		SingletonNew<FadeController>.Instance.ChangeFloat
			(
			new Ref<float>(() => audioSource.volume, 
				(var) => 
				{ 
					audioSource.volume = var;
					// Because of how dynamically changing the value works,
					// - im going to have to write the action in the setter
					if (audioSource.volume <= 0)
						Stop(destroy);
				}
			),
			0,
			fadeOutSeconds);
	}

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