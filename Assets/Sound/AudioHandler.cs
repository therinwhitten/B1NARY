using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>Handles audio for efficiency and garbage collection, uses 
/// <see cref="CustomAudioClip"/> and/or <see cref="UnityCustomAudioClip"/> for 
/// custom files and uses <see cref="SoundCoroutine"/>s for code to change it 
/// themselves.</summary>
public class AudioHandler : SingletonNew<AudioHandler>
{
	public static bool HasPreviousInstance { get; private set; } = false;

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

	protected override void SingletonStart()
	{
		audioClipDictionary = customAudioData.Cast<CustomAudioClip>().ToDictionary(x => x.audioClip);
		Debug.Log($"{nameof(AudioHandler)} started!");
		if (HasPreviousInstance)
			Debug.LogWarning($"Another {nameof(AudioHandler)} already exists!");
		else
			HasPreviousInstance = true;
	}


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

	private AudioClip GetSound(string soundPath)
	{
		AudioClip audioClip = Resources.Load<AudioClip>(soundPath);
		if (audioClip == null)
			throw new NullReferenceException($"{soundPath} does not lead to a sound" +
			"file!");
		return audioClip;
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

	///	<summary>Searches a sound in the resources folder via filePath, then plays it.</summary>
	///	<param name ="soundPath">the audioclip path in the resource folder to play.</param>
	///	<param name ="useCustomAudioData">
	///		if the <see cref="AudioClip"/> is found in 
	///		customAudioData, then it will play that instead.
	///	</param>
	///	<returns>
	///		Optionally a <see cref="SoundCoroutine"/>, may not be 
	///		needed to function.
	///	</returns>
	public Func<SoundCoroutine> PlaySound(string soundPath, 
		AudioMixerGroup mixerGroup = null, bool useCustomAudioData = true)
		=> PlaySound(GetSound(soundPath), mixerGroup, useCustomAudioData);

	///	<summary>Fades in a sound.</summary>
	///	<param name ="clip">the audioclip to play.</param>
	///	<param name="fadeInSeconds">Seconds which it goes from 0 to 1.</param>
	///	<param name ="useCustomAudioData">
	///		if the <see cref="AudioClip"/> is found in 
	///		customAudioData, then it will play that instead.
	///	</param>
	///	<returns>
	///		Optionally a <see cref="SoundCoroutine"/>, may not be 
	///		needed to function.
	///	</returns>
	public Func<SoundCoroutine> PlayFadedSound(AudioClip clip, float fadeInSeconds,
		AudioMixerGroup mixerGroup = null, bool useCustomAudioData = true)
	{
		var sound = audioClipDictionary.ContainsKey(clip) && useCustomAudioData
			? GetCoroutine(audioClipDictionary[clip], audioClipDictionary[clip].audioMixerGroup)
			: GetCoroutine((CustomAudioClip)clip, mixerGroup);
		sound().PlaySingle(fadeInSeconds);
		return sound;
	}

	///	<summary>Searches a sound in the resources folder via filePath, then fades it in.</summary>
	///	<param name ="soundPath">the audioclip path in the resource folder to play.</param>
	///	<param name="fadeInSeconds">Seconds which it goes from 0 to 1.</param>
	///	<param name ="useCustomAudioData">
	///		if the <see cref="AudioClip"/> is found in 
	///		customAudioData, then it will play that instead.
	///	</param>
	///	<returns>
	///		Optionally a <see cref="SoundCoroutine"/>, may not be 
	///		needed to function.
	///	</returns>
	public Func<SoundCoroutine> PlayFadedSound(string soundPath, float fadeInSeconds,
		AudioMixerGroup mixerGroup = null, bool useCustomAudioData = true)
		=> PlayFadedSound(GetSound(soundPath), fadeInSeconds, mixerGroup, useCustomAudioData);

	public void StopSoundViaFade(AudioClip sound, float fadeOutSeconds)
		=> SoundCoroutineCache[sound].Stop(fadeOutSeconds);
	public void StopSoundViaFade(string soundName, float fadeOutSeconds)
	{
		foreach (AudioClip sound in SoundCoroutineCache.Keys)
			if (sound.name == soundName)
				SoundCoroutineCache[sound].Stop(fadeOutSeconds);
		throw new KeyNotFoundException(soundName);
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

	~AudioHandler()
	{
		Debug.Log($"{nameof(AudioHandler)} being disposed of");
		// This may cause issues if multiple audioHandlers are ran, but shouldn't
		// - be intended to do that anyway.
		HasPreviousInstance = false;
	}
}