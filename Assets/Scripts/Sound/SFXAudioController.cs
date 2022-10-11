namespace B1NARY.Audio
{
	using B1NARY.DesignPatterns;
	using System;
	using System.IO;
	using System.Collections.Generic;
	using UnityEngine;
	using System.Collections;
	using B1NARY.Scripting.Experimental;

	public sealed class SFXAudioController : Singleton<SFXAudioController>
	{
		public const string baseResourcesPath = "Sounds/Sound Libraries";
		
		public static readonly IEnumerable<KeyValuePair<string, Delegate>> Commands = new Dictionary<string, Delegate>()
		{
			// Although reflection is computationally heavy normally, it doesn't
			// - seem so in this situation.
			["fadeinsound"] = (Action<string, string>)((name, floatStr) =>
			{
				name = name.Trim();
				float fadeIn = float.Parse(floatStr);
				try { Instance.PlaySound(name, fadeIn); }
				catch (SoundNotFoundException ex)
				{
					Debug.LogWarning($"{name} is not a valid soundfile Path!\n" + ex, Instance);
				}
			}),
			["fadeoutsound"] = (Action<string, string>)((name, floatStr) =>
			{
				name = name.Trim();
				float fadeOut = float.Parse(floatStr);
				try { Instance.StopSound(name, fadeOut); }
				catch (SoundNotFoundException ex)
				{
					Debug.LogWarning($"{name} is not a valid soundfile Path!\n" + ex, Instance);
				}
				catch (KeyNotFoundException ex)
				{
					Debug.LogWarning($": Cannot find sound to close: {name}\n" + ex, Instance);
				}
			}),
			["playsound"] = (Action<string>)((name) =>
			{
				name = name.Trim();
				try
				{
					Instance.PlaySound(name);
				}
				catch (SoundNotFoundException ex)
				{
					Debug.LogWarning($"{name} is not a valid soundfile Path!\n" + ex, Instance);
				}
				catch (KeyNotFoundException ex)
				{
					Debug.LogWarning($": Cannot find sound to play: {name}\n" + ex, Instance);
				}
			}),
			["stopsound"] = (Action<string>)((name) =>
			{
				name = name.Trim();
				try { Instance.StopSound(name); }
				catch (SoundNotFoundException ex)
				{
					Debug.LogWarning($"{name} is not a valid soundfile Path!\n" + ex, Instance);
				}
				catch (KeyNotFoundException ex)
				{
					Debug.LogWarning($": Cannot find sound to play: {name}\n" + ex, Instance);
				}
			}),
		};

		public SoundLibrary CurrentSoundLibrary { get; private set; }

		public IReadOnlyDictionary<AudioClip, AudioTracker> ActiveAudioTrackers => m_activeAudioTrackers;

		private Dictionary<AudioClip, AudioTracker> m_activeAudioTrackers;
		private void AddAudioClipToDictionary(CustomAudioClip customAudioClip)
		{
			m_activeAudioTrackers.Add(customAudioClip.clip, new AudioTracker(this));
			m_activeAudioTrackers[customAudioClip.clip].FinishedPlaying += () => m_activeAudioTrackers.Remove(customAudioClip.clip);
		}

		private void Awake()
		{
			m_activeAudioTrackers = new Dictionary<AudioClip, AudioTracker>();
			SceneManager.SwitchedScenes.AddPersistentListener(PerScene);
			PerScene();
		}
		private void PerScene()
		{
			LoadSoundLibrary(baseResourcesPath + '/' + SceneManager.ActiveScene.name);
		}

		public void LoadSoundLibrary(string resourcesPath)
		{
			SoundLibrary newSoundLibrary = Resources.Load<SoundLibrary>(resourcesPath);
			if (newSoundLibrary == null)
				throw new FileNotFoundException($"{resourcesPath} is not a valid resource path of sound library!");
			CurrentSoundLibrary = newSoundLibrary;
			if (newSoundLibrary.ContainsPlayOnAwakeCommands)
			{
				var enumerator = newSoundLibrary.PlayOnAwakeCommands.GetEnumerator();
				while (enumerator.MoveNext())
				{
					AddAudioClipToDictionary(enumerator.Current);
					m_activeAudioTrackers[enumerator.Current].PlaySingle(enumerator.Current);
				}
			}
		}

		public void PlaySound(string soundName)
		{
			CustomAudioClip audioClip = CurrentSoundLibrary.GetCustomAudioClip(CurrentSoundLibrary.GetAudioClip(soundName));
			AddAudioClipToDictionary(audioClip);
			m_activeAudioTrackers[audioClip].PlaySingle(audioClip);
		}
		public void PlaySound(string soundName, float fadeIn)
		{
			CustomAudioClip audioClip = CurrentSoundLibrary.GetCustomAudioClip(CurrentSoundLibrary.GetAudioClip(soundName));
			AddAudioClipToDictionary(audioClip);
			m_activeAudioTrackers[audioClip].PlaySingle(audioClip, fadeIn);
		}
		public void StopSound(string soundName)
		{
			m_activeAudioTrackers[CurrentSoundLibrary.GetAudioClip(soundName)].Stop();
		}
		public void StopSound(string soundName, float fadeOut)
		{
			m_activeAudioTrackers[CurrentSoundLibrary.GetAudioClip(soundName)].Stop(fadeOut);
		}

		private AudioSource oneShotAudioSource;
		CoroutineWrapper oneShotSounds;
		public void PlayOneShot(AudioClip audioClip, float volume)
		{

			if (CoroutineWrapper.IsNotRunningOrNull(oneShotSounds))
			{
				oneShotAudioSource = gameObject.AddComponent<AudioSource>();
				oneShotAudioSource.PlayOneShot(audioClip, volume);
				oneShotSounds = new CoroutineWrapper(this, WaitUntil());
				oneShotSounds.AfterActions += () => oneShotAudioSource = null;
				oneShotSounds.Start();
			}
			else
				oneShotAudioSource.PlayOneShot(audioClip, volume);
			IEnumerator WaitUntil()
			{
				yield return new WaitUntil(() => !oneShotAudioSource.isPlaying);
			}
		}
	}
}