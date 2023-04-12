namespace B1NARY.Audio
{
	using B1NARY.DesignPatterns;
	using System;
	using System.IO;
	using System.Collections.Generic;
	using UnityEngine;
	using System.Collections;
	using B1NARY.Scripting;
	using System.Linq;
	using UnityEngine.Audio;

	public sealed class AudioController : Singleton<AudioController>
	{
		public const string baseResourcesPath = "Sounds/Sound Libraries";
		
		public static readonly CommandArray Commands = new CommandArray()
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
		public (string name, int index) AddAudioClipToDictionary(CustomAudioClip customAudioClip)
		{
			int index = 0;
			while (m_activeAudioTrackers.ContainsKey((customAudioClip.Name, index)))
				index++;
			var audioTracker = new AudioTracker(this);
			m_activeAudioTrackers.Add((customAudioClip.Name, index), audioTracker);
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
					if (enumerator.Current.Value is null)
					{
						m_activeAudioTrackers.Remove(enumerator.Current.Key);
						LateUpdate(); // Use a IEnumerator again to clear others
						return;
					}
					if (enumerator.Current.Value.IsPlaying)
						continue;
					enumerator.Current.Value.Dispose();
					m_activeAudioTrackers.Remove(enumerator.Current.Key);
					LateUpdate(); // Use a IEnumerator again to clear others
					return;
				}
		}
		private void ReloadSoundLibrary()
		{
			string resourcesPath = baseResourcesPath + '/' + SceneManager.ActiveScene.name;
			using (var enumerator = ActiveAudioTrackers.GetEnumerator())
				while (enumerator.MoveNext())
				{
					CustomAudioClip clip;
					if (enumerator.Current.Value.CustomAudioClip != null)
						clip = enumerator.Current.Value.CustomAudioClip;
					else if (!CurrentSoundLibrary.TryGetCustomAudioClip(enumerator.Current.Value.ClipName, out clip))
					{
						Debug.LogError($"Cannot find fileInfo about {enumerator.Current.Key.name}!");
						continue;
					}

					if (!clip.destroyWhenTransitioningScenes)
						continue;
					if (clip.fadeTime > 0f)
						enumerator.Current.Value.Stop(clip.fadeTime);
					else
						enumerator.Current.Value.Stop();
				}
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

		public AudioTracker PlaySound(string soundName)
		{
			if (!CurrentSoundLibrary.TryGetCustomAudioClip(soundName, out CustomAudioClip audioClip))
			{
				Debug.LogError($"Sound Name'{soundName}' is not found in '{CurrentSoundLibrary.name}'!");
				return null;
			}
			return PlaySound(audioClip);
		}
		public AudioTracker PlaySound(CustomAudioClip audioClip)
		{
			(string name, int index) pairKey = AddAudioClipToDictionary(audioClip);
			AudioTracker tracker = ActiveAudioTrackers[pairKey];
			tracker.PlaySingle(audioClip);
			return tracker;
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
			coroutineWrapper.AfterActions += mono =>
			{
				Destroy(oneShotSounds[audioMixerGroup.name].source);
				oneShotSounds[audioMixerGroup.name].disposeWrapper.Stop();
				oneShotSounds.Remove(audioMixerGroup.name);
			};
			oneShotSounds.Add(audioMixerGroup.name, (coroutineWrapper, customAudioSource));
			oneShotSounds[audioMixerGroup.name].disposeWrapper.Start();
		}
		private IEnumerator WaitUntil(string key)
		{
			yield return new WaitUntil(() => !oneShotSounds[key].source.isPlaying);
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.Audio.Editor
{
	using UnityEditor;
	using B1NARY.Audio;
	using UnityEngine;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

	[CustomEditor(typeof(AudioController))]
	public class AudioControllerEditor : Editor
	{
		public static void DisplayAudioData(IAudioInfo audioInfo)
		{
			Rect fullNameRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 20f)),
				toggleRect = new Rect(fullNameRect) { width = 20f };
			fullNameRect.xMin += toggleRect.xMax + 2;
			audioInfo.IsPlaying = EditorGUI.Toggle(toggleRect, audioInfo.IsPlaying);
			EditorGUI.LabelField(fullNameRect, audioInfo.ClipName, EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			Rect barRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 24f));
			barRect.xMin += 2f;
			barRect.xMax -= 2f;
			EditorGUI.ProgressBar(barRect, audioInfo.CompletionPercent(), audioInfo.TimeInfo());
			audioInfo.Volume = EditorGUILayout.Slider("Volume", audioInfo.Volume, 0f, 1f);
			audioInfo.Loop = EditorGUILayout.Toggle("Looping", audioInfo.Loop);
			audioInfo.Pitch = EditorGUILayout.Slider("Pitch", audioInfo.Pitch, 0f, 3f);
			EditorGUI.indentLevel--;
		}
		public static void DisplayAudioController(AudioController audioController)
		{
			if (audioController.ActiveAudioTrackers == null || !Application.isPlaying)
			{
				EditorGUILayout.LabelField("Reading data is only on runtime!");
				return;
			}
			EditorGUILayout.LabelField($"Total {nameof(AudioTracker)}s tracked: {audioController.ActiveAudioTrackers.Count}", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			using (IEnumerator<KeyValuePair<(string name, int index), AudioTracker>> enumerator = audioController.ActiveAudioTrackers.GetEnumerator())
				while (enumerator.MoveNext())
					DisplayAudioData(enumerator.Current.Value);
			EditorGUI.indentLevel--;
		}


		private AudioController audioController;
		private void Awake() => audioController = (AudioController)target;


		public override void OnInspectorGUI()
		{
			if (audioController.CurrentSoundLibrary != null)
				EditorGUILayout.LabelField($"Current Library: '{audioController.CurrentSoundLibrary.name}'");
			DisplayAudioController(audioController);
		}
		public override bool RequiresConstantRepaint() => audioController != null && audioController.ActiveAudioTrackers != null ? audioController.ActiveAudioTrackers.Count > 0 : false;
	}
}
#endif