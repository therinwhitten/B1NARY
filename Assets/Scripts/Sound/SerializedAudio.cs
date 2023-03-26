namespace B1NARY.Audio
{
	using OVSXmlSerializer;
	using System;
	using UnityEngine.Audio;

	public struct SerializedAudio : IAudioInfo
	{
		public static explicit operator SerializedAudio(AudioTracker audio)
		{
			return new SerializedAudio(audio);
		}
		public static SerializedAudio[] SerializeAudio()
		{
			var audio = new SerializedAudio[AudioController.Instance.ActiveAudioTrackers.Count];
			using (var enumerator = AudioController.Instance.ActiveAudioTrackers.GetEnumerator())
				for (int i = 0; enumerator.MoveNext(); i++)
					audio[i] = new SerializedAudio(enumerator.Current.Value);
			return audio;
		}

		public TimeSpan PlayedSeconds => TimeSpan.FromTicks(ticks);
		private long ticks;
		[field: XmlAttribute("clipName")]
		public string ClipName { get; }
		public SerializedAudio(AudioTracker tracker)
		{
			ticks = tracker.PlayedSeconds.Ticks;
			ClipName = tracker.ClipName;
		}
		public AudioTracker Play()
		{
			AudioTracker tracker = AudioController.Instance.PlaySound(ClipName);
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