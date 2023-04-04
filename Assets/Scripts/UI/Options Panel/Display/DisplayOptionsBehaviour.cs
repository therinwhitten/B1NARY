namespace B1NARY.UI
{
	using System.Linq;
	using System.Collections.Generic;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Rendering;
	using System;

	public sealed class DisplayOptionsBehaviour : MonoBehaviour
	{
		public TMP_Dropdown hentaiDropdown;
		public FullScreenDropdown fullScreenDropdown;
		public RenderPipelineAsset[] qualityLevels;
		public TMP_Dropdown qualityDropdown;
		[SerializeField] private TMP_Dropdown resolutionDropdown;
		public TMP_Dropdown themeDropdown;
		public TMP_Dropdown languageDropdown;
		[SerializeField] private Slider glowSlider;
		[SerializeField] private TextMeshProUGUI glowText;
		[SerializeField] private Slider fpsSlider;
		[SerializeField] private TextMeshProUGUI fpsText;
		
		
		
		private void Awake()
		{
			// H-Scene, already implemented by another class
			//hentaiDropdown.value = Convert.ToInt32(B1NARYConfig.HEnable);
			//hentaiDropdown.onValueChanged.AddListener((value) => B1NARYConfig.HEnable.Value = Convert.ToBoolean(value));
			// Quality
			qualityDropdown.value = QualitySettings.GetQualityLevel();
			qualityDropdown.onValueChanged.AddListener((value) =>
			{
				QualitySettings.SetQualityLevel(value);
				QualitySettings.renderPipeline = qualityLevels[value];
			});
			// Glow
			B1NARYConfig.Graphics.Glow.AttachValue((value) => glowText.text = value.ToString("N0"));
			glowSlider.value = B1NARYConfig.Graphics.Glow.Value;
			glowSlider.onValueChanged.AddListener((value) => B1NARYConfig.Graphics.Glow.Value = value);
			// FPS
			B1NARYConfig.Graphics.FrameRate.AttachValue((value) => fpsText.text = value.ToString("N0"));
			fpsSlider.value = B1NARYConfig.Graphics.FrameRate.Value;
			fpsSlider.onValueChanged.AddListener((value) => B1NARYConfig.Graphics.FrameRate.Value = (int)value);
		}
	}
}