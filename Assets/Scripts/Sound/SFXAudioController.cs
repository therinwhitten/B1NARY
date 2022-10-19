namespace B1NARY.Audio
{
	using B1NARY.DesignPatterns;
	using System;
	using System.IO;
	using System.Collections.Generic;
	using UnityEngine;
	using System.Collections;
	using B1NARY.Scripting.Experimental;
	using System.Linq;

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
		/// <summary>
		/// Determines where it would convert a simple string name into an actual
		/// <see cref="CustomAudioClip"/>
		/// </summary>
		public SoundLibrary CurrentSoundLibrary { get; private set; }

		public IReadOnlyDictionary<(string name, int index), AudioTracker> ActiveAudioTrackers => m_activeAudioTrackers;

		private Dictionary<(string name, int index), AudioTracker> m_activeAudioTrackers;
		private (string name, int index) AddAudioClipToDictionary(CustomAudioClip customAudioClip)
		{
			int index = 0;
			while (m_activeAudioTrackers.ContainsKey((customAudioClip.Name, index)))
				index++;
			m_activeAudioTrackers.Add((customAudioClip.Name, index), new AudioTracker());
			return (customAudioClip.Name, index);
		}

		private void Awake()
		{
			m_activeAudioTrackers = new Dictionary<(string name, int index), AudioTracker>();
			disposableCoroutines = new Dictionary<(string name, int index), IEnumerator>();
			SceneManager.InstanceOrDefault.SwitchedScenes.AddPersistentListener(ReloadSoundLibrary);
		}

		private Dictionary<(string name, int index), IEnumerator> disposableCoroutines;
		private void Update()
		{
			KeyValuePair<(string name, int index), IEnumerator>[] coroutines =
				disposableCoroutines.ToArray();
			for (int i = 0; i < coroutines.Length; i++)
			{
				if (coroutines[i].Value.MoveNext())
					continue;
				disposableCoroutines.Remove(coroutines[i].Key);
				if (m_activeAudioTrackers.TryGetValue(coroutines[i].Key, out var tracker))
				{
					tracker.Dispose();
					m_activeAudioTrackers.Remove(coroutines[i].Key);
				}
			}
		}
		
		private void ReloadSoundLibrary()
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
					var pairKey = AddAudioClipToDictionary(enumerator.Current);
					disposableCoroutines.Add(pairKey, m_activeAudioTrackers[pairKey].PlaySingle(enumerator.Current));
				}
			}
		}

		public void PlaySound(string soundName)
		{
			CustomAudioClip audioClip = CurrentSoundLibrary.GetCustomAudioClip(CurrentSoundLibrary.GetAudioClip(soundName));
			var pairKey = AddAudioClipToDictionary(audioClip);
			disposableCoroutines.Add(pairKey, m_activeAudioTrackers[pairKey].PlaySingle(audioClip));
		}
		public void PlaySound(string soundName, float fadeIn)
		{
			CustomAudioClip audioClip = CurrentSoundLibrary.GetCustomAudioClip(CurrentSoundLibrary.GetAudioClip(soundName));
			var pairKey = AddAudioClipToDictionary(audioClip);
			disposableCoroutines.Add(pairKey, m_activeAudioTrackers[pairKey].PlaySingle(audioClip, fadeIn));
		}
		public void StopSound(string soundName)
		{
			var exampleClip = new CustomAudioClip(CurrentSoundLibrary.GetAudioClip(soundName));
			IEnumerable<(string name, int index)> items = m_activeAudioTrackers.Keys.Where(pair => exampleClip.Name == pair.name);
			if (items.Count() == 1)
				m_activeAudioTrackers[items.Single()].Stop();
			Debug.Log($"Stopping multiple sounds with '{exampleClip.Name}'");
			Array.ForEach(items.ToArray(), pair => m_activeAudioTrackers[pair].Stop());
		}
		public void StopSound(string soundName, float fadeOut)
		{
			var exampleClip = new CustomAudioClip(CurrentSoundLibrary.GetAudioClip(soundName));
			IEnumerable<(string name, int index)> items = m_activeAudioTrackers.Keys.Where(pair => exampleClip.Name == pair.name);
			if (items.Count() == 1)
				m_activeAudioTrackers[items.Single()].Stop(fadeOut);
			Debug.Log($"Stopping multiple sounds with '{exampleClip.Name}'");
			Array.ForEach(items.ToArray(), pair => m_activeAudioTrackers[pair].Stop(fadeOut));
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