using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

///<summary>Internal struct handled by code to manage sound files, see 
///<see cref="UnityCustomAudioClip"/> as it mainly borrows from there</summary>
[Serializable] 
public class CustomAudioClip
{

	public static implicit operator AudioClip(CustomAudioClip input)
		=> input.clip;
	public static explicit operator CustomAudioClip(AudioClip input)
		=> new CustomAudioClip(input);

	public CustomAudioClip(AudioClip audioClip)
	{
		clip = audioClip;
	}

	public string Name => clip != null ? clip.name.Trim() : string.Empty;

	public AudioClip clip;
	public AudioMixerGroup audioMixerGroup = null;
	public float volume = 1;
	public float volumeVariance = 0;
	public float pitch = 1;
	public float pitchVariance = 0;
	public bool loop = false;
	public bool playOnAwake = false;
	public bool destroyWhenTransitioningScenes = true;
	public float fadeTime = 0;
	public RandomFowarder.RandomType randomType;

	public float FinalVolume { get
		{
			if (volumeVariance == 0)
				return volume;
			float adjustedVolumeRandom = volumeVariance * volume,
				randomVolume = volume - adjustedVolumeRandom;
			randomVolume += RandomFowarder.NextFloat(randomType) * adjustedVolumeRandom;
			return randomVolume;
		} 
	}
	public float FinalPitch { get
		{
			if (pitchVariance == 0)
				return pitch;
			float adjustedPitchRandom = pitchVariance * pitch,
				randomPitch = pitch - adjustedPitchRandom;
			randomPitch += RandomFowarder.NextFloat(randomType) * adjustedPitchRandom;
			return randomPitch;
		} 
	}

	public override string ToString() => clip.ToString();
}