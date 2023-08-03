namespace B1NARY.UI
{
	using System;
	using UnityEngine;
	using UnityEngine.UI; 
	using UnityEngine.Audio;
	using B1NARY.Audio;
	using System.Collections.Generic;
	using OVSXmlSerializer.Extras;
	using B1NARY.DataPersistence;
	using Microsoft.Win32;

	public class SoundOptionsBehaviour : MonoBehaviour 
	{
		//  Sliders for Inspector
		public AudioMixer mixer;
		public Slider[] soundSliders;

		//  Strings Might need to be public to transfer Scenes and Save to Audio Manager. Video Source https://www.youtube.com/watch?v=pbuJUaO-wpY&t=383s
		public const string MIXER_MASTER = "Master";
		public const string MIXER_MUSIC = "Music";
		public const string MIXER_SFX = "SFX";
		public const string MIXER_UI = "UI";
		public static Dictionary<string, ChangableValue<float>> GetConstantMixers() => new()
		{
			[MIXER_MASTER] = PlayerConfig.Instance.audio.master,
			[MIXER_MUSIC] = PlayerConfig.Instance.audio.music,
			[MIXER_SFX] = PlayerConfig.Instance.audio.SFX,
			[MIXER_UI] = PlayerConfig.Instance.audio.UI
		};

		//public const string MIXER_PLAYER = "Player";
		//public const string MIXER_NPC = "NPC";
		//public const string MIXER_CAMEOS = "Cameos"; 
		//public const string MIXER_CANDII = "Candii"; 
		//public const string MIXER_ASTERELLA = "Asterella";
		//public const string MIXER_BENOSPHERE = "Benosphere";
		//public const string MIXER_DEL = "Del";
		//public const string MIXER_FEFE = "Fefe";
		//public const string MIXER_VL = "VL";
		//public const string MIXER_KITTY = "Kitty";
		//public const string MIXER_MOE = "Moe";
		//public const string MIXER_MELODY = "Melody";
		//public const string MIXER_SZYCROTIC = "Szycrotic";
		//public const string MIXER_SILVERVALE = "Silvervale";
		//public const string MIXER_LIZZY = "Lizzy";
		//  Sliders Assignment to Mixer
		private void Start()
		{
			Dictionary<string, ChangableValue<float>> mixers = GetConstantMixers();
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
				soundSliders[i].onValueChanged.AddListener(value => SetVolume(name, value));
			}
		}
		
		/// <summary>
		/// Sets both the volume ingame and in the data persistence.
		/// </summary>
		/// <param name="key"> The name of the volume. </param>
		/// <param name="value"> The Logarithmic(?) value. </param>
		public void SetVolume(string key, float value)
		{
			if (GetConstantMixers().TryGetValue(key, out var changableValue))
			{
				changableValue.Value = value;
				float output = Mathf.Log10(value) * 20; 
				mixer.SetFloat(key, output);
			}
			else
			{
				float output = Mathf.Log10(value) * 20;
				if (!mixer.SetFloat(key, output))
				{
					Debug.LogWarning($"'{key}' is not found in the exposed parameters in mixer group, '{mixer.name}'", this);
					return;
				}
				PlayerConfig.Instance.audio.characterVoices[key] = value;
			}
		}
	}
}

