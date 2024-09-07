namespace B1NARY
{
	using B1NARY;
	using System;
	using UnityEngine;

	[CreateAssetMenu(fileName = "New Dynamic Audio Clip", menuName = "Dynamic Audio Clip")]
	public class DynamicAudioClip : ScriptableObject
	{
		public RandomForwarder.RandomType randomType;
		public AudioClip[] audioClips;
		public void Play(AudioSource source, float volume = 1f)
		{
			source.PlayOneShot(audioClips.Random(randomType), volume);
		}
		public void Play(AudioSource source, int index, float volume = 1f)
		{
			source.PlayOneShot(audioClips[index], volume);
		}
	}
	public static class DynamicAudioSource
	{
		public static void Play(this AudioSource source, AudioClip clip)
		{
			source.Stop();
			source.clip = clip;
			source.Play();
		}
		public static void Play(this AudioSource source, DynamicAudioClip clip)
		{
			source.Stop();
			source.clip = clip.audioClips.Random();
			source.Play();
		}
		public static void PlayOneShot(this AudioSource source, DynamicAudioClip clip)
		{
			source.PlayOneShot(clip.audioClips.Random());
		}
		public static void PlayOneShot(this AudioSource source, DynamicAudioClip clip, float volume)
		{
			source.PlayOneShot(clip.audioClips.Random(), volume);
		}
	}
}