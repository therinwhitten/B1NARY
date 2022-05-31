using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Handles audio for efficiency and garbage collection, uses 
/// <see cref="CustomAudioClip"/> and/or <see cref="UnityCustomAudioClip"/> for 
/// custom files and uses <see cref="SoundCoroutine"/>s for code to change it 
/// themselves.</summary>
public class AudioHandler : MonoBehaviour
{
	private string lastSpeaker = "";

	// Might be slightly tedious to re-implement the array every scene, but
	// - there are easy work-arounds and its not too big of a deal anyway
	[SerializeField, Tooltip("This calculates the data when needed.\n"
		+ "Can be nullable")] 
	private UnityCustomAudioClip[] customAudioData;
	private Dictionary<AudioClip, CustomAudioClip> audioClipDictionary = 
		new Dictionary<AudioClip, CustomAudioClip>();


	private Dictionary<AudioClip, SoundCoroutine> soundCoroutineCache
		= new Dictionary<AudioClip, SoundCoroutine>();

	private void Start()
	{
		foreach (UnityCustomAudioClip clip in customAudioData)
			audioClipDictionary.Add(clip.audioClip, (CustomAudioClip)clip);
	}

	// Returning a soundcoroutine may cause reference issues, so may do 
	// - Func<SoundCoroutine> instead.

	/// <summary>Easily create a coroutine and log it in to the cache.</summary>
	/// <param name="key">AudioClip to put into the soundCoroutine</param>
	/// <returns>
	///		<see cref="SoundCoroutine"/> that is meant to be kept track of and 
	///		used for accessibility of handling sound.
	///	</returns>
	private SoundCoroutine GetCoroutine(CustomAudioClip key)
	{
		if (!soundCoroutineCache.ContainsKey(key))
		{
			soundCoroutineCache.Add(key, new SoundCoroutine(this, key));
			soundCoroutineCache[key].GarbageCollection += (sender, clip) =>
			{
				Destroy(soundCoroutineCache[key].audioSource);
				soundCoroutineCache.Remove(key); 
			};
		}
		return soundCoroutineCache[key]; 
	}

	///	<summary>Plays a sound.</summary>
	///	<param name ="clip">the audioclip to play.</param>
	///	<param name ="useCustomAudioData">
	///		if the <see cref="AudioClip"/> is found in 
	///		customAudioData, then it will play that instead.
	///	</param>
	///	<returns>
	///		Optionally a <see cref="SoundCoroutine"/>, may not be 
	///		needed to function.
	///	</returns>
	public SoundCoroutine PlaySound(AudioClip clip, bool useCustomAudioData = true)
	{
		var sound = audioClipDictionary.ContainsKey(clip) && useCustomAudioData
			? GetCoroutine(audioClipDictionary[clip])
			: GetCoroutine((CustomAudioClip)clip);
		sound.PlaySingle();
		return sound;
	}

	///	<summary>Plays a sound with custom data. Ignores soundCoroutineCache</summary>
	///	<param name ="clip">the audioclip to play.</param>
	///	<returns>
	///		Optionally a <see cref="SoundCoroutine"/>, may not be 
	///		needed to function.
	///	</returns>
	public SoundCoroutine PlaySound(CustomAudioClip clip)
	{
		var sound = GetCoroutine(clip);
		sound.AudioClip = clip;
		sound.PlaySingle();
		return sound;
	}

	public SoundCoroutine PlaySound(AudioClip clip, float fadeInSeconds, 
		bool useCustomAudioData = true)
	{
		var sound = audioClipDictionary.ContainsKey(clip) && useCustomAudioData
			? GetCoroutine(audioClipDictionary[clip])
			: GetCoroutine((CustomAudioClip)clip);
		sound.PlaySingle(fadeInSeconds);
		return sound;
	}

	///	<summary>Plays a sound that is meant to be repeatedly played.</summary>
	///	<param name ="clip">the audioclip to play.</param>
	///	<returns>
	///		Optionally a <see cref="SoundCoroutine"/>, may not be 
	///		needed to function.
	///	</returns>
	public SoundCoroutine PlayOneShot(AudioClip clip, bool useCustomAudioData = true)
	{
		var sound = audioClipDictionary.ContainsKey(clip) && useCustomAudioData
			? GetCoroutine(audioClipDictionary[clip])
			: GetCoroutine((CustomAudioClip)clip);
		sound.PlayOneShot();
		return sound;
	}
}