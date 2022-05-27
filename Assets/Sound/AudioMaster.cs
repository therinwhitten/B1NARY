using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioListener))]
public class AudioMaster : MonoBehaviour
{
	private AudioListener audioListener;

	// Theoretically, the end user will create a file in the directory which 
	// - contains all custom audio files between scenes, like a drag-and-drop.
	// - That way, it would be easier to both have it keep it on load or
	// - delete it per scene switch.
	[SerializeField, Tooltip("This calculates the data when needed.\n"
		+ "Can be nullable")] 
	private CustomAudioClipArray customAudioData;

	private Dictionary<AudioClip, SoundCoroutine> soundCoroutineCache
		= new Dictionary<AudioClip, SoundCoroutine>();
	
	// Returning a soundcoroutine may cause reference issues, so may do 
	// - Func<SoundCoroutine> instead.
	private SoundCoroutine GetCoroutine(AudioClip Key)
	{
		if (!soundCoroutineCache.ContainsKey(Key))
		{
			soundCoroutineCache.Add(Key, new SoundCoroutine(this, (CustomAudioClip)clip));
			soundCoroutineCache[clip].GarbageCollection += (sender, empty) =>
				{ soundCoroutineCache.Remove(clip); };
		}
		return soundCoroutineCache[Key]; 
	}

	private void Start()
	{
		audioListener = GetComponent<AudioListener>();
	}

	///	<summary>
	///		<para>Plays a sound.</para>
	///		<param name ="clip">the audioclip to play.</param>
	///		<param name ="useCustomAudioData">
	///			if the <see ="member">audioClip</see> is found in 
	///			customAudioData, then it will play that instead.
	///		</param>
	///		<returns>
	///			Optionally returns a <see ="member">SoundCoroutine</see>, may not 
	///			be needed to function
	///		</returns>
	///	</summary>
	public SoundCoroutine PlaySound(AudioClip clip, bool useCustomAudioData = true)
	{
		if (useCustomAudioData && customAudioData != null)
			for (int i = 0; i < customAudioData.data.Length; i++)
				if (clip == customAudioData.data[i])
					return PlaySound(i);
		var sound = GetCoroutine(clip);
		sound.PlaySingle();
		return sound;
	}

	///	<summary>
	///		<para>
	///			Plays a sound specified in the customAudioData array via the index
	///		</para>
	///		<param name ="index">Index for customAudioData array</param>
	///		<returns>
	///			Optionally returns a <see ="member">Coroutine</see>, may not 
	///			be needed to function
	///		</returns>
	///	</summary>
	public SoundCoroutine PlaySound(int index) 
	{
		var sound = GetCoroutine(customAudioData[index]);
		sound.PlaySingle();
		return sound;
	}

	///	<summary>
	///		<para>Play a sound that is meant to be repeated multiple times</para>
	///		<param name = "clip">The audioClip to play.</param>
	///		<returns>
	///			Optionally returns a <see ="member">Coroutine</see>, may not 
	///			be needed to function
	///		</returns>
	///	</summary>
	public SoundCoroutine PlayOneShot(AudioClip clip)
	{
		var sound = GetCoroutine(clip);
		sound.PlayOneShot();
		return sound;
	}

	
}