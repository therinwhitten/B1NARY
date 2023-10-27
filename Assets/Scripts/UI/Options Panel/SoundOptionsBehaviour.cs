namespace B1NARY.UI
{
	using System;
	using UnityEngine;
	using UnityEngine.UI; 
	using UnityEngine.Audio;
	using B1NARY.Audio;
	using System.Collections.Generic;
	using OVSSerializer.Extras;
	using B1NARY.DataPersistence;
	using Microsoft.Win32;
	using HDConsole;

	public class SoundOptionsBehaviour : MonoBehaviour 
	{
		// Sliders for Inspector
		public AudioMixer mixer;
		public Slider[] soundSliders;

		// Sliders Assignment to Mixer
		private void Start()
		{
			Dictionary<string, ChangableValue<float>> mixers = SoundSyncer.GetConstantMixers();
			for (int i = 0; i < soundSliders.Length; i++)
			{
				if (!soundSliders[i].gameObject.activeInHierarchy)
					continue;
				// Gets to the part from slider component past 'assembly' to the name of slider.
				Transform sliderMaster = soundSliders[i].transform.parent.parent;
				string name = sliderMaster.name;
				if (mixers.TryGetValue(name, out ChangableValue<float> changableValue))
					soundSliders[i].value = changableValue.Value;
				else if (PlayerConfig.Instance.audio.characterVoices.TryGetValue(name, out float value))
					soundSliders[i].value = value;
				soundSliders[i].onValueChanged.AddListener(value => SoundSyncer.Instance.SetVolume(name, value));
			}
		}
		
	}
}

