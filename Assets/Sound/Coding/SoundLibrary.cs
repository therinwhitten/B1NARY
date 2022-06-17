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
	public CustomAudioClip[] customAudioClips;
	public CustomAudioClip this[int index] => customAudioClips[index];
	public int Length => customAudioClips.Length;



	private Dictionary<AudioClip, CustomAudioClip> audioClipLink = null;
	public Dictionary<AudioClip, CustomAudioClip> AudioClipLink { get 
		{
			if (audioClipLink == null)
			{
				audioClipLink = new Dictionary<AudioClip, CustomAudioClip>();
				for (int i = 0; i < customAudioClips.Length; i++)
					if (!audioClipLink.ContainsKey(customAudioClips[i]))
					audioClipLink.Add(customAudioClips[i].audioClip,
						customAudioClips[i]);
			}
			return audioClipLink;
		}
	}

	private Dictionary<string, AudioClip> stringLink = null;
	public Dictionary<string, AudioClip> StringLink { get 
		{
			if (stringLink == null)
			{
				stringLink = new Dictionary<string, AudioClip>();
				for (int i = 0; i < customAudioClips.Length; i++)
					if (!stringLink.ContainsKey(customAudioClips[i].audioClip.name))
						stringLink.Add(customAudioClips[i].audioClip.name,
						customAudioClips[i].audioClip);
			}
			return stringLink;
		}
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