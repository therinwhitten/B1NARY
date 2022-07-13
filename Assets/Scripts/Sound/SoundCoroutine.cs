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
	public bool DeleteCoroutineOnSwap { get; set; } = true;
	public bool IsPlaying { get
		{
			if (AudioSource != null)
				return AudioSource.isPlaying;
			return false;
		}
	}
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
			if (DeleteCoroutineOnSwap && audioSource != null)
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

	private float oldVolume, oldPitch;
	private (float min, float max) pitchVariance, volumeVariance;
	private bool playOnAwake, fadeWhenTransitioning;
	public CustomAudioClip AudioClip
	{
		get => new CustomAudioClip(audioSource.clip) 
		{
			minPitchVariance = pitchVariance.min,
			maxPitchVariance = pitchVariance.max,
			maxVolumeVariance = volumeVariance.max,
			minVolumeVariance = volumeVariance.min,
			clip = audioSource.clip,
			volume = oldVolume,
			pitch = oldPitch,
			audioMixerGroup = audioSource.outputAudioMixerGroup,
			loop = audioSource.loop,
			playOnAwake = playOnAwake,
			fadeWhenTransitioning = fadeWhenTransitioning,
		};
		set
		{
			audioSource.clip = value;
			volumeVariance = (value.minVolumeVariance, value.maxVolumeVariance);
			pitchVariance = (value.minPitchVariance, value.maxPitchVariance);
			if (value.minPitchVariance != value.maxPitchVariance)
				audioSource.pitch = RandomFowarder.NextRange(value.pitch,
					value.minPitchVariance, value.maxPitchVariance, RandomFowarder.RandomType.CSharp);
			else
				audioSource.pitch = value.pitch;
			oldVolume = value.volume;
			if (value.minVolumeVariance != value.maxVolumeVariance)
				audioSource.volume = RandomFowarder.NextRange(value.volume, 
					value.minVolumeVariance, value.maxVolumeVariance, RandomFowarder.RandomType.CSharp);
			else
				audioSource.volume = value.volume;
			oldPitch = value.pitch;
			audioSource.outputAudioMixerGroup = value.audioMixerGroup;
			audioSource.loop = value.loop;
			playOnAwake = value.playOnAwake;
			fadeWhenTransitioning = value.fadeWhenTransitioning;
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
		if (garbageCollection != null)
			monoBehaviour.StopCoroutine(garbageCollection);
		if (destroyOnFinish && destroy)
			OnDestroy();
	}
	public event Action CalledToStop;

	private void SwitchSceneCheck(object sender, string sceneName)
	{
		if (DeleteCoroutineOnSwap)
			return;
		if (AudioSource == null)
			audioSource = monoBehaviour.gameObject.AddComponent<AudioSource>();
		if (AudioClip.fadeWhenTransitioning)
			Stop();
		GameCommands.SwitchedScenes += SwitchSceneCheck;
	}

	private bool isDestroyed = false;
	private void OnDestroy()
	{
		Stop(false);
		if (isDestroyed)
			return;
		GarbageCollection?.Invoke(this, audioSource.clip);
	}
	public event EventHandler<AudioClip> GarbageCollection;

	~SoundCoroutine()
	{
		OnDestroy();
		GameCommands.SwitchedScenes -= SwitchSceneCheck;
	}
}