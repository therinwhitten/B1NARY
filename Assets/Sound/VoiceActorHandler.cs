using UnityEngine;

public class VoiceActorHandler : SingletonNew<VoiceActorHandler>
{
	private SoundCoroutine speakerCoroutine = null;
	private string lastSpeaker = null;

	protected override void SingletonStart()
	{
		speakerCoroutine = new SoundCoroutine(this) { destroyOnFinish = false };
		speakerCoroutine.Finished += (sender, args) => PlayNew();
	}

	public void PlayVoice(string name, float volume, AudioSource source, AudioClip clip)
	{
		if (name == null)
		{
			Debug.LogWarning($"character name is unreadable! Is this intentional?");
			return;
		}
		canPlay = true;
		speakerPackage = (source, name, volume, clip);
		if (speakerCoroutine.IsPlaying)
			speakerCoroutine.Stop(0.2f, false);
		else if (speakerCoroutine.IsFadingAway)
			speakerCoroutine.Stop();
		else PlayNew();
	}

	// I hope i find a better way than having global variables here.
	private bool canPlay = false;
	private (AudioSource, string, float, AudioClip) speakerPackage;
	private void PlayNew()
	{
		if (!canPlay)
			return;
		if (lastSpeaker != speakerPackage.Item2)
			lastSpeaker = speakerPackage.Item2;
		speakerCoroutine.AudioSource = speakerPackage.Item1;
		speakerCoroutine.AudioClip = new CustomAudioClip(speakerPackage.Item4) { volume = speakerPackage.Item3 };
		speakerCoroutine.PlaySingle();
	}
}