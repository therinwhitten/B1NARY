using UnityEngine;

// This is a class as it provides scriptable-object functionality to an
// - ordinary array.
[CreateAssetMenu(menuName = "Audio Manager/Custom Audio Clip List", order = 1)]
public class CustomAudioClipArray : ScriptableObject
{
	public static implicit operator CustomAudioClip[](CustomAudioClipArray ar)
		=> ar.data;
	public CustomAudioClip[] data;
}