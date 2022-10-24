namespace B1NARY.UI
{
	using UnityEngine;
	using UnityEngine.Audio;
	using B1NARY.Audio;

	[AddComponentMenu("Audio/Buttons/Button Sound Behaviour")]
	public class ButtonSoundBehaviour : MonoBehaviour
	{
		[Tooltip("The Mixer Group to apply to the sound.")]
		public AudioMixerGroup audioMixerGroup;

		[Tooltip("The sounds to play when the button is being hovered over, or " +
			"when a method that invokes a method relating to playing the hovering sound")]
		public AudioClip[] hoverSounds;

		[Tooltip("The sounds to play when the button is pressed, or when a method"
			+ " that invokes a method relating to playing the pressed sound")]
		public AudioClip[] pressedSounds;

		/// <summary>
		/// Plays a one-shot sound using <see cref="hoverSounds"/> using a
		/// <paramref name="index"/> to select a sound within it.
		/// </summary>
		/// <param name="index"> The index to play within it. </param>
		public void PlayHoverSound(int index) =>
			PlaySound(hoverSounds[index]);
		/// <summary>
		/// Plays a one-shot sound using <see cref="hoverSounds"/> using a
		/// random number generator to randomly select a sound.
		/// </summary>
		/// <param name="randomType"> The random type to pass into <see cref="RandomFowarder"/>. </param>
		public void PlayHoverSound(RandomFowarder.RandomType randomType = RandomFowarder.RandomType.Unity) =>
			PlaySound(hoverSounds[RandomFowarder.Next(hoverSounds.Length, randomType)]);

		/// <summary>
		/// Plays a one-shot sound using <see cref="pressedSounds"/> using a
		/// <paramref name="index"/> to select a sound within it.
		/// </summary>
		/// <param name="index"> The index to play within it. </param>
		public void PlayPressedSound(int index)
			=> PlaySound(pressedSounds[index]);
		/// <summary>
		/// Plays a one-shot sound using <see cref="pressedSounds"/> using a
		/// random number generator to randomly select a sound.
		/// </summary>
		/// <param name="randomType"> The random type to pass into <see cref="RandomFowarder"/>. </param>
		public void PlayPressedSound(RandomFowarder.RandomType randomType = RandomFowarder.RandomType.Unity) =>
			PlaySound(pressedSounds[RandomFowarder.Next(pressedSounds.Length, randomType)]);

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