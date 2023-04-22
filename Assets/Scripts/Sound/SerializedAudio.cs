namespace B1NARY.Audio
{
	using OVSXmlSerializer;
	using System;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.Audio;

	public struct SerializedAudio : IAudioInfo
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
			Debug.Log($"array length {audio.Length}:, \n\t{string.Join("\n\t", audio.Select(aud => aud.ClipName))}");
			return audio;
		}

		public TimeSpan PlayedSeconds => TimeSpan.FromTicks(ticks);
		private long ticks;
		[field: XmlAttribute("clipName")]
		public string ClipName { get; }
		public string SoundLibrary { get; }
		public SerializedAudio(AudioTracker tracker)
		{
			ticks = tracker.PlayedSeconds.Ticks;
			ClipName = tracker.ClipName;
			SoundLibrary = tracker.SoundLibrary.name;
		}
		public AudioTracker Play()
		{
			AudioTracker tracker;
			if (SoundLibrary != AudioController.Instance.ActiveLibrary.name)
			{
				SoundLibrary oldLibrary = AudioController.Instance.ActiveLibrary;
				AudioController.Instance.ActiveLibrary = Resources.Load<SoundLibrary>($"{AudioController.RESOURCES_SOUND_LIBRARY}/{SoundLibrary}");
				tracker = AudioController.Instance.AddSound(ClipName);
				AudioController.Instance.ActiveLibrary = oldLibrary;
			}
			else
				tracker = AudioController.Instance.AddSound(ClipName);
			if (tracker != null)
				tracker.PlayedSeconds = PlayedSeconds;
			return tracker;
		}

		bool IAudioInfo.IsPlaying { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		float IAudioInfo.Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		float IAudioInfo.Pitch { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		bool IAudioInfo.Loop { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		AudioMixerGroup IAudioInfo.CurrentGroup { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		TimeSpan IAudioInfo.TotalSeconds => throw new NotImplementedException();
	}
}