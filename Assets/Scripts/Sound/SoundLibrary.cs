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
public class SoundLibrary : ScriptableObject, IEnumerable<CustomAudioClip>
{
	public List<CustomAudioClip> customAudioClips;
	public CustomAudioClip this[int index] => customAudioClips[index];
	public int Length => customAudioClips.Count;

	private Dictionary<AudioClip, CustomAudioClip> audioClipLink
		= new Dictionary<AudioClip, CustomAudioClip>();
	public CustomAudioClip GetCustomAudioClip(AudioClip audioClip)
	{
		if (audioClipLink.TryGetValue(audioClip, out var output))
			return output;
		for (int i = 0; i < customAudioClips.Count; i++)
		{
			// Since methods can call on this multiple times when the method
			// - isn't finished, ill have to do additional checks here.
			if (customAudioClips[i].clip == null)
				continue;
			if (audioClipLink.ContainsValue(customAudioClips[i]))
				continue;
			audioClipLink.Add(customAudioClips[i].clip, customAudioClips[i]);
			if (audioClipLink.TryGetValue(audioClip, out var value))
				return value;
		}
		throw new ArgumentOutOfRangeException($"Cannot find {audioClip.name}!");
	}
	public bool ContainsCustomAudioClip(AudioClip audioClip)
	{
		try
		{
			GetCustomAudioClip(audioClip);
			return true;
		}
		catch (ArgumentOutOfRangeException) { return false; }
	}


	private Dictionary<string, AudioClip> stringLink 
		= new Dictionary<string, AudioClip>();
	public AudioClip GetAudioClip(string audioClip)
	{
		if (stringLink.TryGetValue(audioClip, out var output))
			return output;
		for (int i = 0; i < customAudioClips.Count; i++)
		{
			// Since methods can call on this multiple times when the method
			// - isn't finished, ill have to do additional checks here.
			if (customAudioClips[i].clip == null)
				continue;
			if (stringLink.ContainsValue(customAudioClips[i].clip))
				continue;
			stringLink.Add(customAudioClips[i].clip.name, customAudioClips[i].clip);
			if (stringLink.TryGetValue(audioClip, out var value))
				return value;
		}
		throw new ArgumentOutOfRangeException($"Cannot find {audioClip}!");
	}
	public bool ContainsAudioClip(string audioClip)
	{
		try
		{
			GetAudioClip(audioClip);
			return true;
		}
		catch (ArgumentOutOfRangeException) { return false; }
	}

	public IEnumerator<CustomAudioClip> GetEnumerator()
	{
		return ((IEnumerable<CustomAudioClip>)customAudioClips).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return customAudioClips.GetEnumerator();
	}
}