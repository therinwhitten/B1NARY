namespace B1NARY.Audio
{
	using OVSXmlSerializer;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.Audio;

	public struct SerializedAudio
	{
		public static explicit operator SerializedAudio(AudioTracker audio)
		{
			return new SerializedAudio(audio);
		}
		public static List<SerializedAudio> SerializeAudio()
		{
			var otherValues = new HashSet<int>(AudioController.Instance.otherValues);
			int length = AudioController.Instance.ActiveAudio.Count - otherValues.Count;
			var audio = new List<SerializedAudio>(length);
			for (int i = 0; i < length; i++)
				if (!otherValues.Contains(i))
					audio.Add(new SerializedAudio(AudioController.Instance.ActiveAudio[i]));
			return audio;
		}

		public TimeSpan PlayedSeconds => TimeSpan.FromTicks(ticks);
		private readonly long ticks;
		[field: XmlAttribute("clipName")]
		public string ClipName { get; }
		public string SceneName { get; }
		public SerializedAudio(AudioTracker tracker)
		{
			if (tracker is null)
				throw new ArgumentNullException(nameof(tracker));
			ticks = tracker.PlayedSeconds.Ticks;
			ClipName = tracker.ClipName;
			SceneName = tracker.SceneName;
		}
		public AudioTracker Play()
		{
			AudioTracker tracker;
			if (SceneName != SceneManager.ActiveScene.name)
			{
				SoundLibrary oldLibrary = AudioController.Instance.ActiveLibrary;
				string resourcesPath = $"{AudioController.RESOURCES_SOUND_LIBRARY}/{SceneName}";
				AudioController.Instance.ActiveLibrary = Resources.Load<SoundLibrary>(resourcesPath);
				if (AudioController.Instance.ActiveLibrary == null)
				{
					AudioController.Instance.ActiveLibrary = oldLibrary;
					throw new NullReferenceException($"Scene name '{SceneName}' for the sound library of clip '{ClipName}' is not found.\n"
						+ $"Audio Path: {resourcesPath}");
				}
				tracker = AudioController.Instance.AddSound(ClipName);
				AudioController.Instance.ActiveLibrary = oldLibrary;
			}
			else
				tracker = AudioController.Instance.AddSound(ClipName);
			if (tracker != null)
				tracker.PlayedSeconds = PlayedSeconds;
			return tracker;
		}
	}
}