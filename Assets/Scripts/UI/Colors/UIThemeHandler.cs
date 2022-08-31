namespace B1NARY
{
	using System;
	using UnityEngine;
	using UnityEngine.Serialization;
	using UnityEngine.UI;
	using UI;

	/// <summary>
	/// Allows you to easily change a single gameObject with the <see cref="Image"/>
	/// component to a color. By enabling, you change it via the parameters the
	/// component stores. Disabling reverts to it's previous known color.
	/// </summary>
	/// <seealso cref="MonoBehaviour"/>
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

		private static ColorFormat _currentlyEquippedFormat;
		public static ColorFormat CurrentlyEquippedFormat
		{
			get
			{
				if (_currentlyEquippedFormat == null)
				{
					string formatName = PlayerPrefs.GetString(nameof(CurrentlyEquippedFormat), string.Empty);
					if (string.IsNullOrEmpty(formatName) == false)
						CurrentlyEquippedFormat = Resources.Load<ColorFormat>($"{resourcesColorThemePath}/{formatName}");
					else
						_currentlyEquippedFormat = Resources.Load<ColorFormat>($"{resourcesColorThemePath}/Default");
				}
				return _currentlyEquippedFormat;
			}
			set
			{
				PlayerPrefs.SetString(nameof(CurrentlyEquippedFormat), value.name);
				_currentlyEquippedFormat = value;
			}
		}


		public static void ChangeColor(Image image, Option option = Option.Primary)
		{
			switch (option)
			{
				case Option.Primary:
					image.color = CurrentlyEquippedFormat.primaryUI;
					break;
				case Option.Secondary:
					image.color = CurrentlyEquippedFormat.SecondaryUI;
					break;
				case Option.Custom:
				default:
					throw new IndexOutOfRangeException(option.ToString());
			}
		}
		public static void ChangeColor(Image image, string name)
		{
			if (CurrentlyEquippedFormat.ExtraUIValues.TryGetValue(name, out Color color))
			{
				image.color = color;
				return;
			}
			Debug.LogError($"'{name}' is not located within the currently " +
				$"equipped format: {CurrentlyEquippedFormat.name}, resorting to default.", image.gameObject);
			ChangeColor(image);
		}

		[FormerlySerializedAs("Theme Option"), Tooltip("The amogus")]
		public Option option = Option.Primary;
		[Tooltip("What the name of the custom theme color uses")]
		public string themeName = string.Empty;
		private Image image;

		private Color previousColor;

		private void Awake()
		{
			image = GetComponent<Image>();
		}
		private void OnEnable()
		{
			previousColor = image.color;
			if (option == Option.Custom)
				ChangeColor(image, themeName);
			else
				ChangeColor(image, option);
		}
		private void OnDisable()
		{
			image.color = previousColor;
		}
	}
}