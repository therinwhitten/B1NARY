using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;

/// <summary>Handles audio for efficiency and garbage collection, uses 
/// <see cref="CustomAudioClip"/> and/or <see cref="UnityCustomAudioClip"/> for 
/// custom files and uses <see cref="SoundCoroutine"/> for code to change it 
/// themselves.</summary>
public class AudioHandler : SingletonAlt<AudioHandler>
{
	public VoiceActorHandler VoiceActorHandler { get; private set; }

	/// <summary> Custom sounds for each scene, may not be used if 
	/// audiofiles are called directly or via file in resources folder </summary>
	public SoundLibrary CustomAudioData { get; private set; }

	/// <summary> Cache for storing automated sound data </summary>
	public Dictionary<AudioClip, SoundCoroutine> SoundCoroutineCache { get; private set; }
		= new Dictionary<AudioClip, SoundCoroutine>();


	protected override void SingletonStart()
	{
		VoiceActorHandler = new VoiceActorHandler(this);
		LoadNewLibrary(null, SceneManager.GetActiveScene().name);
		MakeNewAudioHandler(null, EventArgs.Empty);
		PlayOnAwakeCommands();
		//GameCommands.SwitchedScenes += OnSceneChange;
	}
	private void PlayOnAwakeCommands()
	{
		if (!CustomAudioData.ContainsPlayOnAwakeCommands)
			return;
		foreach (CustomAudioClip clip in CustomAudioData.PlayOnAwakeCommands)
			PlaySound(clip);
	}

	private void MakeNewAudioHandler(object sender, EventArgs args)
	{
		VoiceActorHandler = new VoiceActorHandler(this);
		StartCoroutine(Delay());
		IEnumerator Delay()
		{ 
			yield return new WaitForEndOfFrame(); 
			GameCommands.SwitchingScenes += MakeNewAudioHandler; 
		}
	}

	public void LoadNewLibrary(object sender, string sceneName)
	{
		const string fileDirectory = "Sounds/Sound Libraries";
		CustomAudioData = Resources.Load<SoundLibrary>($"{fileDirectory}/{sceneName}");
		if (CustomAudioData == null)
			Debug.LogError("There are no detected sound libraries in" +
				$" resource folder : {fileDirectory}/{sceneName}!");

		PlayOnAwakeCommands();
		StartCoroutine(Buffer());
		IEnumerator Buffer()
		{ 
			yield return new WaitForEndOfFrame();
			GameCommands.SwitchedScenes += LoadNewLibrary; 
			var enumerable = SoundCoroutineCache
				.Where(pair => !pair.Value.AudioClip.fadeWhenTransitioning && 
				!CustomAudioData.ContainsCustomAudioClip(pair.Key));
			if (enumerable.Any())
			{
				var argumentBuilder = new StringBuilder($"Total Exceptions: {enumerable.Count()}");
				foreach (var pair in enumerable)
					argumentBuilder.AppendLine($"Although '{pair.Key.name}' is allowed"
						+ $" to transition scenes, library '{CustomAudioData.name}'" +
						" doesn't contain it! ");
				if (GamePreferences.GetBool(GameCommands.exceptionLoadName, true))
					throw new InvalidOperationException(argumentBuilder.ToString());
				else
					Debug.LogWarning(argumentBuilder);
			}
			foreach (KeyValuePair<AudioClip, SoundCoroutine> item in enumerable)
				if (item.Value.AudioClip.fadeWhenTransitioning)
					item.Value.Stop();
		}
	}

	public void ChangeFloat(Ref<float> value, float final, float secondsTaken)
	{
		float difference = value.Value - final;
		Func<float, float, bool> condition = difference > 0 ?
			(Func<float, float, bool>)((current, final2) => current > final2) :
			(Func<float, float, bool>)((current, final2) => current < final2);
		StartCoroutine(Coroutine(final));
		IEnumerator Coroutine(float finalValue)
		{
			while (value.Value != finalValue)
			{
				float change = (Time.deltaTime / secondsTaken) * difference;
				if (condition.Invoke(value.Value, finalValue))
				{
					value.Value = finalValue;
					yield break;
				}
				else
					value.Value += change;
				yield return new WaitForEndOfFrame();
			}
		}
	}


	private Func<SoundCoroutine> GetCoroutine(AudioClip clip, AudioMixerGroup group,
		bool useCustomAudioData)
	{
		if (!useCustomAudioData)
			goto DoomVisualNovelWhen;
		if (CustomAudioData.ContainsCustomAudioClip(clip))
			return GetCoroutine(CustomAudioData.GetCustomAudioClip(clip));
		DoomVisualNovelWhen:
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
			SoundCoroutineCache.Add(key, new SoundCoroutine(this, CustomAudioData.name, key.audioMixerGroup, clip: key));
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
		if (CustomAudioData.ContainsAudioClip(soundPath))
			return CustomAudioData.GetAudioClip(soundPath);
		Debug.Log($"'{soundPath}' is not found in {nameof(CustomAudioData)},"
			+ " searching via filePath");
		// filePath
		AudioClip audioClip = Resources.Load<AudioClip>(soundPath);
		if (audioClip == null)
			throw new SoundNotFoundException($"{soundPath} does not lead to a sound" +
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
		if (CustomAudioData.ContainsAudioClip(soundName))
		{
			SoundCoroutineCache[CustomAudioData.GetAudioClip(soundName)].Stop(fadeOutSeconds);
			return;
		}
		// All the memory cross-scenes gets dumped when loading, so manually
		// - searching for info in linq 
		Debug.Log($"DEBUG: {soundName} not found in original sound library. Searching via clipAudios.");
		IEnumerable<AudioClip> clipAudio = 
			SoundCoroutineCache.Values.Select(coroutine => coroutine.AudioClip.clip);
		IEnumerable<string> stringAudio = clipAudio.Select(clip => clip.name.Trim());
		if (stringAudio.Contains(soundName))
		{
			int index = Array.IndexOf(stringAudio.ToArray(), soundName);
			SoundCoroutineCache[clipAudio.ElementAt(index)].Stop(true);
			return;
		}
		throw new KeyNotFoundException(soundName);
	}

	/// <summary> Stops a sound via the filename of the sound. </summary>
	/// <param name ="soundName"> the soundFile name to stop. </param>
	public void StopSound(string soundName)
	{
		if (CustomAudioData.ContainsAudioClip(soundName))
			SoundCoroutineCache[CustomAudioData.GetAudioClip(soundName)].Stop(true);
		else
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
}