using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>Handles audio for efficiency and garbage collection, uses 
/// <see cref="CustomAudioClip"/> and/or <see cref="UnityCustomAudioClip"/> for 
/// custom files and uses <see cref="SoundCoroutine"/>s for code to change it 
/// themselves.</summary>
public class AudioHandler : MonoBehaviour
{

	// Might be slightly tedious to re-implement the array every scene, but
	// - there are easy work-arounds and its not too big of a deal anyway
	[SerializeField, Tooltip("A readonly array which keeps track of data for" +
		" audioClips, solves at runtime.")] 
	private UnityCustomAudioClip[] customAudioData;
	private Dictionary<AudioClip, CustomAudioClip> audioClipDictionary = 
		new Dictionary<AudioClip, CustomAudioClip>();

	/// <summary>Cache for storing automated sound data</summary>
	public Dictionary<AudioClip, SoundCoroutine> SoundCoroutineCache { get; private set; }
		= new Dictionary<AudioClip, SoundCoroutine>();

	private void Start()
	{
		audioClipDictionary = customAudioData.Cast<CustomAudioClip>().ToDictionary(x => x.audioClip);
	}

	// Returning a soundcoroutine may cause reference issues, so may do 
	// - Func<SoundCoroutine> instead.

	/// <summary>Easily create a coroutine and log it in to the cache.</summary>
	/// <param name="key">AudioClip to put into the soundCoroutine</param>
	/// <returns>
	///		<see cref="SoundCoroutine"/> that is meant to be kept track of and 
	///		used for accessibility of handling sound.
	///	</returns>
	public Func<SoundCoroutine> GetCoroutine(CustomAudioClip key, AudioMixerGroup mixerGroup)
	{
		if (!SoundCoroutineCache.ContainsKey(key))
		{
			SoundCoroutineCache.Add(key, new SoundCoroutine(this, mixerGroup, clip: key));
			SoundCoroutineCache[key].GarbageCollection += (sender, clip) =>
			{
				Destroy(SoundCoroutineCache[key].AudioSource);
				SoundCoroutineCache.Remove(key); 
			};
		}
		return () => SoundCoroutineCache[key]; 
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
	public Func<SoundCoroutine> PlaySound(AudioClip clip, 
		AudioMixerGroup mixerGroup = null, bool useCustomAudioData = true)
	{
		var sound = audioClipDictionary.ContainsKey(clip) && useCustomAudioData
			? GetCoroutine(audioClipDictionary[clip], audioClipDictionary[clip].audioMixerGroup)
			: GetCoroutine((CustomAudioClip)clip, mixerGroup);
		sound().PlaySingle();
		return sound;
	}

	///	<summary>Plays a sound with custom data. Ignores soundCoroutineCache</summary>
	///	<param name ="clip">the audioclip to play.</param>
	///	<returns>
	///		Optionally a <see cref="SoundCoroutine"/>, may not be 
	///		needed to function.
	///	</returns>
	public Func<SoundCoroutine> PlaySound(CustomAudioClip clip, AudioMixerGroup mixerGroup = null)
	{
		var sound = GetCoroutine(clip, mixerGroup);
		sound().AudioClip = clip;
		sound().PlaySingle();
		return sound;
	}

	public Func<SoundCoroutine> PlaySound(AudioClip clip, float fadeInSeconds,
		AudioMixerGroup mixerGroup = null, bool useCustomAudioData = true)
	{
		var sound = audioClipDictionary.ContainsKey(clip) && useCustomAudioData
			? GetCoroutine(audioClipDictionary[clip], audioClipDictionary[clip].audioMixerGroup)
			: GetCoroutine((CustomAudioClip)clip, mixerGroup);
		sound().PlaySingle(fadeInSeconds);
		return sound;
	}

	///	<summary>Plays a sound that is meant to be repeatedly played.</summary>
	///	<param name ="clip">the audioclip to play.</param>
	///	<returns>
	///		Optionally a <see cref="SoundCoroutine"/>, may not be 
	///		needed to function.
	///	</returns>
	public Func<SoundCoroutine> PlayOneShot(AudioClip clip, 
		AudioMixerGroup mixerGroup = null, bool useCustomAudioData = true)
	{
		var sound = audioClipDictionary.ContainsKey(clip) && useCustomAudioData
			? GetCoroutine(audioClipDictionary[clip], audioClipDictionary[clip].audioMixerGroup)
			: GetCoroutine((CustomAudioClip)clip, mixerGroup);
		sound().PlayOneShot();
		return sound;
	}
}