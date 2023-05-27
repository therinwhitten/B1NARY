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
	using System.Globalization;
	using SceneManager = UnityEngine.SceneManagement.SceneManager;
	using UnityEngine.SceneManagement;
	using System.Diagnostics.CodeAnalysis;

	public sealed class AudioController : Singleton<AudioController>
	{
		public const string RESOURCES_SOUND_LIBRARY = "Sounds/Sound Libraries";

		public static readonly CommandArray Commands = new CommandArray()
		{
			["fadeinsound"] = (Action<string, string>)((name, floatStr) =>
			{
				name = name.Trim();
				float fadeIn = float.Parse(floatStr);
				Instance.AddSound(name, fadeIn);
			}),
			["fadeoutsound"] = (Action<string, string>)((name, floatStr) =>
			{
				name = name.Trim();
				float fadeOut = float.Parse(floatStr);
				if (Instance.RemoveSound(name, fadeOut))
					Debug.LogWarning($"{name} doesn't lead to any sounds and cannot be stopped!" +
						$"\nStoppable sounds: {string.Join(", ", Instance.ActiveAudio)}");
			}),
			["playsound"] = (Action<string>)((name) =>
			{
				name = name.Trim();
				Instance.AddSound(name);
			}),
			["stopsound"] = (Action<string>)((name) =>
			{
				name = name.Trim();
				if (Instance.RemoveSound(name))
					Debug.LogWarning($"{name} doesn't lead to any sounds and cannot be stopped!" +
						$"\nStoppable sounds: {string.Join(", ", Instance.ActiveAudio)}");
			}),
		};

		public SoundLibrary ActiveLibrary { get; internal set; } = null;
		// List contains null values to reserve space
		public IReadOnlyList<AudioTracker> ActiveAudio => audioTrackers;
		private readonly List<AudioTracker> audioTrackers = new List<AudioTracker>();
		public readonly Queue<int> otherValues = new Queue<int>();
		internal List<Func<bool>> fadeSounds = new List<Func<bool>>();

		private void Awake()
		{
			SceneManager.activeSceneChanged += OnSceneChange;
			OnSceneChange(default, SceneManager.GetActiveScene());
		}

		private void OnSceneChange(Scene oldScene, Scene newScene)
		{
			string resourcesPath = $"{RESOURCES_SOUND_LIBRARY}/{newScene.name}";

			for (int i = 0; i < audioTrackers.Count; i++)
			{
				AudioTracker current = audioTrackers[i];
				if (current is null)
					continue;
				if (current.CustomAudioClip.destroyWhenTransitioningScenes)
					RemoveSound(i);
			}

			SoundLibrary newSoundLibrary = Resources.Load<SoundLibrary>(resourcesPath);
			if (newSoundLibrary == null)
				throw new FileNotFoundException($"{resourcesPath} is not a valid resource path of sound library!");
			ActiveLibrary = newSoundLibrary;
			if (newSoundLibrary.ContainsPlayOnAwakeCommands)
			{
				using (IEnumerator<CustomAudioClip> enumerator = newSoundLibrary.PlayOnAwakeCommands.GetEnumerator())
					while (enumerator.MoveNext())
						AddSound(enumerator.Current);
			}
		}

		private void FixedUpdate()
		{
			for (int i = 0; i < fadeSounds.Count; i++)
			{
				Func<bool> current = fadeSounds[i];
				if (current.Invoke())
					continue;
				fadeSounds.RemoveAt(i);
				i--;
			}
		}
		private void LateUpdate()
		{
			for (int i = 0; i < audioTrackers.Count; i++)
			{
				AudioTracker current = audioTrackers[i];
				if (current is null)
					continue;
				if (!current.canAutoDispose)
					continue;
				if (current.IsPlaying)
					continue;
				current.Dispose();
				RemoveSound(i);
				i--;
			}
		}

		public bool RemoveSound(int index, float fadeOut = 0f)
		{
			if (audioTrackers.Count <= index)
				return false;
			if (audioTrackers[index] is null)
				return false;
			audioTrackers[index].Stop(fadeOut);
			audioTrackers[index] = null;
			otherValues.Enqueue(index);
			return true;
		}
		public bool RemoveSound(string soundName, float fadeOut = 0f)
		{
			for (int i = 0; i < audioTrackers.Count; i++)
			{
				AudioTracker tracker = audioTrackers[i];
				if (tracker is null)
					continue;
				if (tracker.ClipName == soundName)
					return RemoveSound(i, fadeOut);
			}
			return false;
		}
		public void RemoveAllSounds()
		{
			for (int i = 0; i < audioTrackers.Count; i++)
				RemoveSound(i);
		}
		
		public AudioTracker AddSound(CustomAudioClip clip, float fadeIn = 0f)
		{
			var tracker = new AudioTracker(clip);
			if (otherValues.Count > 0)
				audioTrackers[otherValues.Dequeue()] = tracker;
			else
				audioTrackers.Add(tracker);
			tracker.Start(fadeIn);
			return tracker;
		}

		[SuppressMessage("Correctness", "UNT0008:Null propagation on Unity objects", Justification = "<Pending>")]
		public AudioTracker AddSound(string soundName, float fadeIn = 0f)
		{
			for (int i = 0; i < ActiveLibrary.customAudioClips.Count; i++)
			{
				CustomAudioClip clip = ActiveLibrary.customAudioClips[i];
				if (clip.Name.ToLower() == soundName.ToLower())
					return AddSound(clip, fadeIn);
			}
			Debug.LogException(new NullReferenceException($"sound '{soundName}' does not exist in library '{ActiveLibrary?.name}'"));
			return null;
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
			if (!Application.isPlaying)
			{
				EditorGUILayout.LabelField("Reading data is only on runtime!");
				return;
			}
			EditorGUILayout.LabelField($"Total {nameof(AudioTracker)}s tracked: {audioController.ActiveAudio.Count}", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			for (int i = 0; i < audioController.ActiveAudio.Count; i++)
			{
				DisplayAudioData(audioController.ActiveAudio[i]);
			}
			EditorGUI.indentLevel--;
		}


		private AudioController audioController;
		private void Awake() => audioController = (AudioController)target;


		public override void OnInspectorGUI()
		{
			if (audioController.ActiveLibrary != null)
				EditorGUILayout.LabelField($"Current Library: '{audioController.ActiveLibrary.name}'");
			DisplayAudioController(audioController);
		}
		public override bool RequiresConstantRepaint() => audioController != null && audioController.ActiveAudio != null ? audioController.ActiveAudio.Count > 0 : false;
	}
}
#endif