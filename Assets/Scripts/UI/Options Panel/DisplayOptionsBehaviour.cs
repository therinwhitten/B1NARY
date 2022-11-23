namespace B1NARY.UI
{
	using System;
	using System.Linq;
	using System.Collections;
	using System.Collections.Generic;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Rendering;
	using UnityEngine.Rendering.Universal;

	public sealed class DisplayOptionsBehaviour : MonoBehaviour
	{
		public bool HentaiMode { get => PlayerPrefsShortcuts.IsHentaiEnabled; set => PlayerPrefsShortcuts.IsHentaiEnabled = value; }
		public TMP_Dropdown hentaiDropdown;
		public bool InFullScreen { get => Screen.fullScreen; set => Screen.fullScreen = value; }
		public TMP_Dropdown fullScreenDropdown;
		public RenderPipelineAsset[] qualityLevels;
		public TMP_Dropdown qualityDropdown;
		public TMP_Dropdown resolutionDropdown;
		public TMP_Dropdown themeDropdown;
		public TMP_Dropdown languageDropdown;
		private Resolution[] resolutions;
		[SerializeField] Slider glow;
		[SerializeField] Volume volumeProfile;
		public float BloomIntensity
		{
			get => volumeProfile.profile.components.Single().parameters[1].GetValue<float>();
			set => ((VolumeParameter<float>)volumeProfile.profile.components.Single().parameters[1]).value = value;
		}
		
		
		

		private void Start() //Dynamic Resolution Settings
		{
			resolutions = Screen.resolutions;
			resolutionDropdown.ClearOptions();
			var options = new string[resolutions.Length];
			int index = -1;
			for (int i = 0; i < resolutions.Length; i++)
			{
				options[i] = $"{resolutions[i].width}x{resolutions[i].height}@{resolutions[i].refreshRate}hz";
				if (Screen.currentResolution.width == resolutions[i].width && Screen.currentResolution.height == resolutions[i].height)
					index = i;
			}
			resolutionDropdown.AddOptions(options.Select(str => new TMP_Dropdown.OptionData(str)).ToList());
			resolutionDropdown.value = index;
			resolutionDropdown.RefreshShownValue();
		}
		private void Awake()
		{
			hentaiDropdown.value = HentaiMode ? 1 : 0;
			hentaiDropdown.onValueChanged.AddListener(ChangedHentaiValue);
			fullScreenDropdown.value = InFullScreen ? 0 : 1;
			fullScreenDropdown.onValueChanged.AddListener(ChangedFullScreenValue);
			qualityDropdown.value = QualitySettings.GetQualityLevel();
			glow.value = BloomIntensity;
		}

		public void SetResolution (int resolutionIndex) // Apply resolutions to match Dropdown
		{
			Resolution resolution = resolutions[resolutionIndex];
			Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
		}
		public void ChangeIntensity(float value) => BloomIntensity = value;
		public void ChangeLevel(int value) // Graphics Quality
		{
			QualitySettings.SetQualityLevel(value);
			QualitySettings.renderPipeline = qualityLevels[value];
		} 
		public void ChangedHentaiValue(int option) => HentaiMode = option == 1;
		public void ChangedFullScreenValue(int fullScreen) => InFullScreen = fullScreen == 0;
		
	}
}