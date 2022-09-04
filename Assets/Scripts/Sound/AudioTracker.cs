namespace B1NARY.Audio
{
	using System;
	using System.Collections;
	using UnityEngine;
	using UnityEngine.Audio;

	/// <summary>
	///		An extended <see cref="UnityEngine.AudioSource"/> for easily manipulating
	///		sounds and contains Garbage Collection and Events when finished playing.
	/// </summary>
	public class AudioTracker
	{
		public bool IsFadingAway { get; private set; } = false;
		public bool DeleteAudioSourceOnSwap { get; set; } = true;
		public bool IsPlaying
		{
			get
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
					Debug.LogWarning(nameof(AudioTracker) + 
						$" assigned to an empty audioSource");
					return;
				}
				audioSource.outputAudioMixerGroup = AudioMixerGroup;
			}
		}
		private Coroutine garbageCollection = null;
		public bool destroyOnFinish = true;
		public readonly string currentSoundLibrary;

		private Ref<float> GetVolumeRef(Func<float> get = null, Action<float> set = null)
			=> new Ref<float>(get == null ? (() => audioSource != null ? audioSource.volume : float.NaN) : get,
				set == null ? ((setVar) => audioSource.volume = setVar) : set);



		public AudioTracker(MonoBehaviour monoBehaviour, string soundLibrary,
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
			SceneManager.SwitchedScenes.AddPersistentListener(SwitchSceneCheck);
		}

		private CustomAudioClip _audioClipCache;
		public CustomAudioClip AudioClip
		{
			get => _audioClipCache;
			set
			{
				_audioClipCache = value;

				audioSource.clip = value;
				audioSource.volume = value.FinalVolume;
				audioSource.pitch = value.FinalPitch;
				audioSource.outputAudioMixerGroup = value.audioMixerGroup;
				audioSource.loop = value.loop;

				// This should be handled by other outside sources like AudioHandler
				audioSource.playOnAwake = value.playOnAwake;
			}
		}



		public void PlaySingle()
		{
			if (audioSource == null)
			{
				Debug.LogError(nameof(AudioTracker) + 
					": Cannot play sounds because there is no AudioSource to play on!");
				return;
			}
			if (IsPlaying)
			{
				Debug.LogError(nameof(AudioTracker) + $": Only one instance of {AudioClip.Name} can be " +
					$"played one at a time, or use '{nameof(PlayOneShot)}' method");
				return;
			}
			audioSource.Play();
			if (garbageCollection == null)
				garbageCollection = monoBehaviour.StartCoroutine(GarbageCollectionCoroutine());
		}

		public void PlaySingle(float fadeInSeconds)
		{
			if (audioSource == null)
			{
				Debug.LogError(nameof(AudioTracker) +
					": Cannot play sounds because there is no AudioSource to play on!");
				return;
			}
			if (IsPlaying)
			{
				Debug.LogError(nameof(AudioTracker) + $": Only one instance of {AudioClip.Name} can be " +
					$"played one at a time, or use '{nameof(PlayOneShot)}' method");
				return;
			}
			float targetValue = audioSource.volume;
			audioSource.volume = 0;
			_ = monoBehaviour.ChangeFloatAsync
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
			if (audioSource == null)
			{
				Debug.LogError(nameof(AudioTracker) +
					": Cannot play sounds because there is no AudioSource to play on!");
				return;
			}
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
				Debug.LogError(nameof(AudioTracker) + $" does" +
					$" not have an availible {nameof(MonoBehaviour)}!");
			}
			IsStopping = true;
			IsFadingAway = true;
			_ = monoBehaviour.ChangeFloatAsync(
				GetVolumeRef(set: (@float) =>
				{
					// Because of how dynamically changing the value works,
					// - im going to have to write the action in the setter
					if (audioSource == null || audioSource.volume <= 0)
					{
						IsFadingAway = false;
						Stop(destroy);
					}
					else
						audioSource.volume = @float;
				}
				),
				0,
				fadeOutSeconds);
		}

		public void Stop(bool destroy = true)
		{
			CalledToStop?.Invoke();
			Finished?.Invoke();
			if (audioSource != null)
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
			if (AudioClip != null)
			{
				if (AudioSource == null)
					audioSource = monoBehaviour.gameObject.AddComponent<AudioSource>();
				if (AudioClip.destroyWhenTransitioningScenes)
					if (AudioClip.fadeTime != 0)
						Stop(AudioClip.fadeTime);
					else
						Stop();
			}
			else
				Debug.LogError(nameof(AudioTracker) + $": No availible {nameof(AudioClip)}" +
					" and won't be terminated, please tell a dev!");
		}

		private bool isDestroyed = false;
		private void OnDestroy()
		{
			if (IsPlaying)
				Stop(false);
			if (isDestroyed)
				return;
			GarbageCollection?.Invoke();
		}
		public Action GarbageCollection;

		~AudioTracker()
		{
			OnDestroy();
			if (AudioSource != null)
				UnityEngine.Object.Destroy(AudioSource);
			SceneManager.SwitchedScenes.RemoveListener(SwitchSceneCheck);
		}
	}
}