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
		[SerializeField] private TMP_Dropdown resolutionDropdown;
		public TMP_Dropdown themeDropdown;
		public TMP_Dropdown languageDropdown;
		private Resolution[] resolutions;

		private List<Resolution> filteredResolutions;
		private float currentRefreshRate;
		private int currentResolutionIndex = 0;
		[SerializeField] Slider glow;
		[SerializeField] Volume volumeProfile;
		public float BloomIntensity
		{
			get => volumeProfile.profile.components.Single().parameters[1].GetValue<float>();
			set => ((VolumeParameter<float>)volumeProfile.profile.components.Single().parameters[1]).value = value;
		}
		
		
		

		void Start() //Dynamic Resolution Settings
		{
			resolutions = Screen.resolutions;
			filteredResolutions = new List<Resolution>();

			resolutionDropdown.ClearOptions();
			currentRefreshRate = Screen.currentResolution.refreshRate;

			for (int i = 0; i < resolutions.Length; i++)
			{
				if (resolutions[i].refreshRate == currentRefreshRate)
				{
					filteredResolutions.Add(resolutions[i]);
				}
			}

			List<string> options = new List<string>();
			for (int i = 0; i < filteredResolutions.Count; i++)
			{
				string resolutionOption =filteredResolutions[i].width + "x" +filteredResolutions[i].height + " " +filteredResolutions[i].refreshRate + "Hz";
				options.Add(resolutionOption);
				if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
				{
					currentResolutionIndex = i;
				}
				resolutionDropdown.AddOptions(options);
				resolutionDropdown.value = currentResolutionIndex;
				resolutionDropdown.RefreshShownValue();
			}
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
			Resolution resolution = filteredResolutions[resolutionIndex];
			Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.ExclusiveFullScreen);
			Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.FullScreenWindow);
			Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.Windowed);

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