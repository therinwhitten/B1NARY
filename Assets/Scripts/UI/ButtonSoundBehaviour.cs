using UnityEngine;
using UnityEngine.Audio;

public class ButtonSoundBehaviour : MonoBehaviour
{
	public AudioMixerGroup audioMixerGroup;
	public AudioClip onHover, onPress;

	// Normally i would just allow people to just override the audioclip in the
	// - PlaySoundOnStartTransition, but it doesn't appear to have any choice
	// - to do so. This is so far the only way i know.
	public void PlayHoverSound() => PlaySound(onHover, audioMixerGroup);
	public void PlayPressSound() => PlaySound(onPress, audioMixerGroup);

	private static void PlaySound(AudioClip sound, AudioMixerGroup mixerGroup)
	{
		if (sound == null)
			return;
		AudioHandler.Instance.PlayOneShot(sound, mixerGroup, true);
	}
}