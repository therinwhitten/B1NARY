namespace B1NARY.Audio
{
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;

	///	<summary>
	///		A group of sounds customized by the user, can be used separately 
	///		or multiple.
	///	</summary>
	[CreateAssetMenu(fileName = "New Sound Library", menuName = "B1NARY/Sound Library (SL)", order = 0)]
	public class SoundLibrary : ScriptableObject
	{

		/// <summary>
		/// Gets a value indicating whether it has sounds that has an imageOption that can
		/// play it on awake.
		/// </summary>
		public bool ContainsPlayOnAwakeCommands { get; private set; }
		/// <summary>
		/// The total list/copy that are wanted to play on awake. set as <see langword="null"/> 
		/// if <see cref="ContainsPlayOnAwakeCommands"/> is <see langword="false"/>
		/// </summary>
		public IEnumerable<CustomAudioClip> PlayOnAwakeCommands { get; private set; } = null;

		/// <summary>
		/// The list of <see cref="CustomAudioClip"/>s that the <see cref="SoundLibrary"/>
		/// stores. This is inputted by the user.
		/// </summary>
		public List<CustomAudioClip> customAudioClips;

		/// <summary>
		/// Gets the <see cref="CustomAudioClip"/> at the specified index. Basically
		/// forwards to the list.
		/// </summary>
		public CustomAudioClip this[int index] => customAudioClips[index];

		/// <summary>
		/// Gets the count of <see cref="customAudioClips"/>.
		/// </summary>
		public int Count => customAudioClips.Count;

		private void Awake()
		{
			if (customAudioClips == null)
			{
				customAudioClips = new List<CustomAudioClip>();
				Debug.LogError("Clip data has been found null. Replacing " +
					"default settings. Hopefully you made a backup of them?", this);
				return;
			}
			IEnumerable<CustomAudioClip> playAwakeClips = customAudioClips.Where(CClip => CClip.playOnAwake);
			if (ContainsPlayOnAwakeCommands = playAwakeClips.Any())
				PlayOnAwakeCommands = playAwakeClips;
		}
		public bool TryGetCustomAudioClip(string name, out CustomAudioClip clip)
		{
			for (int i = 0; i < customAudioClips.Count; i++)
				if (customAudioClips[i].Name == name)
				{
					clip = customAudioClips[i];
					return true;
				}
			clip = null;
			return false;
		}
		public bool TryGetCustomAudioClip(AudioClip example, out CustomAudioClip clip)
		{
			for (int i = 0; i < customAudioClips.Count; i++)
				if (customAudioClips[i].clip == example)
				{
					clip = customAudioClips[i];
					return true;
				}
			clip = null;
			return false;
		}
	}
}