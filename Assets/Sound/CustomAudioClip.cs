using UnityEngine;

// This provides a reference point so the AudioMaster, for example can easily
// customize custom audio
public class CustomAudioClip : ScriptableObject
{
    // Having an actual comparable interface that is used by dictionaries and
    // - hashsets may work better than comparing strings. Be sure to implement
    // - that if learned.
    // TODO: Add the IEqualityComparer<T> Interface!
    public static bool operator ==(CustomAudioClip left, AudioClip right)
        => left.ToString() == right.ToString();
    public static bool operator !=(CustomAudioClip left, AudioClip right)
        => !(left == right);
    public static implicit operator AudioClip(CustomAudioClip input)
        => input.audioClip;
    public static explicit operator CustomAudioClip(AudioClip input)
        => new CustomAudioClip() { audioClip = input };


    public AudioClip audioClip;
    [Range(0, 1)] public float volume = 1;
    [Range(0, 1)] public float volumeVariance = 0;
    [Range(0, 3)] public float pitch = 1; 
    [Range(0, 1)] public float pitchVariance = 0;

    // Having the tostring fowarded to audioClip is due how its simply an audioClip
    // - with special features.
    public override string ToString() => audioClip.ToString();
}