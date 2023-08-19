﻿namespace B1NARY.UI.Colors
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using B1NARY.Scripting;
	using System.IO;
	using FormatSerializer = OVSXmlSerializer.XmlSerializer<ColorFormat>;
	using B1NARY.DataPersistence;
	using B1NARY.IO;


	// Have it change formats per scene if it exists.
	[Serializable]
	public partial class ColorFormat
	{
		public enum SetState
		{
			/// <summary> If the set value is applied. </summary>
			Passed,
			/// <summary> If the set value is overrided and ignored. </summary>
			OverridedBy,
			/// <summary> If the set value is not found, effectively <see langword="null"/>. </summary>
			Unknown,
		}
		public record FormatFile(OSFile FileInfo, ColorFormat Format);
		public const string DEFAULT_THEME_NAME = "Default";
		public static OSDirectory RootPath => SaveSlot.StreamingAssets.GetSubdirectories("Color Themes");
		public static OSDirectory CustomThemePath => RootPath.GetSubdirectories("Custom");
		public static OSFile DefaultThemePath => RootPath.GetFile("Default.xml");
		public readonly static IndexFile indexFile = IndexFile.LoadNew();
		public static CommandArray Commands = new()
		{
			["colorformat"] = (Action<string>)(newColorFormat =>
			{
				Set(newColorFormat);
			}),
		};

		public static ColorFormat DefaultFormat
		{
			get
			{
				if (m_defaultFormat is null)
				{
					if (DefaultThemePath.Exists)
					{
						using FileStream stream = DefaultThemePath.OpenRead();
						ColorFormat format = FormatSerializer.Default.Deserialize(stream);
						if (format != null)
							return m_defaultFormat = format;
					}
					m_defaultFormat = new ColorFormat()
					{
						FormatName = DEFAULT_THEME_NAME
					};
					m_defaultFormat.Save(DefaultThemePath.Name, true);
				}
				return m_defaultFormat;
			}
			set
			{
				m_defaultFormat = value; 
				m_defaultFormat.Save(DefaultThemePath.Name, true);
			}
		} private static ColorFormat m_defaultFormat;



		public static ColorFormat CurrentFormat
		{
			get
			{
				if (m_currentFormat is null)
				{
					if (PlayerConfig.Instance.graphics.HasOverride)
					{
						string format = PlayerConfig.Instance.graphics.currentFormat;
						if (Set(format) == SetState.Passed)
							return m_currentFormat;
					}
					// Getting default..
					return SetToDefault();
				}
				return m_currentFormat;
			}
			set => Set(value);
		}
		private static ColorFormat m_currentFormat;
		public static event Action<ColorFormat> ChangedFormat;


		public static ColorFormat SetToDefault()
		{
			PlayerConfig.Instance.graphics.currentFormat.Value = "";
			m_currentFormat = DefaultFormat;
			return m_currentFormat;
		}
		public static SetState Set(ColorFormat format, bool @override = false)
		{
			if (@override)
			{
				PlayerConfig.Instance.graphics.currentFormat.Value = format.FormatName;
				m_currentFormat = format;
				ChangedFormat?.Invoke(format);
				return SetState.Passed;
			}
			if (PlayerConfig.Instance.graphics.HasOverride)
				return SetState.OverridedBy;
			m_currentFormat = format;
			ChangedFormat?.Invoke(format);
			return SetState.Passed;
		}
		public static SetState Set(string formatName, bool @override = false)
		{
			for (int i = 0; i < AvailableFormats.Count; i++)
				if (AvailableFormats[i].Format.FormatName == formatName)
				{
					return Set(AvailableFormats[i].Format, @override);
				}
			return SetState.Unknown;
		}

		internal static List<FormatFile> AllFormats =>
			new(AvailableFormats)
			{
				new FormatFile(DefaultThemePath, DefaultFormat)
			};
		/// <summary>
		/// All formats that the system that can access
		/// </summary>
		public static IReadOnlyList<FormatFile> AvailableFormats
		{
			get
			{
				if (m_availableFormats is null)
				{
					OSFile[] files = CustomThemePath.GetFiles();
					m_availableFormats = new List<FormatFile>(files.Length + 1)
					{
						new FormatFile(DefaultThemePath, DefaultFormat),
					};
					for (int i = 0; i < files.Length; i++)
					{
						OSFile fileInfo = files[i];
						if (fileInfo.Extension != ".xml")
							continue;
						try
						{
							using FileStream stream = fileInfo.OpenRead();
							ColorFormat format = FormatSerializer.Default.Deserialize(stream);
							m_availableFormats.Add(new FormatFile(fileInfo, format));
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
		internal static List<FormatFile> m_availableFormats;
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
					HashSet<string> names = new(indexFile.files);
					var allFormats = AllFormats;
					for (int i = 0; i < allFormats.Count; i++)
					{
						ColorFormat format = allFormats[i].Format;
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
	}
}
#if UNITY_EDITOR
namespace B1NARY.UI.Colors.Editor
{
	using UnityEngine;
	using UnityEditor;
	using B1NARY.UI;
	using B1NARY.Editor;
	using B1NARY.Editor.Debugger;
	using System.Linq;
	using System.Collections.Generic;
	using System.IO;
	using B1NARY.IO;

	public class ColorFormatWindow : EditorWindow
	{
		private static readonly Vector2Int defaultMinSize = new(300, 350);
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
			Rect leftRect = new(fullRect) { width = fullRect.width / 3 };
			Rect rightRect = new(fullRect) { xMin = leftRect.xMax + 2 };
			List<ColorFormat.FormatFile> allFormats = ColorFormat.AllFormats;
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
				IndexFile file = ColorFormat.indexFile;
				for (int i = 0; i < allFormats.Count; i++)
				{
					ColorFormat format = allFormats[i].Format;
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
			selection = EditorGUILayout.Popup(selection, allFormats.Select(pair => pair.Format.FormatName).ToArray());
			ColorFormat currentFormat = allFormats[selection].Format;
			OSFile fileInfo = allFormats[selection].FileInfo;
			if (!ReferenceEquals(currentFormat, ColorFormat.DefaultFormat))
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
				currentFormat.Save(fileInfo.Name, ReferenceEquals(currentFormat, ColorFormat.DefaultFormat));
				ColorFormat.m_availableFormats = null;
				ColorFormat.m_playerFormats = null;
			}
			EditorGUI.indentLevel--;
		}
	}
}
#endif