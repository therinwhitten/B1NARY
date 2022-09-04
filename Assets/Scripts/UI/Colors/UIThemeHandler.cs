namespace B1NARY
{
	using System;
	using UnityEngine;
	using UnityEngine.Serialization;
	using UnityEngine.UI;
	using UI;
	using System.Text;
	using System.Linq;

	/// <summary>
	/// Allows you to easily change a single gameObject with the <see cref="Image"/>
	/// component to a color. By enabling, you change it via the parameters the
	/// component stores. Disabling reverts to it's previous known color.
	/// </summary>
	/// <seealso cref="MonoBehaviour"/>
	public class UIThemeHandler : MonoBehaviour
	{
		public const string resourcesColorThemePath = "UI/Color Themes";
		public enum Option
		{
			Primary,
			Secondary,
			Custom
		}
		public enum Target
		{
			Button,
			Image,
			Raw_Image,
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


		public static Color GetColor(Option option)
		{
			switch (option)
			{
				case Option.Primary:
					return CurrentlyEquippedFormat.primaryUI;
				case Option.Secondary:
					return CurrentlyEquippedFormat.SecondaryUI;
				case Option.Custom:
					throw new ArgumentException($"although {option} is a enum, you need" +
						$"another argument or name to differentiate other settings!");
				default:
					throw new IndexOutOfRangeException(option.ToString());
			}
		}
		public static Color GetColor(string name)
		{
			if (Enum.TryParse(name, out Option option))
				return GetColor(option);
			if (CurrentlyEquippedFormat.ExtraUIValues.TryGetValue(name, out Color color))
				return color;
			throw new NullReferenceException($"'{name}' is not located within the currently " +
				$"equipped format: {CurrentlyEquippedFormat.name}.");
		}

		public Image ImageData { get; private set; }
		public RawImage RawImageData { get; private set; }
		public string imageThemeName = Option.Secondary.ToString();

		public Button ButtonData { get; private set; }
		public string buttonHighlightedName = Option.Primary.ToString(),
			buttonPressedName = Option.Primary.ToString(),
			buttonSelectedName = Option.Primary.ToString(),
			buttonDisabledName = Option.Primary.ToString();

		private Target? m_currentTarget;
		public Target CurrentTarget 
		{ 
			get 
			{
				if (m_currentTarget.HasValue)
					return m_currentTarget.Value;

				if (TryGetComponent(out Button buttonOut))
				{
					ButtonData = buttonOut;
					m_currentTarget = Target.Button;
				}
				else if (TryGetComponent(out Image imageOut))
				{
					ImageData = imageOut;
					m_currentTarget = Target.Image;
				}
				else if (TryGetComponent(out RawImage rawImageOut))
				{
					RawImageData = rawImageOut;
					m_currentTarget = Target.RawImage;
				}
				else
				{
					var exceptionBuilder = new StringBuilder("There is no components to hold onto: ");
					foreach (Target @enum in Enum.GetValues(typeof(Target)))
						exceptionBuilder.Append($"{@enum}, ");
					exceptionBuilder.Append("are acceptable.");
					throw new MissingComponentException(exceptionBuilder.ToString());
				}
				return m_currentTarget.Value;
			}
			private set => m_currentTarget = value;
		}

		private ColorBlock previousButtonColors;
		private Color[] previousColors;

		public void OnEnable()
		{
			switch (CurrentTarget)
			{
				case Target.Image:
					previousColors = new Color[] { ImageData.color };
					break;
				case Target.RawImage:
					previousColors = new Color[] { RawImageData.color };
					break;
				case Target.Button:
					previousButtonColors = ButtonData.colors;
					break;
				default:
					throw new IndexOutOfRangeException(CurrentTarget.ToString());
			}
			UpdateColors(CurrentTarget);
		}
		public void UpdateColors(Target target)
		{
			switch (target)
			{
				case Target.Image:
					ImageData.color = GetColor(imageThemeName);
					return;
				case Target.RawImage:
					RawImageData.color = GetColor(imageThemeName);
					return;
				case Target.Button:
					ButtonData.colors = new ColorBlock()
					{
						colorMultiplier = ButtonData.colors.colorMultiplier,
						disabledColor = GetColor(buttonDisabledName),
						fadeDuration = ButtonData.colors.fadeDuration,
						highlightedColor = GetColor(buttonHighlightedName),
						normalColor = GetColor(imageThemeName),
						pressedColor = GetColor(buttonPressedName),
						selectedColor = GetColor(buttonSelectedName),
					};
					return;
				default:
					throw new IndexOutOfRangeException(CurrentTarget.ToString());
			}
		}

		public void OnDisable()
		{
			switch (CurrentTarget)
			{
				case Target.Button:
					ButtonData.colors = previousButtonColors;
					break;
				case Target.Image:
					ImageData.color = previousColors.Single();
					break;
				case Target.RawImage:
					RawImageData.color = previousColors.Single();
					break;
				default:
					throw new IndexOutOfRangeException(CurrentTarget.ToString());
			}
		}
	}
}