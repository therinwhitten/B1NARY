using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
///		An extended <see cref="UnityEngine.AudioSource"/> for easily manipulating
///		sounds and contains Garbage Collection and Events when finished playing.
/// </summary>
public class SoundCoroutine
{
	public bool IsFadingAway { get; private set; } = false;
	public bool DeleteAudioSourceOnSwap { get; set; } = true;
	public bool IsPlaying { get
		{
			if (AudioSource != null)
				return AudioSource.isPlaying;
			return false;
		}
	}
	public bool IsStopping { get; private set; } = false;

	public MonoBehaviour monoBehaviour;
	public AudioMixerGroup AudioMixerGroup { get; private set; }
	private AudioSource audioSource;
	public AudioSource AudioSource
	{
		get => audioSource;
		set
		{
			if (audioSource == value)
				return;
			if (DeleteAudioSourceOnSwap && audioSource != null)
				UnityEngine.Object.Destroy(audioSource);
			audioSource = value;
			if (audioSource == null)
			{
				Debug.LogWarning($"{nameof(SoundCoroutine)} assigned to an empty" +
					" audioSource");
				return;
			}
			audioSource.outputAudioMixerGroup = AudioMixerGroup;
		}
	}
	private Coroutine garbageCollection = null;
	public bool destroyOnFinish = true;
	public readonly string currentSoundLibrary;



	public SoundCoroutine(MonoBehaviour monoBehaviour, string soundLibrary, 
		AudioMixerGroup mixerGroup = null, CustomAudioClip clip = null)
	{
		this.monoBehaviour = monoBehaviour;
		audioSource = monoBehaviour.gameObject.AddComponent<AudioSource>();
		audioSource.outputAudioMixerGroup = AudioMixerGroup;
		AudioMixerGroup = mixerGroup;
		audioSource.outputAudioMixerGroup = mixerGroup;
		currentSoundLibrary = soundLibrary;
		if (clip != null)
			AudioClip = clip;
		GameCommands.SwitchedScenes += SwitchSceneCheck;
	}

	private CustomAudioClip _audioClipCache;
	public CustomAudioClip AudioClip
	{
		get => _audioClipCache;
		set
		{
			_audioClipCache = value;

			audioSource.clip = value;
			audioSource.volume = value.ApplyVolumeRandomization();
			audioSource.pitch = value.ApplyPitchRandomization();
			audioSource.outputAudioMixerGroup = value.audioMixerGroup;
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
		AudioHandler.Instance.ChangeFloat
			(
			new Ref<float>(() => audioSource.volume, set => audioSource.volume = set), 
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
		IsStopping = false;
		Stop();
	}
	public event Action Finished;

	public void Stop(float fadeOutSeconds, bool destroy = true)
	{

		CalledToStop?.Invoke();
		if (monoBehaviour == null)
		{
			monoBehaviour = UnityEngine.Object.FindObjectOfType<MonoBehaviour>();
			Debug.LogError($"{nameof(SoundCoroutine)} does" +
				$" not have an availible {nameof(MonoBehaviour)}!");
		}
		IsStopping = true;
		IsFadingAway = true;
		monoBehaviour.ChangeFloat(
			new Ref<float>(() => audioSource.volume, 
				(@float) => 
				{ 
					audioSource.volume = @float;
					// Because of how dynamically changing the value works,
					// - im going to have to write the action in the setter
					if (audioSource.volume <= 0)
					{
						IsFadingAway = false;
						Stop(destroy);
					}
				}
			),
			0,
			fadeOutSeconds);
	}

	public void Stop(bool destroy = true)
	{
		CalledToStop?.Invoke();
		Finished?.Invoke();
		audioSource.Stop();
		IsStopping = false;
		if (garbageCollection != null)
			monoBehaviour.StopCoroutine(garbageCollection);
		if (destroyOnFinish && destroy)
			OnDestroy();
	}
	public event Action CalledToStop;

	private void SwitchSceneCheck(string sceneName)
	{
		if (AudioSource == null)
			audioSource = monoBehaviour.gameObject.AddComponent<AudioSource>();
		if (AudioClip != null)
			if (AudioClip.destroyWhenTransitioningScenes)
				if (AudioClip.fadeTime != 0)
					Stop(AudioClip.fadeTime);
				else
					Stop();
		GameCommands.SwitchedScenes += SwitchSceneCheck;
	}

	private bool isDestroyed = false;
	private void OnDestroy()
	{
		Stop(false);
		if (isDestroyed)
			return;
		GarbageCollection?.Invoke();
	}
	public Action GarbageCollection;

	~SoundCoroutine()
	{
		OnDestroy();
		if (AudioSource != null)
			UnityEngine.Object.Destroy(AudioSource);
		GameCommands.SwitchedScenes -= SwitchSceneCheck;
	}
}