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

	[SerializeField, Tooltip("A readonly array which keeps track of data for" +
		" audioClips, solves at runtime.")] 
	private UnityCustomAudioClip[] customAudioData;

	/// <summary> Easily Links a normal audioclip to a custom one. </summary>
	private Dictionary<AudioClip, CustomAudioClip> audioClipDictionary = 
		new Dictionary<AudioClip, CustomAudioClip>();

	/// <summary> names of audioclips tied to them. </summary>
	private Dictionary<string, AudioClip> nameDictionary = 
		new Dictionary<string, AudioClip>();

	/// <summary>Cache for storing automated sound data</summary>
	public static Dictionary<AudioClip, SoundCoroutine> SoundCoroutineCache { get; private set; }
		= new Dictionary<AudioClip, SoundCoroutine>();

	public VoiceActorHandler VoiceActorHandler { get; private set; }

	protected override void SingletonStart()
	{
		for (int i = 0; i < customAudioData.Length; i++)
		{
			audioClipDictionary.Add(customAudioData[i].audioClip, (CustomAudioClip)customAudioData[i]);
			nameDictionary.Add(customAudioData[i].audioClip.name, customAudioData[i].audioClip);
		}
		Debug.Log($"{nameof(AudioHandler)} started!");
		if (HasPreviousInstance)
			Debug.LogError($"Another {nameof(AudioHandler)} already exists!");
		else
		{
			HasPreviousInstance = true;
			VoiceActorHandler = gameObject.AddComponent<VoiceActorHandler>();
		}
		PlayOnAwakeCommands();
		GameCommands.SwitchingScenes += (sender, args) => TransferSounds();
	}
	private void PlayOnAwakeCommands()
	{
		IEnumerable<CustomAudioClip> playOnAwake =
			from clip in audioClipDictionary
			where clip.Value.playOnAwake
			select clip.Value;
		foreach (var clip in playOnAwake)
			PlaySound(clip);
	}


	private Func<SoundCoroutine> GetCoroutine(AudioClip clip, AudioMixerGroup group, 
		bool useCustomAudioData)
		=> audioClipDictionary.ContainsKey(clip) && useCustomAudioData
			? GetCoroutine(audioClipDictionary[clip])
			: GetCoroutine(new CustomAudioClip(clip) { audioMixerGroup = group });

	/// <summary>Easily create a coroutine and log it in to the cache.</summary>
	/// <param name="key">AudioClip to put into the soundCoroutine</param>
	/// <returns>
	///		<see cref="SoundCoroutine"/> that is meant to be kept track of and 
	///		used for accessibility of handling sound.
	///	</returns>
	public Func<SoundCoroutine> GetCoroutine(CustomAudioClip key)
	{
		if (!SoundCoroutineCache.ContainsKey(key))
		{
			SoundCoroutineCache.Add(key, new SoundCoroutine(this, key.audioMixerGroup, clip: key));
			SoundCoroutineCache[key].GarbageCollection += (sender, clip) =>
				GarbageCollectionDefault(key);
		}
		return () => SoundCoroutineCache[key]; 
	}
	private void GarbageCollectionDefault(AudioClip clip)
	{
		Destroy(SoundCoroutineCache[clip].AudioSource);
		SoundCoroutineCache.Remove(clip);
	}

	/// <summary> Retrives a sound via string </summary>
	/// <param name="soundPath"></param>
	/// <returns></returns>
	/// <exception cref="NullReferenceException">When neither the fileName finding in <see cref="CustomAudioClip[]"/></exception>
	private AudioClip GetRawAudioClip(string soundPath)
	{
		// names in customAudioClip
		if (nameDictionary.ContainsKey(soundPath))
			return nameDictionary[soundPath];
		Debug.Log($"'{soundPath}' is not found in {nameof(customAudioData)},"
			+ " searching via filePath");
		// filePath
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
		Func<SoundCoroutine> sound = GetCoroutine(clip, mixerGroup, useCustomAudioData);
		sound().PlaySingle();
		return sound;
	}

	///	<summary>Plays a sound with custom data. Ignores soundCoroutineCache</summary>
	///	<param name ="clip">the audioclip to play.</param>
	///	<returns>
	///		Optionally a <see cref="SoundCoroutine"/>, may not be 
	///		needed to function.
	///	</returns>
	public Func<SoundCoroutine> PlaySound(CustomAudioClip clip)
	{
		var sound = GetCoroutine(clip);
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
		=> PlaySound(GetRawAudioClip(soundPath), mixerGroup, useCustomAudioData);

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
		Func<SoundCoroutine> sound = GetCoroutine(clip, mixerGroup, useCustomAudioData);
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
		=> PlayFadedSound(GetRawAudioClip(soundPath), fadeInSeconds, mixerGroup, useCustomAudioData);

	public void StopSoundViaFade(AudioClip sound, float fadeOutSeconds)
		=> SoundCoroutineCache[sound].Stop(fadeOutSeconds);
	public void StopSoundViaFade(string soundName, float fadeOutSeconds)
	{
		if (nameDictionary.ContainsKey(soundName))
			SoundCoroutineCache[nameDictionary[soundName]].Stop(fadeOutSeconds);
		throw new KeyNotFoundException(soundName);
	}

	public void StopSound(string soundName)
	{
		if (nameDictionary.ContainsKey(soundName))
			SoundCoroutineCache[nameDictionary[soundName]].Stop();
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
		Func<SoundCoroutine> sound = GetCoroutine(clip, mixerGroup, useCustomAudioData);
		sound().PlayOneShot();
		return sound;
	}




	private void TransferSounds()
	{
		var gameObject = new GameObject("AudioHandler Transfer Sounds");
		var transferer = gameObject.AddComponent<AudioCrossSceneTransferer>();
		transferer.AudioCoroutines = SoundCoroutineCache.Values.ToArray();
		transferer.StartSceneTransferer();
	}

	protected override void OnSingletonDestroy()
	{
		Debug.Log($"{nameof(AudioHandler)} being disposed of");
		// This may cause issues if multiple audioHandlers are ran, but shouldn't
		// - be intended to do that anyway.
		HasPreviousInstance = false;
	}
}