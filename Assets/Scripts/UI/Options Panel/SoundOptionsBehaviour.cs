namespace B1NARY.UI
{
	using System;
	using UnityEngine;
	using UnityEngine.UI; 
	using UnityEngine.Audio;
	using B1NARY.Audio;
	using System.Collections.Generic;
	using OVSXmlSerializer.Extras;

	public class SoundOptionsBehaviour : MonoBehaviour 
	{
		//  Sliders for Inspector
		[SerializeField] AudioMixer mixer;
		[SerializeField] Slider masterSlider;
		[SerializeField] Slider musicSlider;
		[SerializeField] Slider sfxSlider;
		[SerializeField] Slider uiSlider;
		[SerializeField] Slider playerSlider;
		[SerializeField] Slider npcSlider;
		[SerializeField] Slider cameosSlider;
		//  Adding the slider components for inspector for VA. Sub Group of the Voice Channel 
		//  Each Cast Member has their own Audio Source
		[SerializeField] Slider candiiSlider;
		[SerializeField] Slider asterellaSlider;
		[SerializeField] Slider benosphereSlider;
		[SerializeField] Slider comradeDelSlider;
		[SerializeField] Slider fefeSlider;
		[SerializeField] Slider virtuallyLewdSlider;
		[SerializeField] Slider kittyMcpancakesSlider;
		[SerializeField] Slider natsumiMoeSlider;
		[SerializeField] Slider projektMelodySlider;
		[SerializeField] Slider szycroticSlider;
		[SerializeField] Slider silvervaleSlider;
		[SerializeField] Slider watchingLizardSlider;

		//  Strings Might need to be public to transfer Scenes and Save to Audio Manager. Video Source https://www.youtube.com/watch?v=pbuJUaO-wpY&t=383s
		public const string MIXER_MASTER = "Master";
		public const string MIXER_MUSIC = "Music";
		public const string MIXER_SFX = "SFX";
		public const string MIXER_UI = "UI";
		public static Dictionary<string, ChangableValue<float>> GetConstantMixers() 
			=> new Dictionary<string, ChangableValue<float>>()
		{
			[MIXER_MASTER] = PlayerConfig.Instance.audio.master,
			[MIXER_MUSIC] = PlayerConfig.Instance.audio.music,
			[MIXER_SFX] = PlayerConfig.Instance.audio.SFX,
			[MIXER_UI] = PlayerConfig.Instance.audio.UI
		};

		public const string MIXER_PLAYER = "Player";
		public const string MIXER_NPC = "NPC";
		public const string MIXER_CAMEOS = "Cameos"; 
		public const string MIXER_CANDII = "Candii"; 
		public const string MIXER_ASTERELLA = "Asterella";
		public const string MIXER_BENOSPHERE = "Benosphere";
		public const string MIXER_DEL = "Del";
		public const string MIXER_FEFE = "Fefe";
		public const string MIXER_VL = "VL";
		public const string MIXER_KITTY = "Kitty";
		public const string MIXER_MOE = "Moe";
		public const string MIXER_MELODY = "Melody";
		public const string MIXER_SZYCROTIC = "Szycrotic";
		public const string MIXER_SILVERVALE = "Silvervale";
		public const string MIXER_LIZZY = "Lizzy";
		//  Sliders Assignment to Mixer
		void Awake()
		{
			masterSlider.onValueChanged.AddListener(SetMasterVolume);
			musicSlider.onValueChanged.AddListener(SetMusicVolume);
			sfxSlider.onValueChanged.AddListener(SetSFXVolume);
			uiSlider.onValueChanged.AddListener(SetUIVolume);
			playerSlider.onValueChanged.AddListener(SetPlayerVolume);
			npcSlider.onValueChanged.AddListener(SetNPCVolume);
			cameosSlider.onValueChanged.AddListener(SetCameosVolume);
			candiiSlider.onValueChanged.AddListener(SetCandiiVolume);
			asterellaSlider.onValueChanged.AddListener(SetAsterellaVolume);
			benosphereSlider.onValueChanged.AddListener(SetBenosphereVolume);
			comradeDelSlider.onValueChanged.AddListener(SetDelVolume);
			fefeSlider.onValueChanged.AddListener(SetFefeVolume);
			virtuallyLewdSlider.onValueChanged.AddListener(SetVLVolume);
			kittyMcpancakesSlider.onValueChanged.AddListener(SetKittyVolume);
			natsumiMoeSlider.onValueChanged.AddListener(SetMoeVolume);
			projektMelodySlider.onValueChanged.AddListener(SetMelodyVolume);
			szycroticSlider.onValueChanged.AddListener(SetSzycroticVolume);
			silvervaleSlider.onValueChanged.AddListener(SetSilvervaleVolume);
			watchingLizardSlider.onValueChanged.AddListener(SetLizzyVolume);
		}
		
		/// <summary>
		/// Sets both the volume ingame and in the data persistence.
		/// </summary>
		/// <param name="key"> The name of the volume. </param>
		/// <param name="value"> The Logarithmic(?) value. </param>
		public void SetVolume(string key, float value)
		{
			float output;
			if (GetConstantMixers().TryGetValue(key, out var changableValue))
			{
				changableValue.Value = value;
				output = Mathf.Log10(value) * 20;
			}
			else
			{
				PlayerConfig.Instance.audio.characterVoices[key] = value;
				output = Mathf.Log10(value) * 20;
			}
			mixer.SetFloat(key, output);
		}

		// Logrithmic Volume Pairs. Need these set up like this. Slider Pairs in inspector need to be around .1 for lowest with 1 the highest. If 0, will break channel.
		public void SetMasterVolume(float value)
		{
			SetVolume(MIXER_MASTER, value);
		}
		public void SetMusicVolume(float value)
		{
			SetVolume(MIXER_MUSIC, value);
		}
		public void SetSFXVolume(float value)
		{
			SetVolume(MIXER_SFX, value);
		}
		public void SetUIVolume(float value)
		{
			SetVolume(MIXER_UI, value);
		}
		public void SetPlayerVolume(float value)
		{
			SetVolume(MIXER_PLAYER, value);
		}
		public void SetNPCVolume(float value)
		{
			SetVolume(MIXER_NPC, value);
		}
		public void SetCameosVolume(float value)
		{
			SetVolume(MIXER_CAMEOS, value);
		}
		public void SetCandiiVolume(float value)
		{
			SetVolume(MIXER_CANDII, value);
		}
		public void SetAsterellaVolume(float value)
		{
			SetVolume(MIXER_ASTERELLA, value);
		}
		public void SetBenosphereVolume(float value)
		{
			SetVolume(MIXER_BENOSPHERE, value);
		}
		public void SetDelVolume(float value)
		{
			SetVolume(MIXER_DEL, value);
		}
		public void SetFefeVolume(float value)
		{
			SetVolume(MIXER_FEFE, value);
		}
		public void SetVLVolume(float value)
		{
			SetVolume(MIXER_VL, value);
		}
		public void SetKittyVolume(float value)
		{
			SetVolume(MIXER_KITTY, value);
		}
		public void SetMoeVolume(float value)
		{
			SetVolume(MIXER_MOE, value);
		}
		public void SetMelodyVolume(float value)
		{
			SetVolume(MIXER_MELODY, value);
		}
		public void SetSzycroticVolume(float value)
		{
			SetVolume(MIXER_SZYCROTIC, value);
		}
		public void SetSilvervaleVolume(float value)
		{
			SetVolume(MIXER_SILVERVALE, value);
		}
		public void SetLizzyVolume(float value)
		{
			SetVolume(MIXER_LIZZY, value);
		}
	}
	



	
 
}

