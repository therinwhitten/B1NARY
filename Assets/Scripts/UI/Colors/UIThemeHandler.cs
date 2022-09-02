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
			Image
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
			Debug.LogError($"'{name}' is not located within the currently " +
				$"equipped format: {CurrentlyEquippedFormat.name}, returning primary default.");
			return GetColor(Option.Primary);
		}

		#region Image Stuff
		public Image ImageData { get; private set; }
		public string imageThemeName = string.Empty;
		#endregion

		#region Button Stuff
		public Button ButtonData { get; private set; }
		public string buttonNormalName = string.Empty, buttonPressedName = string.Empty;
		#endregion

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

		private Color[] previousColors;

		private void OnEnable()
		{
			switch (CurrentTarget)
			{
				case Target.Image:
					previousColors = new Color[] { ImageData.color };
					ImageData.color = GetColor(imageThemeName);
					break;
				case Target.Button:
					previousColors = new Color[] 
					{ ButtonData.colors.normalColor, ButtonData.colors.pressedColor };
					ButtonData.colors = new ColorBlock()
					{
						colorMultiplier = ButtonData.colors.colorMultiplier,
						disabledColor = ButtonData.colors.disabledColor,
						fadeDuration = ButtonData.colors.fadeDuration,
						highlightedColor = ButtonData.colors.highlightedColor,
						normalColor = GetColor(buttonNormalName),
						pressedColor = GetColor(buttonPressedName),
						selectedColor = ButtonData.colors.selectedColor,
					};
					break;
				default:
					throw new IndexOutOfRangeException(CurrentTarget.ToString());
			}
		}
		private void OnDisable()
		{
			switch (CurrentTarget)
			{
				case Target.Button:
					ButtonData.colors = new ColorBlock()
					{
						colorMultiplier = ButtonData.colors.colorMultiplier,
						disabledColor = ButtonData.colors.disabledColor,
						fadeDuration = ButtonData.colors.fadeDuration,
						highlightedColor = ButtonData.colors.highlightedColor,
						normalColor = previousColors[0],
						pressedColor = previousColors[1],
						selectedColor = ButtonData.colors.selectedColor,
					};
					break;
				case Target.Image:
					ImageData.color = previousColors.Single();
					break;
				default:
					throw new IndexOutOfRangeException(CurrentTarget.ToString());
			}
		}
	}
}