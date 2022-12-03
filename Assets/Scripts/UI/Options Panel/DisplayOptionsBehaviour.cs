namespace B1NARY.UI
{
	using System.Linq;
	using System.Collections.Generic;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Rendering;

	public sealed class DisplayOptionsBehaviour : MonoBehaviour
	{
		public bool HentaiMode { get => PlayerPrefsShortcuts.IsHentaiEnabled; set => PlayerPrefsShortcuts.IsHentaiEnabled = value; }
		public TMP_Dropdown hentaiDropdown;
		public FullScreenDropdown fullScreenDropdown;
		public RenderPipelineAsset[] qualityLevels;
		public TMP_Dropdown qualityDropdown;
		[SerializeField] private TMP_Dropdown resolutionDropdown;
		public TMP_Dropdown themeDropdown;
		public TMP_Dropdown languageDropdown;
		private Resolution[] AllResolutions => Screen.resolutions;

		private List<Resolution> filteredResolutions;
		private float CurrentRefreshRate => Screen.currentResolution.refreshRate;
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
			filteredResolutions = AllResolutions
				.Where(resolution => resolution.refreshRate == CurrentRefreshRate)
				.ToList();
			var sortedList = new List<Resolution>(filteredResolutions.Count);
			for (int i = 0; i < sortedList.Capacity; i++)
			{
				int reverseI = sortedList.Capacity - i - 1;
				sortedList.Add(filteredResolutions[reverseI]);
			}


			resolutionDropdown.ClearOptions();


			var options = new List<string>(filteredResolutions.Count);
			for (int i = 0; i < filteredResolutions.Count; i++)
			{
				string resolutionOption = $"{filteredResolutions[i].width}x{filteredResolutions[i].height} {filteredResolutions[i].refreshRate}Hz";
				options.Add(resolutionOption);
				if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
				{
					currentResolutionIndex = i;
				}
				
			}
			resolutionDropdown.AddOptions(options);
			resolutionDropdown.value = currentResolutionIndex;
			resolutionDropdown.RefreshShownValue();
		}
		private void Awake()
		{
			hentaiDropdown.value = HentaiMode ? 1 : 0;
			hentaiDropdown.onValueChanged.AddListener(ChangedHentaiValue);
			qualityDropdown.value = QualitySettings.GetQualityLevel();
			glow.value = BloomIntensity;
		}
		public void ChangeIntensity(float value) => BloomIntensity = value;
		public void ChangeLevel(int value) // Graphics Quality
		{
			QualitySettings.SetQualityLevel(value);
			QualitySettings.renderPipeline = qualityLevels[value];
		} 
		public void ChangedHentaiValue(int option) => HentaiMode = option == 1;
		
	}
}