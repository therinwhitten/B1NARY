namespace B1NARY.UI
{
	using System;
	using UnityEngine;
	using UnityEngine.Audio;
	using UnityEngine.EventSystems;
	using B1NARY.Audio;
	using UnityEngine.UI;


	// Suggestion: Change this to the H Scene Trigger Script for hit boxes and moaning. 
	[AddComponentMenu("Audio/Buttons/Button Sound Behaviour"), RequireComponent(typeof(Button))]
	public class ButtonSoundBehaviour : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
	{
		/// <summary>
		/// If the object or int value is a negative value or not. Recommended
		/// to use <see cref="Math.Sign(int)"/> for other purposes.
		/// </summary>
		/// <param name="value"> The value to check. </param>
		/// <returns> if the int value is negative. </returns>
		public static bool Sign(in int value) => value < 0;

		[Tooltip("The Mixer Group to apply to the sound.")]
		public AudioMixerGroup audioMixerGroup;
		[Tooltip("The current sound configuaration to play sounds with")]
		public ButtonSoundConfig config;


		#region Hover
		/// <summary>
		/// Choose a random sound if negative, or a selected hover sound to play
		/// if positive. This plays hover-based Sounds.
		/// </summary>
		public int hoverIndex = -1;
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
		void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
		{
			if (Math.Sign(hoverIndex) == -1)
			{
				PlayRandomHoverSound();
				return;
			}
			PlayHoverSound(hoverIndex);
		}
		#endregion

		#region Pressed
		/// <summary>
		/// Choose a random sound if negative, or a selected hover sound to play
		/// if positive. This plays pressed-based Sounds.
		/// </summary>
		public int pressedIndex = -1;
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
		void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
		{
			if (Math.Sign(hoverIndex) == -1)
			{
				PlayRandomHoverSound();
				return;
			}
			PlayHoverSound(hoverIndex);

		}
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