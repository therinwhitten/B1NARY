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
		[SerializeField] Slider glow;
		private GlowVolumeInterface glowVolumeInterface;
		[SerializeField] private Slider glowSlider;
		[SerializeField] private TextMeshProUGUI glowText;
		[SerializeField] private Slider fpsSlider;
		[SerializeField] private TextMeshProUGUI fpsText;
		
		
		
		private void Awake()
		{
			hentaiDropdown.value = HentaiMode ? 1 : 0;
			hentaiDropdown.onValueChanged.AddListener(ChangedHentaiValue);
			qualityDropdown.value = QualitySettings.GetQualityLevel();
			glowVolumeInterface = FindObjectOfType<GlowVolumeInterface>();
			glow.value = glowVolumeInterface.BloomIntensity;
		}
		private void Start() //Glow Text Value
		{
			B1NARYConfig.Graphics.Glow.AttachValue((value) => glowText.text = value.ToString("N0"));
			glowSlider.value = B1NARYConfig.Graphics.FrameRate.Value;
			glowSlider.onValueChanged.AddListener((value) => B1NARYConfig.Graphics.FrameRate.Value = (int)value);
			B1NARYConfig.Graphics.FrameRate.AttachValue((value) => fpsText.text = value.ToString("N0"));
			fpsSlider.value = B1NARYConfig.Graphics.FrameRate.Value;
			fpsSlider.onValueChanged.AddListener((value) => B1NARYConfig.Graphics.FrameRate.Value = (int)value);
		}
   
		public void ChangeIntensity(float value) => glowVolumeInterface.BloomIntensity = value;
		public void ChangeLevel(int value) // Graphics Quality
		{
			QualitySettings.SetQualityLevel(value);
			QualitySettings.renderPipeline = qualityLevels[value];
		} 
		public void ChangedHentaiValue(int option) => HentaiMode = option == 1;
		
	}
}