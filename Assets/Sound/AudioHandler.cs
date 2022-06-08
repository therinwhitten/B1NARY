using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

/// <summary>Handles audio for efficiency and garbage collection, uses 
/// <see cref="CustomAudioClip"/> and/or <see cref="UnityCustomAudioClip"/> for 
/// custom files and uses <see cref="SoundCoroutine"/>s for code to change it 
/// themselves.</summary>
public class AudioHandler : SingletonNew<AudioHandler>, IEnumerable<SoundCoroutine>
{
	public static bool HasPreviousInstance { get; private set; } = false;

	[SerializeField] private bool loadAudioSourceForVoiceActors = true;
	public VoiceActorHandler VoiceActorHandler { get; private set; }

	[SerializeField, Tooltip("A readonly array which keeps track of data for" +
		" audioClips, solves at runtime.")] 
	private SoundConfiguration[] customAudioData;

	// <summary> Easily Links a normal audioclip to a custom one. </summary>
	//private Dictionary<AudioClip, CustomAudioClip> audioClipDictionary = 
	//	new Dictionary<AudioClip, CustomAudioClip>();

	// <summary> names of audioclips tied to them. </summary>
	//private Dictionary<string, AudioClip> nameDictionary;

	/// <summary> Cache for storing automated sound data </summary>
	public static Dictionary<AudioClip, SoundCoroutine> SoundCoroutineCache { get; private set; }
		= new Dictionary<AudioClip, SoundCoroutine>();


	protected override void SingletonStart()
	{
		Debug.Log($"{nameof(AudioHandler)} started!");
		if (HasPreviousInstance)
			Debug.LogError($"Another {nameof(AudioHandler)} already exists!");
		else
		{
			HasPreviousInstance = true;
			if (loadAudioSourceForVoiceActors)
				VoiceActorHandler = gameObject.AddComponent<VoiceActorHandler>();
		}
		PlayOnAwakeCommands();
		GameCommands.SwitchingScenes += (sender, args) => TransferSounds();
	}
	private void PlayOnAwakeCommands()
	{
		for (int i = 0; i < customAudioData.Length; i++)
			for (int j = 0; j < customAudioData[i].Length; j++)
				if (customAudioData[i][j].playOnAwake)
					PlaySound(customAudioData[i][j]);
	}


	private Func<SoundCoroutine> GetCoroutine(AudioClip clip, AudioMixerGroup group,
		bool useCustomAudioData)
	{
		if (!useCustomAudioData)
			goto exausted;
		for (int i = 0; i < customAudioData.Length; i++)
			if (customAudioData[i].AudioClipLink.ContainsKey(clip))
				return GetCoroutine(customAudioData[i].AudioClipLink[clip]);
		exausted:
		return GetCoroutine(new CustomAudioClip(clip) { audioMixerGroup = group });
	}

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
		for (int i = 0; i < customAudioData.Length; i++)
			if (customAudioData[i].StringLink.ContainsKey(soundPath))
				return customAudioData[i].StringLink[soundPath];
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

	/// <summary> Stops a sound from playing over a span of time </summary>
	/// <param name ="soundName"> the soundFile name to stop </param>
	/// <param name ="fadeOutSeconds"> the timespan so volume would hit from 1 to 0 </param> 
	public void StopSoundViaFade(string soundName, float fadeOutSeconds)
	{
		for (int i = 0; i < customAudioData.Length; i++)
			if (customAudioData[i].StringLink.ContainsKey(soundName))
				SoundCoroutineCache[customAudioData[i].StringLink[soundName]].Stop(fadeOutSeconds);
		throw new KeyNotFoundException(soundName);
	}

	/// <summary> Stops a sound via the filename of the sound. </summary>
	/// <param name ="soundName"> the soundFile name to stop. </param>
	public void StopSound(string soundName)
	{
		for (int i = 0; i < customAudioData.Length; i++)
			if (customAudioData[i].StringLink.ContainsKey(soundName))
				SoundCoroutineCache[customAudioData[i].StringLink[soundName]].Stop();
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


	// IEnumerable Implementation


	public IEnumerator<SoundCoroutine> GetEnumerator()
	{
		foreach (SoundCoroutine sound in SoundCoroutineCache.Values)
			yield return sound;
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}