﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioCrossSceneTransferer : MonoBehaviour
{
	// This was a nightmare to fix ffs.
	private SoundCoroutine[] audioCoroutines = null;
	private Dictionary<string, SoundCoroutine> lookupCoroutine;
	public SoundCoroutine[] AudioCoroutines
	{
		get => audioCoroutines;
		set
		{
			if (audioCoroutines != null)
				throw new InvalidOperationException(nameof(audioCoroutines) +
					" has already been set!");
			audioCoroutines = value;
		}
	}

	private void Start()
	{
		DontDestroyOnLoad(this);
	}

	public void StartSceneTransferer()
	{
		if (audioCoroutines == null || audioCoroutines.Length == 0)
			throw new ArgumentNullException($"No {nameof(SoundCoroutine)}s are passed!");
		for (int i = 0; i < audioCoroutines.Length; i++)
		{
			float time = audioCoroutines[i].AudioSource.time;
			CustomAudioClip clip = audioCoroutines[i].ExtraDataAudioClip;
			audioCoroutines[i] = new SoundCoroutine(this, clip.audioMixerGroup, clip);
			audioCoroutines[i].AudioSource.time = time;
			audioCoroutines[i].GarbageCollection += (sender, args) => UpdatedValue(args);
			audioCoroutines[i].PlaySingle();
			if (clip.willFadeWhenTransitioning)
			{
				if (clip.fadeWhenTransitioning > 0)
					audioCoroutines[i].Stop(clip.fadeWhenTransitioning);
				else
					audioCoroutines[i].Stop();
			}
		}
		lookupCoroutine = audioCoroutines.ToDictionary(sound => sound.AudioClip.audioClip.name);


		//lookupCoroutine = audioCoroutines.ToDictionary(coroutine => coroutine.AudioClip.audioClip);

		if (!gameObject.TryGetComponent<AudioSource>(out _))
			Destroy(gameObject); // In case if all files are deleted instantly
		else Debug.Log($"{nameof(AudioCrossSceneTransferer)} started with {audioCoroutines.Length} audioSource(s)!");
	}
	private void UpdatedValue(AudioClip clip)
	{
		if (!lookupCoroutine.ContainsKey(clip.name))
			Debug.LogWarning($"bruh {clip}");
		Destroy(lookupCoroutine[clip.name].AudioSource);
		StartCoroutine(DuctTapeFix());
	}
	IEnumerator DuctTapeFix()
	{
		yield return new WaitForEndOfFrame();
		if (!gameObject.TryGetComponent<AudioSource>(out _))
			Destroy(gameObject);
	}

}