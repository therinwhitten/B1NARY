﻿namespace B1NARY
{
	using System;
	using UnityEngine;
	using TMPro;
	using UnityEngine.UI;
	using UI;
	using System.Text;
	using System.Linq;
	using System.Reflection;
	using B1NARY.DesignPatterns;
	using System.Collections.Generic;

	/// <summary>
	/// Allows you to easily change a single gameObject with the <see cref="Image"/>
	/// component to a color. By enabling, you change it via the parameters the
	/// component stores. Disabling reverts to it's previous known color.
	/// </summary>
	/// <seealso cref="MonoBehaviour"/>
	public class UIThemeHandler : Multiton<UIThemeHandler>
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
		public static void ChangeTheme(string themeName)
		{
			themeName = resourcesColorThemePath + themeName;
			_currentlyEquippedFormat = Resources.Load<ColorFormat>(themeName);
			if (Application.isPlaying)
			{
				IEnumerator<UIThemeHandler> themeHandlers = GetEnumerator();
				while (themeHandlers.MoveNext())
					themeHandlers.Current.UpdateColors();
			}
		}
		public void ChangeThemeInstance(string themeName) => ChangeTheme(themeName);


		public string imageThemeName = Option.Secondary.ToString(),
			buttonHighlightedName = Option.Primary.ToString(),
			buttonPressedName = Option.Primary.ToString(),
			buttonSelectedName = Option.Primary.ToString(),
			buttonDisabledName = Option.Primary.ToString();

		/// <summary>
		/// Contains the first <see cref="Color"/> or <see cref="ColorBlock"/>,
		/// these are always value types, so these are referenced via Func.
		/// </summary>
		public Ref<object> ColorEdit 
		{
			get
			{
				if (m_colorEdit == null)
					_ = CurrentTarget;
				return m_colorEdit;
			} 
			private set => m_colorEdit = value; 
		}
		private Ref<object> m_colorEdit;

		public Component CurrentTarget 
		{ 
			get 
			{
				if (m_currentTarget != null)
					return m_currentTarget;
				Component[] components = GetComponents<Component>();
				for (int i = 0; i < components.Length; i++)
				{
					// Get any color properties in the component that has any color parameter.
					var colorEnum = components[i].GetType().GetProperties(BindingFlags.Public).Where(info => info.PropertyType == typeof(Color));
					if (colorEnum.Any())
					{
						ColorEdit = new Ref<object>(() => colorEnum.Single(), set => colorEnum.Single().SetValue(components[i], set));
						m_currentTarget = components[i];
						return m_currentTarget;
					}
					// Get any color block properties in the component that has any color block parameter.
					var colorBlockEnum = components[i].GetType().GetProperties(BindingFlags.Public).Where(info => info.PropertyType == typeof(ColorBlock));
					if (colorBlockEnum.Any())
					{
						ColorEdit = new Ref<object>(() => colorBlockEnum.Single(), set => colorBlockEnum.Single().SetValue(components[i], set));
						m_currentTarget = components[i];
						return m_currentTarget;
					}
				}
				throw new MissingComponentException("There is no components " +
					$"to hold onto that has a {nameof(Color)} or {nameof(ColorBlock)}");
			}
		}
		private Component m_currentTarget;


		protected override void MultitonAwake()
		{
			UpdateColors();
		}
		public void UpdateColors()
		{
			if (ColorEdit.Value is Color)
				ColorEdit.Value = GetColor(imageThemeName);
			else if (ColorEdit.Value is ColorBlock)
				ColorEdit.Value = new ColorBlock()
				{
					colorMultiplier = ((ColorBlock)ColorEdit.Value).colorMultiplier,
					disabledColor = GetColor(buttonDisabledName),
					fadeDuration = ((ColorBlock)ColorEdit.Value).fadeDuration,
					highlightedColor = GetColor(buttonHighlightedName),
					normalColor = GetColor(imageThemeName),
					pressedColor = GetColor(buttonPressedName),
					selectedColor = GetColor(buttonSelectedName),
				};
			else
				throw new IndexOutOfRangeException(CurrentTarget.ToString());
		}
	}
}