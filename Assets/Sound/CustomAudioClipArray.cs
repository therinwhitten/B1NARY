using UnityEngine;

// This is a class as it provides scriptable-object functionality to an
// - ordinary array.
public class CustomAudioClipArray : ScriptableObject
{
    public static implicit operator CustomAudioClip[](CustomAudioClipArray ar)
        => ar.data;
    public CustomAudioClip[] data;
}