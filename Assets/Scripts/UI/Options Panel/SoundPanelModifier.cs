namespace B1NARY.Audio
{
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.Audio;
	using UnityEngine.UI;

	public sealed class SoundPanelModifier : MonoBehaviour
	{
		public float GlobalVolume { get => AudioListener.volume; set => AudioListener.volume = value; }
		public bool UnityAudioToggle { get => AudioListener.pause; set => AudioListener.pause = value; }
		public AudioMixerGroup fullMixerGroup;

		public float SliderArgument { get; set; } = 0f;
		public void ChangedSliderValue(string namePath)
		{
			AudioMixer audioMixer = fullMixerGroup.audioMixer.FindMatchingGroups(namePath).Single().audioMixer;
			//var paths = new Queue<string>(fullPathData.Take(fullPathData.Count - 1));
			//AudioMixer currentMixer = fullMixerGroup.audioMixer;
			//while (paths.Count > 0)
			//	currentMixer = currentMixer.FindMatchingGroups(paths.Dequeue()).Single().audioMixer;
			//audioMixer.
			audioMixer.SetFloat("Volume", SliderArgument);
		}
	}
}
