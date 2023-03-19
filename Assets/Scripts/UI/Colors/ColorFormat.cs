namespace B1NARY.UI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using B1NARY.Scripting;
	using OVSXmlSerializer;
	using System.Xml;
	using HideousDestructor.DataPersistence;
	using System.IO;


	// Have it change formats per scene if it exists.
	[Serializable]
	public class ColorFormat
	{
		public const string DEFAULT_THEME_NAME = "Default";
		public static DirectoryInfo RootPath => SerializableSlot.StreamingAssets.CreateSubdirectory("Color Themes");
		public static DirectoryInfo CustomThemePath => RootPath.CreateSubdirectory("Custom");
		public static FileInfo DefaultThemePath => RootPath.GetFile("Default.xml");
		internal readonly static IndexFile indexFile = IndexFile.LoadNew();
		public static CommandArray Commands = new CommandArray()
		{
			["colorformat"] = (Action<string>)(newColorFormat =>
			{
				Set(newColorFormat);
			}),
		};


		internal static XmlSerializer<ColorFormat> FormatSerializer = new XmlSerializer<ColorFormat>();


		static ColorFormat()
		{
			if (!DefaultThemePath.Exists)
			{
				m_defaultTheme = new ColorFormat()
				{
					FormatName = DEFAULT_THEME_NAME
				};
				m_defaultTheme.Save(DefaultThemePath.Name, true);
			}
			else
				m_defaultTheme = FormatSerializer.Deserialize(DefaultThemePath);
		}

		public static ColorFormat DefaultTheme
		{
			get => m_defaultTheme;
			set
			{
				m_defaultTheme = value;
				m_defaultTheme.Save(DefaultThemePath.Name, true);
			}
		}
		private static ColorFormat m_defaultTheme;
		public static event Action<ColorFormat> ChangedTheme;
		public static ColorFormat CurrentTheme
		{
			get
			{
				if (m_currentTheme is null)
				{
					string themeName = B1NARYConfig.Graphics.Theme;
					if (string.IsNullOrWhiteSpace(themeName))
						m_currentTheme = DefaultTheme;
					else if (!Set(themeName))
						m_currentTheme = DefaultTheme;
				}
				return m_currentTheme;
			}
			set => Set(value);
		}
		private static ColorFormat m_currentTheme;
		public static void Override(string themeName)
		{
			if (Set(themeName))
				B1NARYConfig.Graphics.Theme = themeName;
		}
		public static void Override(ColorFormat theme)
		{
			Set(theme);
			B1NARYConfig.Graphics.Theme = theme.FormatName;
		}
		public static bool Set(string themeName)
		{
			for (int i = 0; i < AvailableFormats.Count; i++)
				if (AvailableFormats[i].format.FormatName == themeName)
				{
					Set(AvailableFormats[i].format);
					return true;
				}
			return false;
		}
		public static void Set(ColorFormat theme)
		{
			m_currentTheme = theme;
			ChangedTheme?.Invoke(theme);
		}
		public static bool HasOverride => !string.IsNullOrEmpty(B1NARYConfig.Graphics.Theme);
		public static void RemoveOverride()
		{
			B1NARYConfig.Graphics.Theme = null;
			Set(DefaultTheme);
		}


		internal static List<(FileInfo fileInfo, ColorFormat format)> AllFormats =>
			new List<(FileInfo fileInfo, ColorFormat format)>(AvailableFormats)
			{
				(DefaultThemePath, DefaultTheme)
			};
		/// <summary>
		/// All formats that the system that can access
		/// </summary>
		public static IReadOnlyList<(FileInfo fileInfo, ColorFormat format)> AvailableFormats
		{
			get
			{
				if (m_availableFormats is null)
				{
					var enumerable = CustomThemePath.GetFiles().Where(file => file.Extension == ".xml");
					m_availableFormats = new List<(FileInfo fileInfo, ColorFormat format)>();
					using (var enumerator = enumerable.GetEnumerator())
						while (enumerator.MoveNext())
						{
							FileInfo fileInfo = enumerator.Current;
							try
							{
								ColorFormat format = FormatSerializer.Deserialize(fileInfo);
								m_availableFormats.Add((fileInfo, format));
							}
							catch (Exception ex)
							{
								Debug.LogException(ex);
							}
						}
				}
				return m_availableFormats;
			}
		}
		internal static List<(FileInfo fileInfo, ColorFormat format)> m_availableFormats;
		/// <summary>
		/// All formats that can be added by the options menu w/ overrides
		/// </summary>
		public static IReadOnlyList<ColorFormat> PlayerFormats
		{
			get
			{
				if (m_playerFormats is null)
				{
					m_playerFormats = new List<ColorFormat>();
					HashSet<string> names = new HashSet<string>(indexFile.files);
					var allFormats = AllFormats;
					for (int i = 0; i < allFormats.Count; i++)
					{
						ColorFormat format = allFormats[i].format;
						if (names.Contains(format.FormatName))
							m_playerFormats.Add(format);
					}
				}
				return m_playerFormats;
			}
			set
			{
				m_playerFormats = new List<ColorFormat>(value);
				indexFile.files = value.Select(format => format.FormatName).ToList();
				indexFile.Save();
			}
		}
		internal static List<ColorFormat> m_playerFormats;

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

		// INSTANCE ---------------------------

		/// <summary>
		/// 
		/// </summary>
#warning TODO: Assign it later on build 1.1.2!
		//[XmlAttribute("name")]
		public string FormatName;
		/// <summary>
		/// The color format version of the color format.
		/// </summary>
		[XmlAttribute("ver")]
		public int Version = 1;
		/// <summary>
		/// The primary color to be used by all UI. Defaulted to this color by
		/// default if something happens.
		/// </summary>
		[XmlNamedAs("Primary")]
		public Color primaryUI = new Color(ToPercent(47), ToPercent(161), ToPercent(206));
		/// <summary>
		/// The secondary color to be used by all UI. The yang to the 
		/// <see cref="primaryUI"/> ying.
		/// </summary>
		[XmlNamedAs("Secondary")]
		public Color SecondaryUI = new Color(ToPercent(47), ToPercent(206), ToPercent(172));
		/// <summary>
		/// 
		/// </summary>
		[XmlNamedAs("Extras")]
		public Dictionary<string, Color> ExtraUIColors = new Dictionary<string, Color>();

		/// <summary>
		/// 
		/// </summary>
		public ColorFormat()
		{

		}

		// OTHER STRUCTS -------------------

		public class IndexFile : IXmlSerializable
		{
			public static FileInfo IndexPath => RootPath.GetFile("PlayerThemeIndex.xml");
			private static XmlSerializer<IndexFile> indexerFormatter = new XmlSerializer<IndexFile>();
			public static IndexFile LoadNew()
			{
				if (IndexPath.Exists)
					return indexerFormatter.Deserialize(IndexPath);
				else
					return new IndexFile();
			}
			public List<string> files = new List<string>();

			bool IXmlSerializable.ShouldWrite => true;
			void IXmlSerializable.Read(XmlNode value)
			{
				files = new List<string>(value.ChildNodes.Count);
				for (int i = 0; i < files.Capacity; i++)
					files.Add(value.ChildNodes[i].InnerText);
			}
			void IXmlSerializable.Write(XmlWriter writer)
			{
				for (int i = 0; i < files.Count; i++)
				{
					writer.WriteElementString("File", files[i]);
				}
			}
			public void Save()
			{
				using (var stream = IndexPath.Open(FileMode.Create, FileAccess.Write))
					indexerFormatter.Serialize(stream, this);
			}
		}

		public void Save(string fileName, bool isDefault = false)
		{
			if (isDefault)
			{
				using (var stream = DefaultThemePath.Open(FileMode.Create, FileAccess.Write))
					FormatSerializer.Serialize(stream, this, "DefaultFormat");
				return;
			}
			using (var stream = CustomThemePath.GetFile(fileName).Open(FileMode.Create, FileAccess.Write))
				FormatSerializer.Serialize(stream, this, "ColorFormat");
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.Editor
{
	using UnityEngine;
	using UnityEditor;
	using B1NARY.UI;
	using B1NARY.Editor.Debugger;
	using System.Linq;
	using System.Collections.Generic;
	using System.IO;

	public class ColorFormatWindow : EditorWindow
	{
		private static readonly Vector2Int defaultMinSize = new Vector2Int(300, 350);
		[MenuItem("B1NARY/Color Format Editor", priority = 1)]
		public static void ShowWindow()
		{
			// Get existing open window or if none, make a new one:
			ColorFormatWindow window = GetWindow<ColorFormatWindow>();
			window.titleContent = new GUIContent("Color Format Editor");
			window.minSize = defaultMinSize;
			window.Show();
		}

		private bool bruh = false;
		private int selection = 0;
		private string newItemName = "";

		private void OnGUI()
		{
			Rect fullRect = GUILayoutUtility.GetRect(Screen.width, 20f);
			Rect leftRect = new Rect(fullRect) { width = fullRect.width / 3 };
			Rect rightRect = new Rect(fullRect) { xMin = leftRect.xMax + 2 };
			List<(FileInfo fileInfo, ColorFormat format)> allFormats = ColorFormat.AllFormats;
			newItemName = EditorGUI.TextField(rightRect, newItemName);
			if (GUILayout.Button("Reset Metadata"))
			{
				ColorFormat.m_availableFormats = null;
				ColorFormat.m_playerFormats = null;
			}
			if (GUI.Button(leftRect, "New"))
			{
				new ColorFormat() { FormatName = newItemName, }.Save($"{newItemName}.xml");
				ColorFormat.m_availableFormats = null;
				ColorFormat.m_playerFormats = null;
			}
			if (bruh = EditorGUILayout.BeginFoldoutHeaderGroup(bruh, "Player-Defined Themes"))
			{
				EditorGUI.indentLevel++;
				ColorFormat.IndexFile file = ColorFormat.indexFile;
				for (int i = 0; i < allFormats.Count; i++)
				{
					ColorFormat format = allFormats[i].format;
					bool oldValue = file.files.Contains(format.FormatName);
					if (EditorGUILayout.Toggle(format.FormatName, oldValue) != oldValue)
					{
						bool newValue = !oldValue;
						if (newValue)
							file.files.Add(format.FormatName);
						else
							file.files.Remove(format.FormatName);
						file.Save();
					}
				}
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
			selection = EditorGUILayout.Popup(selection, allFormats.Select(pair => pair.format.FormatName).ToArray());
			ColorFormat currentFormat = allFormats[selection].format;
			FileInfo fileInfo = allFormats[selection].fileInfo;
			if (!ReferenceEquals(currentFormat, ColorFormat.DefaultTheme))
				currentFormat.FormatName = EditorGUILayout.TextField("Name", currentFormat.FormatName);
			EditorGUI.indentLevel++;
			currentFormat.primaryUI = EditorGUILayout.ColorField("Primary Color", currentFormat.primaryUI);
			currentFormat.SecondaryUI = EditorGUILayout.ColorField("Secondary Color", currentFormat.SecondaryUI);
			var editable = new DictionaryEditor<string, Color>(currentFormat.ExtraUIColors)
			{
				defaultValue = Color.black,
				defaultKey = ""
			};
			if (editable.Repaint())
				currentFormat.ExtraUIColors = editable.Flush();
			if (GUILayout.Button("Save"))
			{
				currentFormat.Save(fileInfo.Name, ReferenceEquals(currentFormat, ColorFormat.DefaultTheme));
				ColorFormat.m_availableFormats = null;
				ColorFormat.m_playerFormats = null;
			}
			EditorGUI.indentLevel--;
		}
		/*
		ColorFormat _colorFormat;
		private ColorFormat ColorFormat
		{
			get
			{
				if (_colorFormat == null)
				{
					_colorFormat = (ColorFormat)target;
				}
				return _colorFormat;
			}
		}
		private bool openedHeader = true;
		public override void OnInspectorGUI()
		{
			EditorUtility.SetDirty(ColorFormat);
			ColorFormat.primaryUI = DirtyAuto.Field(ColorFormat, new GUIContent("Primary Color"), ColorFormat.primaryUI);
			ColorFormat.SecondaryUI = DirtyAuto.Field(ColorFormat, new GUIContent("Secondary Color"), ColorFormat.SecondaryUI);
			if (openedHeader = EditorGUILayout.BeginFoldoutHeaderGroup(openedHeader, new GUIContent("Extra UI Color Data")))
			{
				EditorGUI.indentLevel++;
				var editable = new DictionaryEditor<string, Color>(ColorFormat.ExtraUIValues)
				{
					defaultValue = Color.black,
					defaultKey = ""
				};
				if (editable.Repaint())
				{
					ColorFormat.ExtraUIValues = editable.Flush();
					EditorUtility.SetDirty(ColorFormat);
				}
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}
		*/
	}
}
#endif