using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using System.Collections;

///	<summary>
///		A group of sounds customized by the user, can be used separately 
///		or multiple.
///	</summary>
[CreateAssetMenu(fileName = "New Sound Library", menuName = "B1NARY/Sound Library (SL)", order = 0)]
public class SoundLibrary : ScriptableObject
{
	public bool ContainsPlayOnAwakeCommands { get; private set; }
	public IEnumerable<CustomAudioClip> PlayOnAwakeCommands { get; private set; } = null;
	public List<CustomAudioClip> customAudioClips;
	public CustomAudioClip this[int index] => customAudioClips[index];
	public int Length => customAudioClips.Count;

	private void Awake()
	{
		IEnumerable<CustomAudioClip> playAwakeClips = customAudioClips.Where(CClip => CClip.playOnAwake);
		if (ContainsPlayOnAwakeCommands = playAwakeClips.Any())
			PlayOnAwakeCommands = playAwakeClips;
	}


	private bool completedAudioClipLink = false;
	private Dictionary<AudioClip, int> audioClipLink
		= new Dictionary<AudioClip, int>();
	public CustomAudioClip GetCustomAudioClip(AudioClip audioClip)
	{
		if (audioClipLink.TryGetValue(audioClip, out var output))
			return customAudioClips[output];
		if (completedAudioClipLink)
			goto end;
		for (int i = 0; i < customAudioClips.Count; i++)
		{
			// Since methods can call on this multiple times when the method
			// - isn't finished, ill have to do additional checks here.
			if (customAudioClips[i].clip == null)
				throw new ArgumentNullException($"Sound Library contains an empty audio clip in index {i}!");
			if (audioClipLink.ContainsValue(i))
				continue;
			audioClipLink.Add(customAudioClips[i].clip, i);
			// Multi-threading, sometimes needed
			if (audioClipLink.TryGetValue(audioClip, out var value))
			{
				if (customAudioClips.Count == audioClipLink.Count)
					completedAudioClipLink = true;
				return customAudioClips[value];
			}
		}
		end: throw new ArgumentOutOfRangeException($"Cannot find {audioClip.name.Trim()}!");
	}
	public bool ContainsCustomAudioClip(AudioClip audioClip)
	{
		if (audioClipLink.ContainsKey(audioClip))
			return true;
		if (completedAudioClipLink)
			return false;
		for (int i = 0; i < customAudioClips.Count; i++)
		{
			// Since methods can call on this multiple times when the method
			// - isn't finished, ill have to do additional checks here.
			if (customAudioClips[i].clip == null)
				throw new ArgumentNullException($"Sound Library contains an empty audio clip in index {i}!");
			if (audioClipLink.ContainsValue(i))
				continue;
			audioClipLink.Add(customAudioClips[i].clip, i);
			// Multi-threading, sometimes needed
			if (audioClipLink.ContainsKey(audioClip))
			{
				if (customAudioClips.Count == audioClipLink.Count)
					completedAudioClipLink = true;
				return true;
			}
		}
		return false;
	}

	private bool completedStringLink = false;
	private Dictionary<string, int> stringLink 
		= new Dictionary<string, int>();
	public AudioClip GetAudioClip(string audioClip)
	{
		if (stringLink.TryGetValue(audioClip, out var output))
			return customAudioClips[output];
		if (completedAudioClipLink)
			goto end;
		for (int i = 0; i < customAudioClips.Count; i++)
		{
			// Since methods can call on this multiple times when the method
			// - isn't finished, ill have to do additional checks here.
			if (customAudioClips[i].clip == null)
				throw new ArgumentNullException($"Sound Library contains an empty audio clip in index {i}!");
			if (stringLink.ContainsValue(i))
				continue;
			stringLink.Add(customAudioClips[i].Name, i);
			// Multi-threading, sometimes needed
			if (stringLink.TryGetValue(audioClip, out var value))
			{
				if (customAudioClips.Count == stringLink.Count)
					completedStringLink = true;
				return customAudioClips[value].clip;
			}
		}
		end: throw new ArgumentOutOfRangeException($"Cannot find {audioClip}!");
	}
	public bool ContainsAudioClip(string audioClip)
	{
		if (stringLink.ContainsKey(audioClip))
			return true;
		if (completedStringLink)
			return false;
		for (int i = 0; i < customAudioClips.Count; i++)
		{
			// Since methods can call on this multiple times when the method
			// - isn't finished, ill have to do additional checks here.
			if (customAudioClips[i].clip == null)
				throw new ArgumentNullException($"Sound Library contains an empty audio clip in index {i}!");
			if (stringLink.ContainsValue(i))
				continue;
			stringLink.Add(customAudioClips[i].Name, i);
			// Multi-threading, sometimes needed
			if (stringLink.ContainsKey(audioClip))
			{
				if (customAudioClips.Count == stringLink.Count)
					completedStringLink = true;
				return true;
			}
		}
		return false;
	}
}