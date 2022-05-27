using UnityEngine;

// This provides a reference point so the AudioMaster, for example can easily
// customize custom audio
[CreateAssetMenu(menuName = "Audio Manager/Custom Audio Clip", order = 0)]
public class UnityCustomAudioClip : ScriptableObject
{
	public static explicit operator CustomAudioClip(UnityCustomAudioClip clip)
		=> new CustomAudioClip(clip.audioClip) { 
			pitch = clip.pitch, 
			pitchVariance = clip.pitchVariance, 
			volume = clip.volume, 
			volumeVariance = clip.volumeVariance
		};

	public AudioClip audioClip;
	[Range(0, 1)] public float volume = 1;
	[Range(0, 1)] public float volumeVariance = 0;
	[Range(0, 3)] public float pitch = 1; 
	[Range(0, 1)] public float pitchVariance = 0;
}
public struct CustomAudioClip
{
	// Having an actual comparable interface that is used by dictionaries and
	// - hashsets may work better than comparing strings. Be sure to implement
	// - that if learned.
	// TODO: #12 Add the IEqualityComparer<T> Interface!

	public static bool operator ==(CustomAudioClip left, AudioClip right)
		=> left.ToString() == right.ToString();
	public static bool operator !=(CustomAudioClip left, AudioClip right)
		=> !(left == right);
	public static implicit operator AudioClip(CustomAudioClip input)
		=> input.audioClip;
	public static explicit operator CustomAudioClip(AudioClip input)
		=> new CustomAudioClip(input);

	public CustomAudioClip(AudioClip audioClip)
	{
		this.audioClip = audioClip;
		volume = 1;
		volumeVariance = 0;
		pitch = 1;
		pitchVariance = 0;
	}

	public AudioClip audioClip;
	public float volume, volumeVariance, pitch, pitchVariance;

	// Having the tostring fowarded to audioClip is due how its simply an audioClip
	// - with special features.
	public override string ToString() => audioClip.ToString();

	public override bool Equals(object other)
	{
		if (other is CustomAudioClip clip)
			return this == clip;
		return false;
	}
	public override int GetHashCode() => audioClip.GetHashCode();
}