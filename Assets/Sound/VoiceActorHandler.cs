using UnityEngine;

public class VoiceActorHandler : SingletonNew<VoiceActorHandler>
{
	private SoundCoroutine speakerCoroutine = null;
	private string lastSpeaker = null;

	protected override void SingletonStart()
	{
		speakerCoroutine = new SoundCoroutine(this) { destroyOnFinish = false };
	}

	public void PlayVoice(string name, float volume, AudioSource source, AudioClip clip)
	{
		if (name == null)
		{
			Debug.LogWarning($"character name is null! Is this intentional?");
			return;
		}
		speakerCoroutine.Stop(0.3f, false);
		if (lastSpeaker != name)
			lastSpeaker = name;
		speakerCoroutine.AudioSource = source;
		speakerCoroutine.AudioClip = new CustomAudioClip(clip) { volume = volume };
		speakerCoroutine.PlaySingle();
	}

}