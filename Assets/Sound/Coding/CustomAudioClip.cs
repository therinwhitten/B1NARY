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
		this.clip = audioClip;
	}

	public AudioClip clip;
	public AudioMixerGroup audioMixerGroup = null;
	public float volume = 1;
	public float volumeVariance = 0;
	public float pitch = 1;
	public float pitchVariance = 0;
	public bool loop = false;
	public bool playOnAwake = false;
	public bool willFadeWhenTransitioning = true;
	public float fadeWhenTransitioning = 0.3f;
	public RandomFowarder.RandomType randomType;

	public override string ToString() => clip.ToString();
}