namespace B1NARY.Sounds
{
	using System;
	using System.IO;
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
		/// Gets a value indicating whether it has sounds that has an option that can
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
			IEnumerable<CustomAudioClip> playAwakeClips = customAudioClips.Where(CClip => CClip.playOnAwake);
			if (ContainsPlayOnAwakeCommands = playAwakeClips.Any())
				PlayOnAwakeCommands = playAwakeClips;
			// Initializing the values first-hand due to how loading times
			_ = AudioClipLink;
			_ = StringLink;
		}

		private Dictionary<AudioClip, int> _audioClipLink;
		private Dictionary<AudioClip, int> AudioClipLink
		{
			get
			{
				if (_audioClipLink == null)
				{
					int i = -1;
					_audioClipLink = customAudioClips.ToDictionary(clip => clip.clip, input => { i++; return i; });
				}
				return _audioClipLink;
			}
		}
		public CustomAudioClip GetCustomAudioClip(AudioClip audioClip) =>
			customAudioClips[AudioClipLink[audioClip]];

		/// <summary>
		/// Determines whether the audio library contains the <see cref="CustomAudioClip"/>
		/// within <see cref="customAudioClips"/>. Uses an algorithm if it cannot find one
		/// as it is parsed when needed.
		/// </summary>
		/// <param name="audioClip">The audio clip to check if it contains it.</param>
		public bool ContainsCustomAudioClip(AudioClip audioClip) =>
			AudioClipLink.ContainsKey(audioClip);

		private Dictionary<string, int> _stringLink;
		private Dictionary<string, int> StringLink
		{
			get
			{
				if (_stringLink == null)
				{
					int i = -1;
					_stringLink = customAudioClips.ToDictionary(clip => clip.Name, input => { i++; return i; });
				}
				return _stringLink;
			}
		}
		public AudioClip GetAudioClip(string audioClip) =>
			customAudioClips[StringLink[audioClip]];

		public bool ContainsAudioClip(string audioClip) =>
			StringLink.ContainsKey(audioClip);
	}
}