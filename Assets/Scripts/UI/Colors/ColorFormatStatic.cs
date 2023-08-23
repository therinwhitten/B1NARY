namespace B1NARY.UI.Colors
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
	using HDConsole;
	using System.Text;
	using ColorUtility = DataPersistence.ColorUtility;


	// Have it change formats per scene if it exists.
	[Serializable]
	public partial class ColorFormat
	{
		public enum SetState : byte
		{
			/// <summary> If the set value is applied. </summary>
			Passed,
			/// <summary> If the set value is overrided and ignored. </summary>
			OverridedBy,
			/// <summary> If the set value is not found, effectively <see langword="null"/>. </summary>
			Unknown,
		}

		public static CommandArray Commands = new()
		{
			["colorformat"] = (Action<string>)(newColorFormat =>
			{
				if (SetFormat(newColorFormat, false) == SetState.Unknown)
					Debug.LogWarning($"format '{newColorFormat}' is not found.");
			}),
		};
		[return: CommandToConsole]
		private static HDCommand[] GetHDCommands() => new HDCommand[]
		{
			HDCommand.AutoCompleteString("bny_format", 
				() => $"{ActiveFormat.FormatName}{(string.IsNullOrEmpty(PlayerConfig.Instance.graphics.currentFormat.Value) ? "" : " [overrided]")}", 
				(set) =>
				{
					if (SetFormat(set, true) == SetState.Unknown)
						HDConsole.WriteLine(LogType.Error, $"{set} format is not found");
				}, "format", HDCommand.MainTags.None, "Gets or Sets the overrided (or active) color format."),

			new HDCommand("bny_format_playable", (args) =>
			{
				StringBuilder builder = new();
				IReadOnlyList<ColorFormat> playableFormats = GetPlayerFormats();
				for (int i = 0; i < playableFormats.Count; i++)
					builder.AppendLine($"\t{playableFormats[i]}");
				HDConsole.WriteLine(builder.ToString());
			}) { description = "Gets all playable formats, or you can select through the options menu." },

			new HDCommand("bny_format_all", (args) =>
			{
				StringBuilder builder = new();
				IReadOnlyList<ColorFormat> formats = GetCombinedAllFormats();
				for (int i = 0; i < formats.Count; i++)
					builder.AppendLine($"\t{formats[i]}");
				HDConsole.WriteLine(builder.ToString());
			}) { description = "Gets all formats in the game. You can set the current format to these, but using playable ones are recommended." },

			new HDCommand("bny_format_create", new string[] { "format name", "primary (RGBA)", "secondary (RGBA)" }, new string[] { "params extra colors (Key/Color)" }, (args) =>
			{
				ColorFormat format = new() 
				{
					FormatName = args[0],
					PrimaryUI = ColorUtility.Parse(args[1]),
					SecondaryUI = ColorUtility.Parse(args[2]) 
				};
				for (int i = 4; i < args.Length; i++)
				{
					string[] split = args[i].Split('/');
					string key = split[0];
					Color color = ColorUtility.Parse(split[1]);
					format.ExtraUIColors.Add(key, color);
				}
				string fullPath = format.SaveFromPlayer();
				HDConsole.WriteLine($"Created format '{format.FormatName}' in '{fullPath}'");

			}) { description = "Creates a new format by the player." },

			new HDCommand("bny_format_modify", new string[] { "format name", "target", "color (RGBA)" }, (args) =>
			{
				ColorFormat targetFormat = GetAllFormats().playerFormats.First(format => format.FormatName == args[0]);
				switch (args[1].ToLower())
				{
					case "primary":
						targetFormat.PrimaryUI = ColorUtility.Parse(args[2]);
						break;
					case "secondary":
						targetFormat.SecondaryUI = ColorUtility.Parse(args[2]);
						break;
					default:
						targetFormat.ExtraUIColors[args[1]] = ColorUtility.Parse(args[2]);
						break;
				}

			}) { description = "Modifies an existing player name using 'primary', 'secondary', or other targets to modify" },
		};

		/// <summary>
		/// The root path of all color themes, including default.
		/// </summary>
		public static OSDirectory RootPath => SaveSlot.StreamingAssets.GetSubdirectories("Color Formats");
		public static OSDirectory PlayerThemePath => SaveSlot.PersistentData.GetSubdirectories("Custom Formats");

		public static OSFile DefaultThemePath => RootPath.GetFile($"{DEFAULT_THEME_NAME}.xml");
		private const string DEFAULT_THEME_NAME = "Default";

		public static event Action<ColorFormat> ChangedFormat;
		public static ColorFormat ActiveFormat
		{
			get
			{
				if (m_active == null)
				{
					if (string.IsNullOrEmpty(PlayerConfig.Instance.graphics.currentFormat.Value))
						SetToDefault();
					else if (SetFormat(PlayerConfig.Instance.graphics.currentFormat.Value, true) == SetState.Unknown)
						SetToDefault();
				}
				return m_active;

				static void SetToDefault() => SetFormat(DefaultFormat, true);
			}
		}
		private static ColorFormat m_active;

		public static SetState SetFormat(string formatName, bool @override = false)
		{
			if (Try(GetCombinedAllFormats(), out ColorFormat target))
				return SetFormat(target, @override);
			return SetState.Unknown;

			bool Try(IReadOnlyList<ColorFormat> formats, out ColorFormat output)
			{
				for (int i = 0; i < formats.Count; i++)
					if (formats[i].FormatName == formatName)
					{
						output = formats[i];
						return true;
					}
				output = null;
				return false;
			}
		}
		public static SetState SetFormat(ColorFormat format, bool @override)
		{
			if (@override)
			{
				m_active = format;
				PlayerConfig.Instance.graphics.currentFormat.Value = format.IsDefault ? string.Empty : format.FormatName;
				ChangedFormat?.Invoke(format);
				return SetState.Passed;
			}

			if (!string.IsNullOrEmpty(PlayerConfig.Instance.graphics.currentFormat.Value))
				return SetState.OverridedBy;
			m_active = format;
			ChangedFormat?.Invoke(format);
			return SetState.Passed;
		}

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
					m_defaultFormat.Save();
				}
				return m_defaultFormat;
			}
			set
			{
				m_defaultFormat = value;
				m_defaultFormat.FormatName = DEFAULT_THEME_NAME;
				m_defaultFormat.Save();
			}
		}
		private static ColorFormat m_defaultFormat;

		public static IReadOnlyList<ColorFormat> GetCombinedAllFormats()
		{
			if (m_combinedColorFormats is not null)
				return m_combinedColorFormats;
			var (developerFormats, playerFormats) = GetAllFormats();
			ColorFormat[] allFormats = new ColorFormat[developerFormats.Count + playerFormats.Count];
			int formatIndex = 0;
			void SetNewValue(ColorFormat format)
			{
				allFormats[formatIndex] = format;
				formatIndex++;
			}
			for (int i = 0; i < developerFormats.Count; i++)
				SetNewValue(developerFormats[i]);
			for (int i = 0; i < playerFormats.Count; i++)
				SetNewValue(playerFormats[i]);
			return m_combinedColorFormats = allFormats;
		}
		private static ColorFormat[] m_combinedColorFormats;
		/// <summary>
		/// Gets all formats that is found in the system; split between dev and
		/// player formats.
		/// </summary>
		public static (IReadOnlyList<ColorFormat> developerFormats, IReadOnlyList<ColorFormat> playerFormats) GetAllFormats()
		{
			if (m_availableFormats is not null)
				return m_availableFormats.Value;
			List<ColorFormat> developerFormats = GetFormats(RootPath.GetFiles());
			for (int i = 0; i < developerFormats.Count; i++)
				if (developerFormats[i].IsDefault)
				{
					developerFormats.RemoveAt(i);
					break;
				}
			List<ColorFormat> playerFormats = GetFormats(PlayerThemePath.GetFiles());
			return (m_availableFormats = (developerFormats, playerFormats)).Value;

			static List<ColorFormat> GetFormats(IList<OSFile> files)
			{
				List<ColorFormat> list = new(files.Count / 2);
				for (int i = 0; i < files.Count; i++)
					if (files[i].Extension == ".xml")
					{
						using FileStream fileStream = files[i].OpenRead();
						ColorFormat format = FormatSerializer.Deserialize(fileStream);
						list.Add(format);
					}
				return list;
			}
		}
		private static (IReadOnlyList<ColorFormat> developerFormats, IReadOnlyList<ColorFormat> playerFormats)? m_availableFormats;

		/// <summary>
		/// Gets all formats that can be overrided by the player.
		/// </summary>
		public static IReadOnlyList<ColorFormat> GetPlayerFormats()
		{
			if (m_playerFormats is not null)
				return m_playerFormats;
			var (developerFormats, playerFormats) = GetAllFormats();
			List<ColorFormat> playableFormats = new(playerFormats);
			for (int i = 0; i < developerFormats.Count; i++)
				if (developerFormats[i].playerControlled)
					playableFormats.Add(developerFormats[i]);
			return m_playerFormats = playableFormats;
		}
		private static IReadOnlyList<ColorFormat> m_playerFormats;

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

		public static void ResetCache()
		{
			m_availableFormats = null;
			m_playerFormats = null;
			m_combinedColorFormats = null;
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
			List<ColorFormat> allFormats = new(ColorFormat.GetAllFormats().developerFormats);
			newItemName = EditorGUI.TextField(rightRect, newItemName);
			if (GUILayout.Button("Reset Metadata"))
				ColorFormat.ResetCache();
			if (GUI.Button(leftRect, "New"))
			{
				new ColorFormat() { FormatName = newItemName, }.Save();
				ColorFormat.ResetCache();
			}
			if (bruh = EditorGUILayout.BeginFoldoutHeaderGroup(bruh, "Player-Defined Themes"))
			{
				EditorGUI.indentLevel++;
				for (int i = 0; i < allFormats.Count; i++)
				{
					ColorFormat format = allFormats[i];
					bool oldValue = format.playerControlled;
					if (EditorGUILayout.Toggle(format.FormatName, format.playerControlled) != format.playerControlled)
					{
						format.playerControlled = !oldValue;
						format.Save();
						ColorFormat.ResetCache();
					}
				}
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
			selection = EditorGUILayout.Popup(selection, allFormats.Select(pair => pair.FormatName).ToArray());
			ColorFormat currentFormat = allFormats[selection];
			if (!ReferenceEquals(currentFormat, ColorFormat.DefaultFormat))
				currentFormat.FormatName = EditorGUILayout.TextField("Name", currentFormat.FormatName);
			EditorGUI.indentLevel++;
			currentFormat.PrimaryUI = EditorGUILayout.ColorField("Primary Color", currentFormat.PrimaryUI);
			currentFormat.SecondaryUI = EditorGUILayout.ColorField("Secondary Color", currentFormat.SecondaryUI);
			DictionaryEditor<string, Color> editable = new(currentFormat.ExtraUIColors)
			{
				defaultValue = Color.black,
				defaultKey = ""
			};
			if (editable.Repaint())
				currentFormat.ExtraUIColors = editable.Flush();
			if (GUILayout.Button("Save"))
			{
				currentFormat.Save();
				ColorFormat.ResetCache();
			}
			EditorGUI.indentLevel--;
		}
	}
}
#endif