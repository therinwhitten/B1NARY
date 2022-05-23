using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMaster : MonoBehaviour
{
    // All sounds are played at the source of the audioListener.
    [SerializeField] private AudioListener audioListener;
    public AudioListener AudioListener => audioListener;

    [SerializeField, ToolTip("This calculates the data when needed.")] 
    private CustomAudioClipArray customAudioData;

    /// <summary>
    ///     <para>Plays a sound.</para>
    ///     <param name ="clip">the audioclip to play.</param>
    ///     <param name ="useCustomAudioData">
    ///         if the <see ="member">audioClip</see> is found in 
    ///         customAudioData, then it will play that instead.
    ///     </param>
    ///     <returns>
    ///         Optionally returns a <see ="member">SoundCoroutine</see>, may not 
    ///         be needed to function
    ///     </returns>
    /// </summary>
    public SoundCoroutine PlaySound(AudioClip clip, bool useCustomAudioData = true)
    {
        if (useCustomAudioData)
            for (int i = 0; i < customAudioData.Length; i++)
                if (clip == customAudioData[i])
                {
                    PlaySound(i);
                    return;
                }
        var activeSound = new SoundCoroutine(audioListener, clip);
        activeSound.Play();
        return activeSound;
    }

    /// <summary>
    ///     <para>
    ///         Plays a sound specified in the customAudioData array via the index
    ///     </para>
    ///     <param name ="index">Index for customAudioData array</param>
    ///     <returns>
    ///         Optionally returns a <see ="member">Coroutine</see>, may not 
    ///         be needed to function
    ///     </returns>
    /// </summary>
    public SoundCoroutine PlaySound(int index) 
    {
        var activeSound = new SoundCoroutine(audioListener, customAudioData[index]);
        activeSound.Play();
        return activeSound;
    }


    private Dictionary<AudioClip, SoundCoroutine> oneShotData 
        = new Dictionary<AudioClip, SoundCoroutine>();
    /// <summary>
    ///     <para>Play a sound that is meant to be repeated multiple times</para>
    ///     
    /// </summary>
    public SoundCoroutine PlayOneShot(AudioClip clip)
    {
        if (!oneShotData.ContainsKey(clip))
            oneShotData.Add(clip, new SoundCoroutine(audioListener, clip));
        oneShotData[clip].PlayShot();
        return oneShotData[clip];
    }
}