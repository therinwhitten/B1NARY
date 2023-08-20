#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif
namespace B1NARY.DataPersistence
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml.Serialization;
	using UnityEngine;
	using OVSXmlSerializer;
	using OVSXmlSerializer.Extras;
	using B1NARY.UI.Globalization;
	using Steamworks;
#if !DISABLESTEAMWORKS
	using global::Steamworks;
#endif
	using Version = System.Version;
	using B1NARY.IO;
	using HDConsole;
	using UnityEngine.Rendering;

	public class PlayerConfig
	{
		public const string PRE_GRAPHICS = "gra_";
		public const string PRE_SOUND = "snd_";

		public static OSFile ConfigLocation => SaveSlot.PersistentData.GetFile("config.xml");
		private static XmlSerializer<PlayerConfig> XmlSerializer { get; }
			= new XmlSerializer<PlayerConfig>(new XmlSerializerConfig
			{
				TypeHandling = IncludeTypes.SmartTypes,
				Indent = true,
				IndentChars = "\t",
				Version = new Version(1, 0),
				VersionLeniency = Versioning.Leniency.Minor,
				IgnoreUndefinedValues = true,
			});


		public static PlayerConfig Instance
		{
			get
			{
				if (m_Instance is null)
				{
					if (ConfigLocation.Exists)
						using (FileStream fileStream = ConfigLocation.Open(FileMode.Open, FileAccess.Read))
							m_Instance = XmlSerializer.Deserialize(fileStream);
					else
						m_Instance = new PlayerConfig();
#if UNITY_EDITOR
					UnityEditor.EditorApplication.playModeStateChanged += EditorQuit;
					void EditorQuit(UnityEditor.PlayModeStateChange state)
					{
						if (state != UnityEditor.PlayModeStateChange.ExitingPlayMode)
							return;
						m_Instance.Save();
					}
#else
			Application.quitting += Quit;
			void Quit()
			{
				m_Instance.Save();
				Application.quitting -= Quit;
			}
#endif
				}
				return m_Instance;
			}
		} private static PlayerConfig m_Instance;

		

		public void Save()
		{
			using MemoryStream stream = XmlSerializer.Serialize(this, "PlayerConfig");
			using FileStream fileStream = ConfigLocation.Create();
			stream.CopyTo(fileStream);
			fileStream.Flush();
		}

		public ChangableValue<bool> quickSaveOverrides = new(false);
		public ChangableValue<bool> hEnable = new(false);
		public ChangableValue<int> dialogueSpeedTicks = new(30);
		public ChangableValue<string> language = new(Languages.Instance.Count > 0 ? Languages.Instance[0] : "English");
		[XmlNamedAs("voicelineBGone")]
		public ChangableValue<bool> voicelineBGone = new(false);
		public Audio audio = new();
		public Graphics graphics = new();
		public HashSet<string> savedAchievements = new();


		/// <summary>
		/// Any Audio settings that first configures the typical master volume
		/// group, the voice group, but also contains any specific characters that
		/// are added to the game by the creator.
		/// </summary>
		public sealed class Audio
		{
			public ChangableValue<float> master = new(1f);
			public ChangableValue<float> SFX = new(1f);
			public ChangableValue<float> music = new(1f);
			public ChangableValue<float> voices = new(1f);
			public ChangableValue<float> UI = new(1f);
			public Dictionary<string, float> characterVoices = new();
		}
		public sealed class Graphics
		{
			public ChangableValue<float> glow = new(1f);
			public ChangableValue<int> graphicSettingIndex = new(QualitySettings.GetQualityLevel());
			public ChangableValue<int> frameRate = new(Math.Min(69, (int)Screen.resolutions.Max(res => res.refreshRateRatio.value)));
			// Let the legendary coders find this line to laugh their asses off,
			// - I want the small file sizes but my manager still wants high quality
			// - images lol
			public ChangableValue<int> thumbnailQuality = new(GetThumbnail());

			private static int GetThumbnail()
			{
				const int odd = 10, max = 100;
#if !DISABLESTEAMWORKS
				const ulong oddsSteamID = 76561198109619934;
				try { return SteamUser.GetSteamID().m_SteamID == oddsSteamID ? odd : max; }
				catch { return max; }
#else
				return max;
#endif
			}
			public ChangableValue<string> currentFormat = new(null);
			public bool HasOverride => !string.IsNullOrEmpty(currentFormat.Value);
		}

		[return: CommandToConsole]
		private static HDCommand[] GetHDCommands() => new HDCommand[]
		{
			HDCommand.AutoCompleteFloat("cl_glow", () => Instance.graphics.glow, (set) => Instance.graphics.glow.Value = set, 0, 10, HDCommand.MainTags.None, "Adjusts glow intensity"),
			HDCommand.AutoCompleteInt("cl_graphic_index", () => Instance.graphics.graphicSettingIndex, (set) => Instance.graphics.graphicSettingIndex.Value = set, 0, QualitySettings.count, HDCommand.MainTags.None),
		};
	}
}