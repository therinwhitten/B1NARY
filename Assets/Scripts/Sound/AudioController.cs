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
	using UnityEngine.Audio;

	public sealed class AudioController : Singleton<AudioController>
	{
		
		public const string baseResourcesPath = "Sounds/Sound Libraries";
		
		public static readonly IEnumerable<KeyValuePair<string, Delegate>> Commands = new Dictionary<string, Delegate>()
		{
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
			m_activeAudioTrackers.Add((customAudioClip.Name, index), new AudioTracker(this));
			return (customAudioClip.Name, index);
		}

		private void Awake()
		{
			m_activeAudioTrackers = new Dictionary<(string name, int index), AudioTracker>();
			oneShotSounds = new Dictionary<string, (CoroutineWrapper disposeWrapper, AudioSource source)>();
			SceneManager.InstanceOrDefault.SwitchedScenes.AddPersistentListener(ReloadSoundLibrary);
		}

		private void LateUpdate()
		{
			using (IEnumerator<KeyValuePair<(string name, int index), AudioTracker>> enumerator = ActiveAudioTrackers.GetEnumerator())
				for (int i = 0; enumerator.MoveNext(); i++)
				{
					if (enumerator.Current.Value.IsPlaying)
						continue;
					enumerator.Current.Value.Dispose();
					m_activeAudioTrackers.Remove(enumerator.Current.Key);
					return;
				}
		}
		private void ReloadSoundLibrary()
		{
			string resourcesPath = baseResourcesPath + '/' + SceneManager.ActiveScene.name; 
			SoundLibrary newSoundLibrary = Resources.Load<SoundLibrary>(resourcesPath);
			if (newSoundLibrary == null)
				throw new FileNotFoundException($"{resourcesPath} is not a valid resource path of sound library!");
			CurrentSoundLibrary = newSoundLibrary;
			if (newSoundLibrary.ContainsPlayOnAwakeCommands)
			{
				using (IEnumerator<CustomAudioClip> enumerator = newSoundLibrary.PlayOnAwakeCommands.GetEnumerator())
					while (enumerator.MoveNext())
						AddAudioClipToDictionary(enumerator.Current);
			}
		}

		public void PlaySound(string soundName)
		{
			if (!CurrentSoundLibrary.TryGetCustomAudioClip(soundName, out var audioClip))
			{
				Debug.LogError($"Sound Name'{soundName}' is not found in '{CurrentSoundLibrary.name}'!");
				return;
			}
			PlaySound(audioClip);
		}
		public void PlaySound(CustomAudioClip audioClip)
		{
			var pairKey = AddAudioClipToDictionary(audioClip);
			ActiveAudioTrackers[pairKey].PlaySingle(audioClip);
		}
		public void PlaySound(string soundName, float fadeIn)
		{
			if (!CurrentSoundLibrary.TryGetCustomAudioClip(soundName, out var audioClip))
			{
				Debug.LogError($"Sound Name'{soundName}' is not found in '{CurrentSoundLibrary.name}'!");
				return;
			}
			PlaySound(audioClip, fadeIn);
		}
		public void PlaySound(CustomAudioClip audioClip, float fadeIn)
		{
			var pairKey = AddAudioClipToDictionary(audioClip);
			ActiveAudioTrackers[pairKey].PlaySingle(audioClip, fadeIn);
		}
		public void StopSound(string soundName)
		{
			if (!CurrentSoundLibrary.TryGetCustomAudioClip(soundName, out var exampleClip))
			{
				Debug.LogError($"Sound Name'{soundName}' is not found in '{CurrentSoundLibrary.name}'!");
				return;
			}
			IEnumerable<(string name, int index)> items = m_activeAudioTrackers.Keys.Where(pair => exampleClip.Name == pair.name);
			if (items.Count() == 1)
				m_activeAudioTrackers[items.Single()].Stop();
			Debug.Log($"Stopping multiple sounds with '{exampleClip.Name}'");
			Array.ForEach(items.ToArray(), pair => m_activeAudioTrackers[pair].Stop());
		}
		public void StopSound(string soundName, float fadeOut)
		{
			if (!CurrentSoundLibrary.TryGetCustomAudioClip(soundName, out var exampleClip))
			{
				Debug.LogError($"Sound Name'{soundName}' is not found in '{CurrentSoundLibrary.name}'!");
				return;
			}
			IEnumerable<(string name, int index)> items = m_activeAudioTrackers.Keys.Where(pair => exampleClip.Name == pair.name);
			if (items.Count() == 1)
				m_activeAudioTrackers[items.Single()].Stop(fadeOut);
			Debug.Log($"Stopping multiple sounds with '{exampleClip.Name}'");
			Array.ForEach(items.ToArray(), pair => m_activeAudioTrackers[pair].Stop(fadeOut));
		}

		private Dictionary<string, (CoroutineWrapper disposeWrapper, AudioSource source)> oneShotSounds;
		public void PlayOneShot(AudioClip clip, AudioMixerGroup audioMixerGroup, float volume = 1f)
		{
			if (oneShotSounds.TryGetValue(audioMixerGroup.name, out var pair))
			{
				pair.source.PlayOneShot(clip, volume);
				return;
			}
			AudioSource customAudioSource = gameObject.AddComponent<AudioSource>();
			customAudioSource.outputAudioMixerGroup = audioMixerGroup;
			customAudioSource.PlayOneShot(clip, volume);
			var coroutineWrapper = new CoroutineWrapper(this, WaitUntil(audioMixerGroup.name));
			coroutineWrapper.AfterActions += () => Destroy(oneShotSounds[audioMixerGroup.name].source);
			coroutineWrapper.AfterActions += oneShotSounds[audioMixerGroup.name].disposeWrapper.Dispose;
			oneShotSounds.Add(audioMixerGroup.name, (coroutineWrapper, customAudioSource));
			oneShotSounds[audioMixerGroup.name].disposeWrapper.Start();
		}
		private IEnumerator WaitUntil(string key)
		{
			yield return new WaitUntil(() => !oneShotSounds[key].source.isPlaying);
		}
	}
}