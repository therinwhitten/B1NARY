namespace B1NARY.UI
{
	using System.Collections;
    using System.Collections.Generic;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Rendering;

	public sealed class DisplayOptionsBehaviour : MonoBehaviour
	{
		public bool HentaiMode { get => PlayerPrefsShortcuts.IsHentaiEnabled; set => PlayerPrefsShortcuts.IsHentaiEnabled = value; }
		public TMP_Dropdown hentaiDropdown;
		public bool InFullScreen { get => Screen.fullScreen; set => Screen.fullScreen = value; }
		public TMP_Dropdown fullScreenDropdown;
		public RenderPipelineAsset[] qualityLevels;
        public TMP_Dropdown qualityDropdown;
		public TMPro.TMP_Dropdown resolutionDropdown;
         Resolution[] resolutions;

		 void Start ()
		 {
           resolutions = Screen.resolutions;
			resolutionDropdown.ClearOptions();
			List<string> options = new List<string>();
			int currentResolutionIndex = 0;
			for (int i = 0; i < resolutions.Length; i++)
			 {
			    string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + "hz";
				options.Add(option);

				if (resolutions[i].width == Screen.currentResolution.width &&
				    resolutions[i].height == Screen.currentResolution.height)
				{
					currentResolutionIndex = i;
				}						
			 }
			resolutionDropdown.AddOptions(resolutions);
			resolutionDropdown.value = currentResolutionIndex;
			resolutionDropdown.RefreshShownValue();
		 }
        private void Awake()
		{
			hentaiDropdown.value = HentaiMode ? 1 : 0;
			hentaiDropdown.onValueChanged.AddListener(ChangedHentaiValue);
			fullScreenDropdown.value = InFullScreen ? 0 : 1;
			fullScreenDropdown.onValueChanged.AddListener(ChangedFullScreenValue);
			qualityDropdown.value = QualitySettings.GetQualityLevel();
			
		}
	    
     public void ChangeLevel(int value) // Graphics Quality
	 {
        QualitySettings.SetQualityLevel(value);
        QualitySettings.renderPipeline = qualityLevels[value];
     } 
		public void ChangedHentaiValue(int option) => HentaiMode = option == 1;
		public void ChangedFullScreenValue(int fullScreen) => InFullScreen = fullScreen == 0;
		
         
		
        

	}
}