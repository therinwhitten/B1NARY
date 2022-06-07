using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AudioPointer = System.Func<CustomAudioClip>;

/// <summary>
///     A group of sounds customized by the user, can be used separately 
///     or multiple.
/// </summary>
public class SoundConfiguration : ScriptableObject
{
    [SerializeField] private CustomAudioClip[] customAudioClips;
    public AudioPointer[] CustomAudioClips { get
        {
            AudioPointer[] output = new AudioPointer[customAudioClips.Length];
            for (int i = 0; i < output.Length; i++)
                output[i] = () => customAudioClips;
            return output;
        }
    }
    public AudioPointer this[int index] => () => customAudioClips[i];
    public int Length => customAudioClips.Length;

    // Extra Configurations, Functions are used since creating multiple 
    // - instances may impact performance more than just using delegates
    private Dictionary<Func<AudioClip>, AudioPointer> audioClipLink = null;
    public IReadonlyDictionary<Func<AudioClip>, AudioPointer> AudioClipLink { get 
        {
            if (audioClipLink == null)
            {
                audioClipLink = new Dictionary<Func<AudioClip>, AudioPointer>();
                for (int i = 0; i < customAudioClips[i]; i++)
                    audioClipLink.Add(() => customAudioClips[i].audioClip, 
                        () => customAudioClips[i]);
            }
            return audioClipLink;
        }
    }

    private Dictionary<string, Func<AudioClip>> stringLink = null;
    public IReadonlyDictionary<string, Func<AudioClip>> StringLink { get 
        {
            if (stringLink == null)
            {
                stringLink = new Dictionary<string, AudioClip>();
                for (int i = 0; i < customAudioClips.Length; i++)
                    stringLink.Add(customAudioClips[i].audioClip.name, 
                        () => customAudioClips[i].audioClip);
            }
            return stringLink;
        }
    }
}