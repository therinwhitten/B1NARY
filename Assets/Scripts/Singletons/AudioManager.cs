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


    private void Awake()
    {
        sources = new Dictionary<string, AudioSource>();
        lib = new Dictionary<string, Sound>();
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
        StartCoroutine(StartFade(source, duration, targetVolume));
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
        StartCoroutine(StartFade(source, duration, targetVolume));
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
        yield break;
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