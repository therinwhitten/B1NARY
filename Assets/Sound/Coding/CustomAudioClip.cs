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
	[FormerlySerializedAs("Mixer Group")] public AudioMixerGroup audioMixerGroup = null;
	[Space, Range(0, 1)] public float volume = 1;
	[Range(0, 1)] public float volumeVariance = 0;
	[Space, Range(0, 3)] public float pitch = 1;
	[Range(0, 1)] public float pitchVariance = 0;
	[Space] public bool loop = false;
	public bool playOnAwake = false;
	[Header("Scene Transitions"), Tooltip("if the sound fade or stop during scene transition or not.")]
	public bool willFadeWhenTransitioning = true;
	[Range(0, 60), Tooltip("How long it will take before completely fading into 0")] 
	public float fadeWhenTransitioning = 0.3f;

	public override string ToString() => clip.ToString();
}