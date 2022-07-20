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

	public void PlayVoice(string name, DialogueLine line, float voiceVolume, AudioSource voice)
	{
		AudioClip clip = GetVoiceLine(line);
		if (clip == null)
		{
			StopVoice();
			return;
		}
		PlayVoice(name, voiceVolume, voice, clip);
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

	public AudioClip GetVoiceLine(DialogueLine line)
	{
		string filePath = $"Voice/{line.scriptName}/{line.index}";
		AudioClip output = Resources.Load<AudioClip>(filePath);
		if (output == null)
			Debug.LogError($"Voice Actor line '{filePath}' does not have" +
				" an exact filename! Did you leave a space within the filename?");
		return output;
	}


	~VoiceActorHandler()
	{
		Object.Destroy(audioSource);
	}
}