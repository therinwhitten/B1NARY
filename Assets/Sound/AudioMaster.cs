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
	public Coroutine PlaySound(AudioClip clip, bool useCustomAudioData = true)
	{
		if (useCustomAudioData && customAudioData != null)
			for (int i = 0; i < customAudioData.data.Length; i++)
				if (clip == customAudioData.data[i])
					return PlaySound(i);
		var activeSound = StartCoroutine(CoroutineYesYes((CustomAudioClip)clip));
		return activeSound;
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
	public Coroutine PlaySound(int index) 
	{
		var activeSound = StartCoroutine(CoroutineYesYes((CustomAudioClip)customAudioData.data[index]));
		return activeSound;
	}

	private IEnumerator CoroutineYesYes(CustomAudioClip clip)
	{
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.clip = clip;

		// This needs testing!
		var random = new System.Random();
		audioSource.pitch = (float)(clip.pitch - (clip.pitchVariance * 3)
			+ (random.NextDouble() * clip.pitchVariance * 3));
		audioSource.volume = (float)(clip.volume - clip.volumeVariance
			+ (random.NextDouble() * clip.volumeVariance));

		audioSource.Play();
		while (audioSource.isPlaying)
			yield return new WaitForFixedUpdate();
		Destroy(audioSource);
		yield break;
	}




	private Dictionary<AudioClip, SoundCoroutine> oneShotData 
		= new Dictionary<AudioClip, SoundCoroutine>();
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
		if (!oneShotData.ContainsKey(clip))
		{
			oneShotData.Add(clip, new SoundCoroutine(audioListener, clip, this));
			oneShotData[clip].GarbageCollection += (sender, args) => 
				{ oneShotData.Remove(clip); };
		}
		oneShotData[clip].PlayShot();
		return oneShotData[clip];
	}
}