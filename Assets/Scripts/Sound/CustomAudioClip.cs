namespace B1NARY.Audio
{
	using System;
	using UnityEngine;
	using UnityEngine.Audio;

	///<summary>Internal struct handled by code to manage sound files, see 
	///<see cref="UnityCustomAudioClip"/> as it mainly borrows from there</summary>
	[Serializable]
	public class CustomAudioClip
	{
		public static explicit operator CustomAudioClip(AudioClip input) => new(input);
		public static explicit operator AudioClip(CustomAudioClip input) => input.clip;

		public CustomAudioClip(AudioClip audioClip)
		{
			clip = audioClip;
		}

		/// <summary>
		/// Gets a trimmed name of <see cref="clip"/>, otherwise, empty if <see langword="null"/>.
		/// </summary>
		public string Name => clip != null ? clip.name.Trim() : string.Empty;

		/// <summary> the Audio Clip stored, meant for playing sounds. </summary>
		public AudioClip clip;
		/// <summary> Mixer Group, meant for automatically changing values via user input. </summary>
		public AudioMixerGroup audioMixerGroup = null;
		/// <summary> The volume the <see cref="clip"/> meant to be played on. </summary>
		public float volume = 1;
		public float volumeVariance = 0;
		/// <summary> The pitch the <see cref="clip"/> meant to be played on. </summary>
		public float pitch = 1;
		public float pitchVariance = 0;
		/// <summary> 
		/// If the <see cref="clip"/> should loop if it reached the 
		/// end of its cycle. 
		/// </summary>
		public bool loop = false;
		/// <summary> If it should play right when it is noticed. </summary>
		public bool playOnAwake = false;
		/// <summary> If the sound playing will stop when switching scenes. </summary>
		public bool destroyWhenTransitioningScenes = true;
		/// <summary> How long it will take to fade during the switch of scenes. </summary>
		public float fadeTime = 0;
		/// <summary> 
		/// Random type used for randomization of <see cref="pitchVariance"/> 
		/// and <see cref="volumeVariance"/>. 
		/// </summary>
		public RandomForwarder.RandomType randomType;

		/// <summary>
		/// Final pitch by basing on <see cref="volume"/>, and adjusting it on 
		/// <see cref="volumeVariance"/> with <see cref="randomType"/>.
		/// </summary>
		public float FinalVolume
		{
			get
			{
				if (volumeVariance == 0)
					return volume;
				float adjustedVolumeRandom = volumeVariance * volume,
					randomVolume = volume - adjustedVolumeRandom;
				randomVolume += RandomForwarder.NextFloat(randomType) * adjustedVolumeRandom;
				return randomVolume;
			}
		}
		/// <summary>
		/// Final pitch by basing on <see cref="pitch"/>, and adjusting it on 
		/// <see cref="pitchVariance"/> with <see cref="randomType"/>.
		/// </summary>
		public float FinalPitch
		{
			get
			{
				if (pitchVariance == 0)
					return pitch;
				float adjustedPitchRandom = pitchVariance * pitch,
					randomPitch = pitch - adjustedPitchRandom;
				randomPitch += RandomForwarder.NextFloat(randomType) * adjustedPitchRandom;
				return randomPitch;
			}
		}

		public void ApplyTo(AudioSource audioSource)
		{
			audioSource.clip = clip;
			audioSource.outputAudioMixerGroup = audioMixerGroup;
			audioSource.loop = loop;
			audioSource.volume = FinalVolume;
			audioSource.pitch = FinalPitch;
		}

		public override string ToString() => clip.ToString();
	}
}