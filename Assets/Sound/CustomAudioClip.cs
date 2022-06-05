using System;
using UnityEngine;
using UnityEngine.Audio;

///<summary>Internal struct handled by code to manage sound files, see 
///<see cref="UnityCustomAudioClip"/> as it mainly borrows from there</summary>
[Serializable] 
public struct CustomAudioClip
{

	public static implicit operator AudioClip(CustomAudioClip input)
		=> input.audioClip;
	public static explicit operator CustomAudioClip(AudioClip input)
		=> new CustomAudioClip(input);

	public CustomAudioClip(AudioClip audioClip)
	{
		this.audioClip = audioClip;
		volume = 1;
		volumeVariance = 0;
		pitch = 1;
		pitchVariance = 0;
		audioMixerGroup = null;
		loop = false;
		fadeWhenTransitioning = 0.5f;
		willFadeWhenTransitioning = true;
	}

	public AudioClip audioClip;
	[Range(0, 1)] public float volume;
	[Range(0, 1)] public float volumeVariance;
	[Range(0, 3)] public float pitch;
	[Range(0, 1)] public float pitchVariance;
	public AudioMixerGroup audioMixerGroup;
	public bool loop;
	[Header("Scene Transitions")] public bool willFadeWhenTransitioning;
	[Range(0, 3)] public float fadeWhenTransitioning;

	public override string ToString() => audioClip.ToString();
}