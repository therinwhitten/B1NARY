using UnityEngine;
using UnityEngine.Audio;

/// <summary>
///		<para>
///			Struct which allows users to customize their own settings for specific 
///			sounds when played by <see cref="AudioHandler"/>. Note playing 
///			sounds tend to not require this class.
///		</para>
///		<para>See <see cref="CustomAudioClip"/> for code data version</para>
///	</summary>
[CreateAssetMenu(menuName = "B1NARY/Custom Audio Clip", order = 0)]
public class UnityCustomAudioClip : ScriptableObject
{
	public static explicit operator CustomAudioClip(UnityCustomAudioClip clip)
		=> new CustomAudioClip(clip.audioClip)
		{
			pitch = clip.pitch,
			pitchVariance = clip.pitchVariance,
			volume = clip.volume,
			volumeVariance = clip.volumeVariance,
			audioMixerGroup = clip.audioMixerGroup,
			loop = clip.loop,
			fadeWhenTransitioning = clip.fadeWhenTransitioning,
			willFadeWhenTransitioning = clip.willFadeWhenTransitioning,
			playOnAwake = clip.playOnAwake,
		};

	public AudioClip audioClip;
	[Range(0, 1)] public float volume = 1;
	[Range(0, 1)] public float volumeVariance = 0;
	[Range(0, 3)] public float pitch = 1;
	[Range(0, 1)] public float pitchVariance = 0;
	public AudioMixerGroup audioMixerGroup;
	public bool loop = false, playOnAwake = false;
	[Header("Scene Transitions")] public bool willFadeWhenTransitioning = true;
	[Range(0, 3)] public float fadeWhenTransitioning = 0.5f;
}