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
	using OVSSerializer;
	using OVSSerializer.Extras;
	using B1NARY.UI.Globalization;
	using Steamworks;
#if !DISABLESTEAMWORKS
	using global::Steamworks;
#endif
	using Version = System.Version;
	using OVSSerializer.IO;
	using HDConsole;
	using UnityEngine.Rendering;
	using System.Xml;
	using System.Collections;

	public class PlayerConfig : IDisposable
	{
		public const string PRE_GRAPHICS = "gra_";
		public const string PRE_SOUND = "snd_";

		public static OSFile ConfigLocation => SaveSlot.PersistentData.GetFile("config.xml");
		private static OVSXmlSerializer<PlayerConfig> XmlSerializer { get; } = new()
		{
			TypeHandling = IncludeTypes.SmartTypes,
			Indent = true,
			IndentChars = "\t",
			Version = new Version(2, 0),
			VersionLeniency = Versioning.Leniency.All,
			IgnoreUndefinedValues = true,
		};


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
						m_Instance.Dispose();
					}
#else
					Application.quitting += Quit;
					void Quit()
					{
						m_Instance.Save();
						Application.quitting -= Quit;
						m_Instance.Dispose();
					}
#endif
				}
				return m_Instance;
			}
		}
		private static PlayerConfig m_Instance;

		public PlayerConfig()
		{
			// Artificial delay to prevent trying to load the same file multiple times.
			SaveSlot.EmptiedSaveCache += UpdateSaveCollectibles;
		}
		private void UpdateSaveCollectibles()
		{
			var list = SaveSlot.AllSaves;
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					collectibles.MergeSaves(list[i].Value.Value);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}


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
		[OVSXmlNamedAs("voicelineBGone")]
		public ChangableValue<bool> voicelineBGone = new(false);
		public Audio audio = new();
		public Graphics graphics = new();
		public HashSet<string> savedAchievements = new();
		public CollectibleMerger collectibles = new();
		public Dictionary<string, float> savedProgressionAchievements = new();

		[Command("cl_clear_config", "Deletes and creates a new config. Quits and relaunches the game to apply settings.")]
		public static void Clear()
		{
			HashSet<string> achievements = Instance.savedAchievements;
			Dictionary<string, float> progAchievements = Instance.savedProgressionAchievements;
			m_Instance = new PlayerConfig() { savedAchievements = achievements, savedProgressionAchievements = progAchievements };
#if UNITY_EDITOR
			UnityEditor.EditorApplication.ExitPlaymode();
#else

			Application.Quit();
#endif
		}


		public void Dispose()
		{
			SaveSlot.EmptiedSaveCache -= UpdateSaveCollectibles;
		}

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
				const int odd = 1, max = 80;
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

		[return: CommandsFromGetter]
		private static HDCommand[] GetHDCommands() => new HDCommand[]
		{
			HDCommand.AutoCompleteFloat("cl_glow", () => Instance.graphics.glow, (set) => Instance.graphics.glow.Value = set, 0, 10, HDCommand.MainTags.None, "Adjusts glow intensity"),
			HDCommand.AutoCompleteInt("cl_graphic_index", () => Instance.graphics.graphicSettingIndex, (set) => Instance.graphics.graphicSettingIndex.Value = set, 0, QualitySettings.count, HDCommand.MainTags.None),
			new HDCommand("bny_add_collectible_gallery", name =>
			{
				SaveSlot.ActiveSlot.collectibles.Gallery.Add(name[0]);
				Instance.collectibles.Gallery.Add(name[0]);
			}),
		};

		[Serializable]
		public record CollectibleMerger(HashSet<string> Gallery, HashSet<string> Map, HashSet<string> CharacterProfiles) : IOVSXmlSerializable
		{
			public const string UNLOCKED_GALLERY_KEY = "UnlockedGallery";
			public const string UNLOCKED_MAP_KEY = "UnlockedMap";
			public const string UNLOCKED_CHAR_KEY = "UnlockedChar";

			public CollectibleMerger() : this(new(), new(), new())
			{
				
			}

			public void MergeSavesFromSingleton()
			{
				MergeSaves(SaveSlot.AllSaves.Select(pair => pair.Value.Value).ToArray());
			}
			public void MergeSaves(params SaveSlot[] slots)
			{
				for (int i = 0; i < slots.Length; i++)
				{
					DataPersistence.CollectibleCollection collection = slots[i].collectibles;
					MergeList(collection.Gallery, Gallery);
					MergeList(collection.Map, Map);
					MergeList(collection.CharacterProfiles, CharacterProfiles);
					static void MergeList(List<string> with, HashSet<string> to)
					{
						for (int i = 0; i < with.Count; i++)
							to.Add(with[i]);
					}
				}
			}

			bool IOVSXmlSerializable.ShouldWrite => true;
			void IOVSXmlSerializable.Read(XmlNode value)
			{
				AddRange(Gallery, Read(UNLOCKED_GALLERY_KEY));
				AddRange(Map, Read(UNLOCKED_MAP_KEY));
				AddRange(CharacterProfiles, Read(UNLOCKED_CHAR_KEY));


				string[] Read(string name) => value.ChildNodes.FindNamedNode(name).InnerText.Split(',');
				void AddRange(HashSet<string> values, in string[] strings)
				{
					for (int i = 0; i < strings.Length; i++)
						values.Add(strings[i]);
				}
			}
			void IOVSXmlSerializable.Write(XmlNode currentNode)
			{
				XmlDocument document = currentNode.OwnerDocument;
				Merge(Gallery, UNLOCKED_GALLERY_KEY);
				Merge(Map, UNLOCKED_MAP_KEY);
				Merge(CharacterProfiles, UNLOCKED_CHAR_KEY);
				void Merge(IEnumerable<string> strings, string name)
				{
					XmlElement element = document.CreateElement(name);
					element.InnerText = string.Join(',', strings);
					currentNode.AppendChild(element);
				}
			}
		}

	}
}