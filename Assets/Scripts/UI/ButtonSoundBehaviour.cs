namespace B1NARY.UI
{
	using UnityEngine;
	using UnityEngine.Audio;
	using B1NARY.Sounds;

	public class ButtonSoundBehaviour : MonoBehaviour
	{
		public AudioMixerGroup audioMixerGroup;
		public AudioClip[] audioSounds;

		// Normally i would just allow people to just override the audioclip in the
		// - PlaySoundOnStartTransition, but it doesn't appear to have any choice
		// - to do so. This is so far the only way i know.
		public void PlaySound(int index) =>
			PlaySound(audioSounds[index], audioMixerGroup);
		private static void PlaySound(AudioClip sound, AudioMixerGroup mixerGroup)
		{
			if (sound == null)
				return;
			AudioHandler.Instance.PlayOneShot(sound, mixerGroup, true);
		}
	}
}