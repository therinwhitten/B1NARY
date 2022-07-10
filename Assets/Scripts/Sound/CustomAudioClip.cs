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
	public float minVolumeVariance = 1, maxVolumeVariance = 1;
	public float pitch = 1;
	public float minPitchVariance = 1, maxPitchVariance = 1;
	public bool loop = false;
	public bool playOnAwake = false;
	public bool fadeWhenTransitioning = true;
	public RandomFowarder.RandomType randomType;

	public override string ToString() => clip.ToString();
}