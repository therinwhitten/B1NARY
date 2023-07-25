namespace B1NARY.DataPersistence
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml.Serialization;
	using UnityEngine;
	using OVSXmlSerializer;
	using OVSXmlSerializer.Extras;
	using B1NARY.UI.Globalization;

	public class PlayerConfig
	{
		public const string PRE_GRAPHICS = "gra_";
		public const string PRE_SOUND = "snd_";

		public static FileInfo ConfigLocation => SaveSlot.PersistentData.GetFile("config.xml");
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
						using (var fileStream = ConfigLocation.OpenRead())
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
			using var stream = XmlSerializer.Serialize(this, "PlayerConfig");
			using var fileStream = ConfigLocation.Create();
			stream.CopyTo(fileStream);
			fileStream.Flush();
		}

		public ChangableValue<bool> quickSaveOverrides = new(false);
		public ChangableValue<bool> hEnable = new(false);
		public ChangableValue<int> dialogueSpeedTicks = new(30);
		public ChangableValue<string> language = new(Languages.Instance.Count > 0 ? Languages.Instance[0] : "English");
		public Audio audio = new();
		public Graphics graphics = new();



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
			public ChangableValue<int> graphicSettingIndex = new(0);
			public ChangableValue<int> refreshRate = new(69);
			public ChangableValue<string> currentFormat = new(null);
			public bool HasOverride => !string.IsNullOrEmpty(currentFormat.Value);
		}
	}
}