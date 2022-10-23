namespace B1NARY.UI
{
	using TMPro;
	using UnityEngine;
	using UnityEngine.Rendering;

	public sealed class DisplayOptionsBehaviour : MonoBehaviour
	{
		public bool HentaiMode { get => PlayerPrefsShortcuts.IsHentaiEnabled; set => PlayerPrefsShortcuts.IsHentaiEnabled = value; }
		public TMP_Dropdown hentaiDropdown;
		public bool InFullScreen { get => Screen.fullScreen; set => Screen.fullScreen = value; }
		public TMP_Dropdown fullScreenDropdown;
		public RenderPipelineAsset[] qualityLevels;
        public TMP_Dropdown dropdown;

        private void Awake()
		{
			hentaiDropdown.value = HentaiMode ? 1 : 0;
			hentaiDropdown.onValueChanged.AddListener(ChangedHentaiValue);
			fullScreenDropdown.value = InFullScreen ? 0 : 1;
			fullScreenDropdown.onValueChanged.AddListener(ChangedFullScreenValue);
			dropdown.value = QualitySettings.GetQualityLevel();
		}
	    
        public void ChangeLevel(int value) //Graphic Levels (Need to Fix)
		  {
             QualitySettings.SetQualityLevel(value);
             QualitySettings.renderPipeline = qualityLevels[value];
          } 
		public void ChangedHentaiValue(int option) => HentaiMode = option == 1;
		public void ChangedFullScreenValue(int fullScreen) => InFullScreen = fullScreen == 0;
		
         
		
        

	}
}