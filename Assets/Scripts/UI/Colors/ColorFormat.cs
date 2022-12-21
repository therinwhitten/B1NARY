namespace B1NARY.UI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using B1NARY.DataPersistence;
	using System.Runtime.Remoting;
	using System.Threading;
	using B1NARY.Scripting;


	// Have it change formats per scene if it exists.
	[Serializable]
	[CreateAssetMenu(fileName = "New UI Color Format", menuName = "B1NARY/Color UI Format", order = 1)]
	public class ColorFormat : ScriptableObject, IEquatable<ColorFormat>
	{
		// Add command array for selection.
		public static CommandArray Commands = new CommandArray()
		{
			["colorformat"] = (Action<string>)(newColorFormat =>
			{
				SoftOverride(newColorFormat);
			}),
		};

		/// <summary>
		/// Adds <see cref="resourcesColorThemePath"/> and the added path into
		/// it's own directory. Using <see cref="Resources.Load(string)"/> with
		/// this is compatible and intended.
		/// </summary>
		/// <param name="path">The file name that is expected to be <see cref="ColorFormat"/>. </param>
		/// <returns> A resources path to use. </returns>
		public static string ResourcesPath(string path) => $"{resourcesColorThemePath}/{path}";
		/// <summary>
		/// A path in the resources folder as reference for color themes.
		/// </summary>
		public const string resourcesColorThemePath = "UI/Color Themes";

		/// <summary>
		/// The key for saving <see cref="PlayerPrefs"/>.
		/// </summary>
		private const string persistenceKey = "B1NARYColorFormat";
		/// <summary>
		/// The name of the <see cref="ColorFormat"/> to default to if there is 
		/// none overrided.
		/// </summary>
		public const string defaultKey = "Default";
		/// <summary>
		/// Gets the currently saved overrided theme name, expected for use in 
		/// <see cref="ResourcesPath(string)"/>. Returns <see cref="defaultKey"/>
		/// if there is no key present, or <see cref="HasOverridedTheme"/> is 
		/// false.
		/// </summary>
		public static string OverridedTheme
		{
			get => PlayerPrefs.GetString(persistenceKey, defaultKey);
			set
			{
				if (m_currentFormat != null)
					CurrentFormat = Resources.Load<ColorFormat>(ResourcesPath(value));
				PlayerPrefs.SetString(persistenceKey, value);
			}
		}
		/// <summary>
		/// Modifies the current theme to the specified <paramref name="themeName"/>
		/// and saved as a player preference.
		/// </summary>
		/// <param name="themeName"> The new theme to change to and persist. </param>
		public static void HardOverride(string themeName)
		{
			OverridedTheme = themeName;
		}
		/// <summary>
		/// Checks if it has a theme in store. Setting it to <see langword="true"/>
		/// will cause to create a new key with <see cref="defaultKey"/>, or
		/// <see cref="CurrentFormat"/> if it is currently active.
		/// </summary>
		public static bool HasOverridedTheme
		{
			get => PlayerPrefs.HasKey(persistenceKey);
			set
			{
				if (value == HasOverridedTheme)
					return;
				if (value)
					PlayerPrefs.SetString(persistenceKey, m_currentFormat == null ? defaultKey : m_currentFormat.name);
				else
					PlayerPrefs.DeleteKey(persistenceKey);
				CurrentFormat = Resources.Load<ColorFormat>(ResourcesPath(OverridedTheme));
			}
		}

		private static ColorFormat m_currentFormat;
		/// <summary>
		/// when <see cref="CurrentFormat"/> is modified in any way.
		/// </summary>
		public static event Action<ColorFormat> CurrentFormatChanged;
		/// <summary>
		/// The currently active format that is equipped for all objects to see.
		/// </summary>
		public static ColorFormat CurrentFormat
		{
			get
			{
				if (!Application.isPlaying)
					throw new InvalidOperationException();
				if (m_currentFormat == null)
					m_currentFormat = Resources.Load<ColorFormat>(ResourcesPath(defaultKey));
				return m_currentFormat;
			}
			private set
			{
				m_currentFormat = value;
				CurrentFormatChanged?.Invoke(m_currentFormat);
			}
		}

		/// <summary>
		/// This modifies the currently used format, if it does not hold a theme
		/// such as the <see cref="OverridedTheme"/>, then it will set to that
		/// active theme.
		/// </summary>
		/// <returns><see cref="HasOverridedTheme"/></returns>
		public static bool SoftOverride(string themeName)
		{
			if (HasOverridedTheme)
				return false;
			CurrentFormat = Resources.Load<ColorFormat>(ResourcesPath(themeName));
			return true;
		}


		/// <summary>
		/// Converts a byte to a float ranging from 0 to 1 into 0 to 255.
		/// </summary>
		public static float ToPercent(byte value)
		{
			return (float)value / byte.MaxValue;
		}
		/// <summary>
		/// Converts a float ranging from 0 to 1 into 0 to 255.
		/// </summary>
		public static byte ToByte(float percent)
		{
			return Convert.ToByte(percent * byte.MaxValue);
		}

		/// <summary>
		/// The primary color to be used by all UI. Defaulted to this color by
		/// default if something happens.
		/// </summary>
		public Color primaryUI = new Color(ToPercent(47), ToPercent(161), ToPercent(206));
		/// <summary>
		/// The secondary color to be used by all UI. The yang to the 
		/// <see cref="primaryUI"/> ying.
		/// </summary>
		public Color SecondaryUI = new Color(ToPercent(47), ToPercent(206), ToPercent(172));

		/// <summary>
		/// uses the setter/getter to modify values and notify the color format
		/// to change states.
		/// </summary>
		public IReadOnlyDictionary<string, Color> ExtraUIValues 
		{ 
			get => m_extraUIValues; set
			{
				savedKeys = value.Keys.ToArray();
				savedValues = value.Values.ToArray();
				m_extraUIValues = value;
			}
		}
		private IReadOnlyDictionary<string, Color> m_extraUIValues;

		/// <summary>
		/// The keys of the keyValuePair list. Separated due to the serialization 
		/// gods being angry at us for making our lives easier. Use 
		/// <see cref="SavedPairs"/> for the pairs.
		/// </summary>
		public string[] savedKeys = Array.Empty<string>();
		/// <summary>
		/// The values of the keyValuePair list. Separated due to the serialization 
		/// gods being angry at us for making our lives easier. Use 
		/// <see cref="SavedPairs"/> for the pairs.
		/// </summary>
		public Color[] savedValues = Array.Empty<Color>();
		/// <summary>
		/// The KeyValuePairs to modify. Its set as read only due to property 
		/// magic.
		/// </summary>
		public IReadOnlyList<KeyValuePair<string, Color>> SavedPairs
		{
			get => savedKeys.Zip(savedValues, (left, right) => new KeyValuePair<string, Color>(left, right)).ToList();
			set
			{
				savedKeys = new string[value.Count];
				savedValues = new Color[value.Count];
				for (int i = 0; i < value.Count; i++)
				{
					savedKeys[i] = value[i].Key;
					savedValues[i] = value[i].Value;
				}
				m_extraUIValues = SavedPairs.ToDictionary(pair => pair.Key, pair => pair.Value);
			}
		}

		/// <summary>
		/// On Deserialization/use.
		/// </summary>
		private void OnEnable()
		{
			m_extraUIValues = SavedPairs.ToDictionary(pair => pair.Key, pair => pair.Value);
		}

		public bool Equals(ColorFormat other)
		{
			return primaryUI == other.primaryUI &&
				SecondaryUI == other.SecondaryUI &&
				name == other.name &&
				ExtraUIValues == other.ExtraUIValues;
		}
	}
}
