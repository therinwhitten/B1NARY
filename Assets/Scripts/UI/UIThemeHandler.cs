namespace B1NARY
{
	using System;
	using UnityEngine;
	using UnityEngine.Serialization;
	using UnityEngine.UI;
	using UI;

	[RequireComponent(typeof(Image))]
	public class UIThemeHandler : MonoBehaviour
	{
		public const string resourcesColorThemePath = "UI/Color Themes";
		public enum Option
		{
			Primary,
			Secondary,
			Custom
		}

		private static ResourcesAsset<ColorFormat> _currentlyEquippedFormat;
		public static ResourcesAsset<ColorFormat> CurrentlyEquippedFormat
		{
			get
			{
				if (_currentlyEquippedFormat == null)
				{
					string formatName = PlayerPrefs.GetString(nameof(CurrentlyEquippedFormat), string.Empty);
					if (!string.IsNullOrEmpty(formatName))
						CurrentlyEquippedFormat = new ResourcesAsset<ColorFormat>($"{resourcesColorThemePath}/{formatName}");
					else
						_currentlyEquippedFormat = new ResourcesAsset<ColorFormat>($"{resourcesColorThemePath}/Default");
				}
				return _currentlyEquippedFormat;
			}
			set
			{
				PlayerPrefs.SetString(nameof(CurrentlyEquippedFormat), value.target.name);
				_currentlyEquippedFormat = value;
			}
		}


		public static void ChangeColor(Image image)
			=> ChangeColor(image, Option.Primary);
		public static void ChangeColor(Image image, Option option)
		{
			switch (option)
			{
				case Option.Primary:
					image.color = CurrentlyEquippedFormat.target.primaryUI;
					break;
				case Option.Secondary:
					image.color = CurrentlyEquippedFormat.target.SecondaryUI;
					break;
				case Option.Custom:
				default:
					throw new IndexOutOfRangeException(option.ToString());
			}
		}
		public static void ChangeColor(Image image, string name)
		{
			image.color = CurrentlyEquippedFormat.target.ExtraUIValues[name];
		}

		[FormerlySerializedAs("Theme Option"), Tooltip("The amogus")]
		public Option option = Option.Primary;
		[Tooltip("What the name of the custom theme color uses")]
		public string themeName = string.Empty;

		private void Start()
		{
			if (option == Option.Custom)
				ChangeColor(GetComponent<Image>(), themeName);
			else
				ChangeColor(GetComponent<Image>(), option);
		}
	}
}