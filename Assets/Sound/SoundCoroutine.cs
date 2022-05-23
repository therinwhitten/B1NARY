using System;
using System.Collections;
using UnityEngine;

public class SoundCoroutine
{
    public readonly AudioSource audioSource;
    private Coroutine coroutine = null;
    public bool destroyOnFinish = false, hasPlayed = false;

    public SoundCoroutine(AudioListener audioListener, AudioClip audioClip)
        : this(audioListener, (CustomAudioClip)audioClip) {}
    public SoundCoroutine(AudioListener audioListener, CustomAudioClip audioClip)
    {
        audioSource = audioListener.transform.AddComponent<AudioSource>();
        SetAudioClip(audioClip);
    }
    public void SetAudioClip(AudioClip audioClip) 
        => SetAudioClip((CustomAudioClip)audioClip);
    public void SetAudioClip(CustomAudioClip audioClip)
    {
        audioSource.clip = clip;

        // This needs testing!
        var random = new System.Random();
        audioSource.pitch = (clip.pitch - (clip.pitchVariance * 3)) 
            + ((random.NextDouble() * clip.pitchVariance) * 3);
        audioSource.volume = (clip.volume - clip.volumeVariance)
            + (random.NextDouble() * clip.volumeVariance);
    }

    public void PlaySingle()
    {
        if (hasPlayed)
            throw new Exception();
        hasPlayed = true;
        audioSource.Play();
        coroutine ??= audioSource.StartCoroutine(CloseCoroutineAwaiter())
    }
    public void PlayShot()
    {
        if (hasPlayed)
            throw new Exception();
        audioSource.PlayOneShot(audioSource.clip, audioSource.volume);
        coroutine ??= audioSource.StartCoroutine(CloseCoroutineAwaiter());
    }
    private IEnumerator CloseCoroutineAwaiter()
        {
            // Better performance than checking every frame or half second
            Func<dynamic> classCreator = audioSource.Clip > 10 ? 
                () => new WaitForSeconds(0.5f) :
                () => new WaitForEndOfFrame();
            while (audioSource.IsPlaying)
                yield return classCreator.Invoke();
            Stop();
        }

    public void Stop()
    {
        GameObject.StopCoroutine(coroutine);
        coroutine = null;
        if (destroyOnFinish)
            Finalize();
    }
    ~SoundCoroutine()
    {
        GameObject.Destroy(audioSource);
    }
}