namespace B1NARY.Audio
{
	using System;
	using UnityEngine;
	using UnityEngine.Audio;

	/// <summary> Extension Methods for <see cref="IAudioInfo"/>. </summary>
	public static class AudioInfo
	{
		/// <summary>
		/// A percentage from 0 to 1 from <see cref="IAudioInfo.PlayedSeconds"/>
		/// to <see cref="IAudioInfo.TotalSeconds"/>.
		/// </summary>
		public static float CompletionPercent(this IAudioInfo info) =>
			Mathf.Clamp01((float)info.PlayedSeconds.TotalSeconds / (float)info.TotalSeconds.TotalSeconds);
		/// <summary>
		/// Turns the two <see cref="TimeSpan"/>s as a single string for visual
		/// purposes.
		/// </summary>
		/// <returns> <c> PlayedTime : TotalTime </c> </returns>
		public static string TimeInfo(this IAudioInfo info) =>
			$"{info.PlayedSeconds.TotalSeconds:N2} : {info.TotalSeconds.TotalSeconds:N2}";
	}

	/// <summary> Extensive data about an audiosource tracker. </summary>
	public interface IAudioInfo
	{
		/// <summary> If the class is playing any sounds. </summary>
		bool IsPlaying { get; set; }
		string ClipName { get; }
		float Volume { get; set; }
		float Pitch { get; set; }
		bool Loop { get; set; }
		AudioMixerGroup CurrentGroup { get; set; }

		TimeSpan PlayedSeconds { get; }
		TimeSpan TotalSeconds { get; }
	}
}