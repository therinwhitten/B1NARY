using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : Singleton<AudioManager>
{
    public AudioMixer Audio;

    public Sound[] sounds;

    private Dictionary<String, AudioSource> sources;
    private Dictionary<String, Sound> lib;
    String lastSpeaker;
    AudioSource lastSource;
    private Dictionary<String, IEnumerator> threads;
    private void Awake()
    {
        sources = new Dictionary<string, AudioSource>();
        lib = new Dictionary<string, Sound>();
        threads = new Dictionary<String, IEnumerator>();
        foreach (Sound s in sounds)
        {
            lib.Add(s.name, s);
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = s.clip;
            source.loop = s.loop;
            source.playOnAwake = s.playOnAwake;
            source.outputAudioMixerGroup = s.mixerGroup;
            source.volume = s.volume;
            sources.Add(s.name, source);
            if (source.playOnAwake)
                source.Play();
        }
    }
    public void FadeIn(string sound, float duration)
    {
        AudioSource source = sources[sound];
        if (source == null)
        {
            Debug.LogWarning("Sound: " + sound + " not found!");
            return;
        }
        float targetVolume = lib[sound].volume;
        source.volume = 0f;
        source.Play();
        IEnumerator thread = StartFade(source, duration, targetVolume);
        SafeStartCoroutine(source.clip.name, thread);
    }
    public void FadeOut(string sound, float duration)
    {
        AudioSource source = sources[sound];
        if (source == null)
        {
            Debug.LogWarning("Sound: " + sound + " not found!");
            return;
        }
        float targetVolume = 0f;
        IEnumerator thread = StartFade(source, duration, targetVolume);
        SafeStartCoroutine(source.clip.name, thread);
    }

    public void Play(string sound, bool unique = true)
    {
        AudioSource source = sources[sound];
        if (source == null)
        {
            Debug.LogWarning("Sound: " + sound + " not found!");
            return;
        }
        if (unique)
        {
            source.Stop();
        }
        source.Play();
    }
    public override void initialize()
    {

    }


    IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume)
    {
        Debug.Log("Fading sound: " + audioSource.clip.name);
        float currentTime = 0;
        float start = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        if (audioSource.volume == 0)
        {
            audioSource.Stop();
        }
        if (audioSource.volume == targetVolume)
        {
            threads[audioSource.clip.name] = null;
        }
        yield break;
    }

    public void PlayVoice(String name, float volume, AudioSource source, AudioClip clip)
    {
        if (lastSpeaker == name || lastSpeaker == null)
        {
            IEnumerator thread = InterruptVoice(source, volume, clip);
            SafeStartCoroutine(name, thread);
            lastSpeaker = name;
            lastSource = source;
        }
        else
        {
            IEnumerator thread = InterruptVoice(lastSource, volume, null);
            SafeStartCoroutine(lastSpeaker, thread);

            thread = InterruptVoice(source, volume, clip);
            SafeStartCoroutine(name, thread);
            lastSpeaker = name;
            lastSource = source;
        }
    }
    IEnumerator InterruptVoice(AudioSource source, float volume, AudioClip newClip)
    {
        float currentTime = 0;
        float start = volume;
        while (currentTime < 0.1f)
        {
            currentTime += Time.deltaTime;
            source.volume = Mathf.Lerp(start, 0, currentTime / 0.1f);
            yield return null;
        }
        if (source.volume == 0 && newClip != null)
        {
            source.Stop();
            source.clip = newClip;
            source.volume = start;
            source.Play();
        }
        yield break;
    }

    private void SafeStartCoroutine(String name, IEnumerator thread)
    {
        try
        {
            if (threads[name] == null)
            {
                threads[name] = thread;
                StartCoroutine(thread);
            }
            else
            {
                StopCoroutine(threads[name]);
                threads[name] = thread;
                StartCoroutine(thread);
            }
        }
        catch (KeyNotFoundException)
        {
            threads.Add(name, thread);
            StartCoroutine(thread);
        }

    }



    // This looks stupid but it's the simplest way to bind sliders from the UI
    public void setMasterVolume(float volume)
    {
        Audio.SetFloat("Master", Mathf.Log10(volume) * 20);
    }
    public void setSFXVolume(float volume)
    {
        Audio.SetFloat("SFX", Mathf.Log10(volume) * 20);
    }
    public void setMusicVolume(float volume)
    {
        Audio.SetFloat("Music", Mathf.Log10(volume) * 20);
    }
    public void setUIVolume(float volume)
    {
        Audio.SetFloat("UI", Mathf.Log10(volume) * 20);
    }
    public void setVoiceVolume(float volume)
    {
        Audio.SetFloat("Voice", Mathf.Log10(volume) * 20);
    }

}