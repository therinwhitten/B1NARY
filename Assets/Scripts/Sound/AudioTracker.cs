namespace B1NARY.Audio
{
	using System;
	using System.Collections;
	using UnityEngine;

	public sealed class AudioTracker : IDisposable
	{
		public event Action FinishedPlaying;
		public event Action Disposing;

		private readonly AudioSource audioSource;
		private readonly MonoBehaviour monoBehaviour;

		private CoroutineWrapper garbageCollector;
		
		public CustomAudioClip CustomAudioClip { get; private set; }
		public bool IsPlaying => audioSource.isPlaying;
		public string ClipName => audioSource.clip.name;
		public float Volume => audioSource.volume;
		public float Pitch => audioSource.pitch;
		public bool Loop => audioSource.loop;
		public TimeSpan PlayedSeconds => TimeSpan.FromSeconds(audioSource.time);
		public TimeSpan TotalSeconds => TimeSpan.FromSeconds(audioSource.clip.length);
		public float CompletionPercent => audioSource.time / audioSource.clip.length;

		public AudioTracker(MonoBehaviour monoBehaviour)
		{
			this.monoBehaviour = monoBehaviour;
			audioSource = monoBehaviour.gameObject.AddComponent<AudioSource>();
		}
		public AudioTracker(MonoBehaviour monoBehaviour, AudioSource audioSource)
		{
			this.monoBehaviour = monoBehaviour;
			this.audioSource = audioSource;
		}

		public void PlaySingle(CustomAudioClip audioClip)
		{
			if (CoroutineWrapper.IsNotRunningOrNull(garbageCollector))
				throw new InvalidOperationException("It is already being played!");
			CustomAudioClip = audioClip;
			audioSource.clip = audioClip;
			audioSource.outputAudioMixerGroup = audioClip.audioMixerGroup;
			audioSource.loop = audioClip.loop;
			audioSource.volume = audioClip.FinalVolume;
			audioSource.pitch = audioClip.FinalPitch;
			garbageCollector = new CoroutineWrapper(monoBehaviour, WaitUntil()).Start();
		}
		public void PlaySingle(CustomAudioClip audioClip, float fadeIn)
		{
			PlaySingle(audioClip);
			float finalVolume = audioSource.volume;
			audioSource.volume = 0f;
			monoBehaviour.ChangeFloat(new Ref<float>(() => audioSource.volume, 
				volume => audioSource.volume = volume), finalVolume, fadeIn);
		}
		
		public void PlayOneShot(AudioClip audioClip, float volume)
		{
			audioSource.PlayOneShot(audioClip, volume);
		}


		private void StopOneShots()
		{
			throw new NotImplementedException();
		}
		public void Stop()
		{
			if (!CoroutineWrapper.IsNotRunningOrNull(garbageCollector))
				throw new InvalidOperationException("It does not have the single sound playing!");
			garbageCollector.Dispose();
			StopOneShots();
		}
		public void Stop(float fadeOut)
		{
			if (!CoroutineWrapper.IsNotRunningOrNull(garbageCollector))
				throw new InvalidOperationException("It does not have the single sound playing!");
			monoBehaviour.ChangeFloat(new Ref<float>(() => audioSource.volume,
				volume => audioSource.volume = volume), 0, fadeOut, () => garbageCollector.Dispose());
			StopOneShots();
		}

		public void Dispose()
		{
			if (audioSource != null)
				GameObject.Destroy(audioSource);
			Disposing?.Invoke();
		}
		private IEnumerator WaitUntil()
		{
			yield return new WaitUntil(() => !garbageCollector.IsRunning);
			FinishedPlaying?.Invoke();
		}
	}
}