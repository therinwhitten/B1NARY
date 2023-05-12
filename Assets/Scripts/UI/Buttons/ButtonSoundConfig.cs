namespace B1NARY.UI
{
	using UnityEngine;
	using System;
	using System.Linq;

	/// <summary>
	/// Uses <see cref="SoundOptionsBehaviour"/> to implement the sounds.
	/// This is used due to preventing multiple references that could be used 
	/// across multiple buttons.
	/// </summary>
	[CreateAssetMenu(fileName = "New Button Sound Config", menuName = "B1NARY/Button Sound Config", order = 2)]
	public sealed class ButtonSoundConfig : ScriptableObject
	{
		public enum SoundType
		{
			Hover,
			Pressed,
			Any
		}

		public Lazy<AudioClip[]> AllSounds { get; private set; }

		[Tooltip("The sounds to play when the button is being hovered over, highlighted, or " +
			"when a method that invokes a method relating to playing the hovering sound")]
		public AudioClip[] hoverSounds;

		[Tooltip("The sounds to play when the button is pressed, or when a method"
			+ " that invokes a method relating to playing the pressed sound")]
		public AudioClip[] pressedSounds;

		private void Awake()
		{
			AllSounds = new Lazy<AudioClip[]>(() => new AudioClip[][] 
			{ hoverSounds, pressedSounds }
				.SelectMany(array => array).ToArray());
		}

		/// <summary>
		/// Gets a random AudioClip from the currently defined fields/arrays.
		/// </summary>
		/// <param name="soundType"> The sound to get it from. </param>
		/// <param name="randomType"> The random type to use. </param>
		/// <returns></returns>
		/// <exception cref="IndexOutOfRangeException">
		/// If the <see cref="SoundType"/> is not defined in the switch case.
		/// </exception>
		public AudioClip GetRandomAudioClip(SoundType soundType, RandomForwarder.RandomType randomType = RandomForwarder.RandomType.Unity)
		{
			AudioClip[] selection;
			switch (soundType) // This would work perfectly with a switch expression
			{
				case SoundType.Hover:
					selection = hoverSounds;
					break;
				case SoundType.Pressed:
					selection = pressedSounds;
					break;
				case SoundType.Any:
					selection = AllSounds.Value;
					break;
				default:
					throw new IndexOutOfRangeException();
			}
			return selection[RandomForwarder.Next(selection.Length, randomType)];
		}
	}
}