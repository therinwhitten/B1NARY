using UnityEngine;

public class VoiceActorHandler
{
	public string LastSpeaker { get; private set; } = string.Empty;
	public AudioSource audioSource;
	public VoiceActorHandler()
	{

	}

	public void StopVoice()
	{
		if (audioSource != null)
			audioSource.Stop();
	}
	public void PlayVoice(string name, float volume, AudioSource source, AudioClip clip)
	{
		if (name == null)
		{
			Debug.LogError($"character name is unreadable! Stopping.");
			return;
		}
		StopVoice();
		LastSpeaker = name;
		if (audioSource != source && source != null)
		{
			Object.Destroy(audioSource);
			audioSource = source;
		}
		audioSource.clip = clip;
		audioSource.volume = volume;
		audioSource.Play();
	}

	~VoiceActorHandler()
	{
		Object.Destroy(audioSource);
	}
}