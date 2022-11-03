namespace B1NARY.UI
{
	using UnityEngine;
	using UnityEngine.Audio;
	using B1NARY.Audio;

	// Suggestion: Change this to the H Scene Trigger Script for hit boxes and moaning. 
	[AddComponentMenu("Audio/Buttons/Button Sound Behaviour")]
	public class ButtonSoundBehaviour : MonoBehaviour
	{
		[Tooltip("The Mixer Group to apply to the sound.")]
		public AudioMixerGroup audioMixerGroup;

		public ButtonSoundConfig config;


		#region Hover
		/// <summary>
		/// Plays a one-shot sound using <see cref="hoverSounds"/> using a
		/// <paramref name="index"/> to select a sound within it.
		/// </summary>
		/// <param name="index"> The index to play within it. </param>
		public void PlayHoverSound(int index) =>
			PlaySound(config.hoverSounds[index]);
		/// <summary>
		/// Plays a one-shot sound using <see cref="hoverSounds"/> using a
		/// random number generator to randomly select a sound.
		/// </summary>
		/// <param name="randomType"> The random type to pass into <see cref="RandomFowarder"/>. </param>
		public void PlayRandomHoverSound(RandomFowarder.RandomType randomType = RandomFowarder.RandomType.Unity) =>
			PlaySound(config.GetRandomAudioClip(ButtonSoundConfig.SoundType.Hover, randomType));
		#endregion

		#region Pressed
		/// <summary>
		/// Plays a one-shot sound using <see cref="pressedSounds"/> using a
		/// <paramref name="index"/> to select a sound within it.
		/// </summary>
		/// <param name="index"> The index to play within it. </param>
		public void PlayPressedSound(int index)
			=> PlaySound(config.pressedSounds[index]);
		/// <summary>
		/// Plays a one-shot sound using <see cref="pressedSounds"/> using a
		/// random number generator to randomly select a sound.
		/// </summary>
		/// <param name="randomType"> The random type to pass into <see cref="RandomFowarder"/>. </param>
		public void PlayRandomPressedSound(RandomFowarder.RandomType randomType = RandomFowarder.RandomType.Unity) =>
			PlaySound(config.GetRandomAudioClip(ButtonSoundConfig.SoundType.Pressed, randomType));
		#endregion

		#region Highlighted
		/// <summary>
		/// Plays a one-shot sound using <see cref="highlightedSounds"/> using a
		/// <paramref name="index"/> to select a sound within it.
		/// </summary>
		/// <param name="index"> The index to play within it. </param>
		public void PlayHighlightedSound(int index)
			=> PlaySound(config.highlightedSounds[index]);
		/// <summary>
		/// Plays a one-shot sound using <see cref="highlightedSounds"/> using a
		/// random number generator to randomly select a sound.
		/// </summary>
		/// <param name="randomType"> The random type to pass into <see cref="RandomFowarder"/>. </param>
		public void PlayRandomHighlightedSound(RandomFowarder.RandomType randomType = RandomFowarder.RandomType.Unity) =>
			PlaySound(config.GetRandomAudioClip(ButtonSoundConfig.SoundType.Highlighted, randomType));
		#endregion

		#region Any
		/// <summary>
		/// Selects any sounds located in <see cref="config"/> to play.
		/// </summary>
		/// <param name="randomType"> The random value to pass through <see cref="RandomFowarder"/>. </param>
		public void PlayRandomSound(RandomFowarder.RandomType randomType = RandomFowarder.RandomType.Unity)
			=> PlaySound(config.GetRandomAudioClip(ButtonSoundConfig.SoundType.Any, randomType));
		#endregion

		/// <summary>
		/// Plays a OneShot sound through <see cref="AudioController"/>
		/// using <paramref name="sound"/>.
		/// </summary>
		/// <param name="sound"> The sound to play. </param>
		public void PlaySound(AudioClip sound)
		{
			if (sound == null)
				return;
			AudioController.Instance.PlayOneShot(sound, audioMixerGroup, 1f);
		}
	}
}