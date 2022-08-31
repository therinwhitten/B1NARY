namespace B1NARY.Audio
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Audio;
	using B1NARY.DesignPatterns;

	/// <summary>Handles audio for efficiency and garbage collection, uses 
	/// <see cref="CustomAudioClip"/> and/or <see cref="UnityCustomAudioClip"/> for 
	/// custom files and uses <see cref="AudioTracker"/> for code to change it 
	/// themselves.</summary>
	public partial class AudioHandler : SingletonAlt<AudioHandler>
	{
		public VoiceActorHandler VoiceActorHandler { get; private set; }

		/// <summary> Custom sounds for each scene, may not be used if 
		/// audiofiles are called directly or via file in resources folder </summary>
		public SoundLibrary CustomAudioData { get; private set; }

		/// <summary> Cache for storing automated sound data </summary>
		public Dictionary<AudioClip, AudioTracker> SoundCoroutineCache { get; private set; }

		[SerializeField]
		private string soundLibraryDirectory = "Sounds/Sound Libraries";


		protected override void SingletonAwake()
		{
			SoundCoroutineCache = new Dictionary<AudioClip, AudioTracker>();
			SceneManager.AddPersistentListener(HandleSceneSwitch);
			HandleSceneSwitch(SceneManager.CurrentScene);
		}

		/// <summary>
		/// Automatically handles scene switch automatically after first calling it.
		/// Mainly handles behaviour for <see cref="CustomAudioData"/>.
		/// </summary>
		/// <param name="newScene"></param>
		private void HandleSceneSwitch(string newScene = null)
		{
			LoadNewLibrary(newScene);
			VoiceActorHandler = new VoiceActorHandler();
		}

		/// <summary>
		/// Plays all sounds in the current <see cref="SoundLibrary"/> that has
		/// <see cref="CustomAudioClip.playOnAwake"/> enabled.
		/// </summary>
		void PlayOnAwakeCommands()
		{
			if (!CustomAudioData.ContainsPlayOnAwakeCommands)
				return;
			foreach (CustomAudioClip clip in CustomAudioData.PlayOnAwakeCommands)
				PlaySound(clip);
		}
		/// <summary>
		/// Sets the value of <see cref="CustomAudioData"/> based on 
		/// <paramref name="sceneName"/>. Complains if there is no audio library with
		/// the name.
		/// </summary>
		/// <remarks> Mainly used by <see cref="HandleSceneSwitch(string)"/> by default. </remarks>
		/// <param name="sceneName"> The current scene name. </param>
		/// <exception cref="AggregateException"></exception>
		private void LoadNewLibrary(string sceneName)
		{
			CustomAudioData = Resources.Load<SoundLibrary>($"{soundLibraryDirectory}/{sceneName}");
			if (CustomAudioData == null)
				Debug.LogError("There are no detected sound libraries in" +
					$" resource folder : {soundLibraryDirectory}/{sceneName}!");
			else
				PlayOnAwakeCommands();
			IEnumerable<AudioTracker> enumerable =
				from soundCoroutine in SoundCoroutineCache.Values
				where !soundCoroutine.AudioClip.destroyWhenTransitioningScenes
				where !soundCoroutine.IsStopping
				where CustomAudioData.ContainsCustomAudioClip(soundCoroutine.AudioClip) == false
				select soundCoroutine;
			if (enumerable.Any())
			{
				IEnumerable<InvalidOperationException> exceptions = enumerable
					.Select(audioTracker => new InvalidOperationException(
						$"\nAlthough '{audioTracker.AudioClip.Name}' is allowed"
						+ $" to transition scenes, library '{CustomAudioData.name}'"
						+ " doesn't contain it! "));
				if (GamePreferences.GetBool(GameCommands.exceptionLoadName, true))
					throw new AggregateException(exceptions);
				CoroutinePointer[] removeArray = enumerable
					.Select<AudioTracker, CoroutinePointer>(pointer => () => pointer).ToArray();
				for (int i = 0; i < removeArray.Length; i++)
					removeArray[i]().Stop(true);
				Debug.LogWarning(nameof(AudioHandler) + ": Forcefully stopping sounds:\n" +
					string.Join("\n", removeArray.Select(pointer => pointer().AudioClip.Name)));
			}
		}


		private CoroutinePointer GetCoroutine(AudioClip clip, AudioMixerGroup group,
			bool useCustomAudioData)
		{
			if (useCustomAudioData && CustomAudioData.ContainsCustomAudioClip(clip))
				return GetCoroutine(CustomAudioData.GetCustomAudioClip(clip));
			return GetCoroutine(new CustomAudioClip(clip) { audioMixerGroup = group });
		}

		/// <summary>Easily create a pointer and log it in to the cache.</summary>
		/// <param name="key">AudioClip to put into the soundCoroutine</param>
		/// <returns>
		///		<see cref="AudioTracker"/> that is meant to be kept track of and 
		///		used for accessibility of handling sound.
		///	</returns>
		public CoroutinePointer GetCoroutine(CustomAudioClip key)
		{
			if (!SoundCoroutineCache.ContainsKey(key))
			{
				SoundCoroutineCache.Add(key, GetSoundCoroutineData());
				SetGarbageCollection();
			}
			return () => SoundCoroutineCache[key];

			AudioTracker GetSoundCoroutineData() => new AudioTracker(
				this, CustomAudioData.name, key.audioMixerGroup, clip: key);
			void SetGarbageCollection() =>
				SoundCoroutineCache[key].GarbageCollection = () =>
					GarbageCollectionDefault(key);
		}
		private void GarbageCollectionDefault(AudioClip clip)
		{
			if (!SoundCoroutineCache.ContainsKey(clip))
				return;
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
			Debug.Log(nameof(AudioHandler) + $": '{soundPath}' is not found in" +
				$" {nameof(CustomAudioData)}, searching via filePath");
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
		///		Optionally a <see cref="AudioTracker"/>, may not be 
		///		needed to function.
		///	</returns>
		public CoroutinePointer PlaySound(AudioClip clip,
			AudioMixerGroup mixerGroup = null, bool useCustomAudioData = true)
		{
			CoroutinePointer sound = GetCoroutine(clip, mixerGroup, useCustomAudioData);
			sound().PlaySingle();
			return sound;
		}

		///	<summary>Plays a sound with custom data. Ignores soundCoroutineCache</summary>
		///	<param name ="clip">the audioclip to play.</param>
		///	<returns>
		///		Optionally a <see cref="AudioTracker"/>, may not be 
		///		needed to function.
		///	</returns>
		public CoroutinePointer PlaySound(CustomAudioClip clip)
		{
			CoroutinePointer sound = GetCoroutine(clip);
			sound().AudioClip = clip;
			sound().PlaySingle();
			return sound;
		}

		///	<summary>Searches a sound in the resources folder via filePath, then plays it.</summary>
		///	<param name ="soundPath">the audioclip savePath in the resource folder to play.</param>
		///	<param name ="useCustomAudioData">
		///		if the <see cref="AudioClip"/> is found in 
		///		customAudioData, then it will play that instead.
		///	</param>
		///	<returns>
		///		Optionally a <see cref="AudioTracker"/>, may not be 
		///		needed to function.
		///	</returns>
		public CoroutinePointer PlaySound(string soundPath,
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
		///		Optionally a <see cref="AudioTracker"/>, may not be 
		///		needed to function.
		///	</returns>
		public CoroutinePointer PlayFadedSound(AudioClip clip, float fadeInSeconds,
			AudioMixerGroup mixerGroup = null, bool useCustomAudioData = true)
		{
			CoroutinePointer sound = GetCoroutine(clip, mixerGroup, useCustomAudioData);
			sound().PlaySingle(fadeInSeconds);
			return sound;
		}

		///	<summary>Searches a sound in the resources folder via filePath, then fades it in.</summary>
		///	<param name ="soundPath">the audioclip savePath in the resource folder to play.</param>
		///	<param name="fadeInSeconds">Seconds which it goes from 0 to 1.</param>
		///	<param name ="useCustomAudioData">
		///		if the <see cref="AudioClip"/> is found in 
		///		customAudioData, then it will play that instead.
		///	</param>
		///	<returns>
		///		Optionally a <see cref="AudioTracker"/>, may not be 
		///		needed to function.
		///	</returns>
		public CoroutinePointer PlayFadedSound(string soundPath, float fadeInSeconds,
			AudioMixerGroup mixerGroup = null, bool useCustomAudioData = true)
		{
			AudioClip audioClip = GetRawAudioClip(soundPath);
			return PlayFadedSound(audioClip, fadeInSeconds, mixerGroup, useCustomAudioData);
		}

		public void StopSoundViaFade(AudioClip sound, float fadeOutSeconds)
			=> SoundCoroutineCache[sound].Stop(fadeOutSeconds);

		/// <summary> Stops a sound from playing over a span of time </summary>
		/// <param name ="soundName"> the soundFile name to stop </param>
		/// <param name ="fadeOutSeconds"> the timespan so volume would hit from 1 to 0 </param> 
		public void StopSoundViaFade(string soundName, float fadeOutSeconds)
		{
			if (CustomAudioData.ContainsAudioClip(soundName))
			{
				AudioClip stopClip = CustomAudioData.GetAudioClip(soundName);
				if (SoundCoroutineCache.TryGetValue(stopClip, out var soundCoroutine))
					if (soundCoroutine.IsStopping)
					{
						Debug.LogError($"{nameof(AudioTracker)}: '{soundCoroutine.AudioClip.Name}' "
							+ "already is fading! Terminating.\nTry using one of them"
							+ " so they won't conflict");
						return;
					}
				SoundCoroutineCache[stopClip].Stop(fadeOutSeconds);
				return;
			}
			// All the memory cross-scenes gets dumped when loading, so manually
			// - searching for info in linq 
			Debug.LogWarning($"{soundName} not found in original sound library." +
				"\nKeep in mind if you want to mess with sounds cross-scenes, " +
				"list them in the sound library! This is performance heavy!");
			soundName = soundName.Trim();
			CustomAudioClip comparator = null;
			foreach (var pair in SoundCoroutineCache)
			{
				if (pair.Value.AudioClip.Name != soundName)
					continue;
				comparator = pair.Value.AudioClip;
				break;
			}
			if (comparator == null)
				throw new KeyNotFoundException(soundName);
			SoundCoroutineCache[(AudioClip)comparator].Stop(true);
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
		///		Optionally a <see cref="AudioTracker"/>, may not be 
		///		needed to function.
		///	</returns>
		public CoroutinePointer PlayOneShot(AudioClip clip,
			AudioMixerGroup mixerGroup = null, bool useCustomAudioData = true)
		{
			CoroutinePointer sound = GetCoroutine(clip, mixerGroup, useCustomAudioData);
			sound().PlayOneShot();
			return sound;
		}

		private void OnDestroy()
		{
			SceneManager.RemoveListener(HandleSceneSwitch);
		}
	}
	public delegate AudioTracker CoroutinePointer();
}