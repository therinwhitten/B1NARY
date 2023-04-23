namespace B1NARY.Audio
{
	using OVSXmlSerializer;
	using System;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.Audio;

	public struct SerializedAudio
	{
		public static explicit operator SerializedAudio(AudioTracker audio)
		{
			return new SerializedAudio(audio);
		}
		public static SerializedAudio[] SerializeAudio()
		{
			var audio = new SerializedAudio[AudioController.Instance.ActiveAudio.Count];
			for (int i = 0; i < audio.Length; i++)
				audio[i] = new SerializedAudio(AudioController.Instance.ActiveAudio[i]);
			return audio;
		}

		public TimeSpan PlayedSeconds => TimeSpan.FromTicks(ticks);
		private long ticks;
		[field: XmlAttribute("clipName")]
		public string ClipName { get; }
		public string SceneName { get; }
		public SerializedAudio(AudioTracker tracker)
		{
			ticks = tracker.PlayedSeconds.Ticks;
			ClipName = tracker.ClipName;
			SceneName = SceneManager.ActiveScene.name;
		}
		public AudioTracker Play()
		{
			AudioTracker tracker;
			if (SceneName != SceneManager.ActiveScene.name)
			{
				SoundLibrary oldLibrary = AudioController.Instance.ActiveLibrary;
				AudioController.Instance.ActiveLibrary = Resources.Load<SoundLibrary>($"{AudioController.RESOURCES_SOUND_LIBRARY}/{SceneName}");
				if (AudioController.Instance.ActiveLibrary == null)
					throw new NullReferenceException($"Scene name '{SceneName}' for the sound library of clip '{ClipName}' is not found.");
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