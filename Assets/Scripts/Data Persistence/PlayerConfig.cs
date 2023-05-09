namespace B1NARY
{
	using B1NARY.DataPersistence;
	using B1NARY.UI;
	using HideousDestructor.DataPersistence;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml.Serialization;
	using UnityEngine;
	using OVSXmlSerializer;
	using OVSXmlSerializer.Extras;
	using B1NARY.UI.Colors;

	public class PlayerConfig
	{
		public const string PRE_GRAPHICS = "gra_";
		public const string PRE_SOUND = "snd_";

		public static FileInfo ConfigLocation => SerializableSlot.PersistentData.GetFile("config.xml");
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
			using (var stream = XmlSerializer.Serialize(this, "PlayerConfig"))
				using (var fileStream = ConfigLocation.Create())
				{
					stream.CopyTo(fileStream);
					fileStream.Flush();
				}
		}

		public ChangableValue<bool> quickSaveOverrides = new ChangableValue<bool>(false);
		public ChangableValue<bool> hEnable = new ChangableValue<bool>(false);
		public ChangableValue<int> dialogueSpeedTicks = new ChangableValue<int>(30);
		public Audio audio = new Audio();
		public Graphics graphics = new Graphics();



		/// <summary>
		/// Any Audio settings that first configures the typical master volume
		/// group, the voice group, but also contains any specific characters that
		/// are added to the game by the creator.
		/// </summary>
		public sealed class Audio
		{
			public ChangableValue<float> master = new ChangableValue<float>(1f);
			public ChangableValue<float> SFX = new ChangableValue<float>(1f);
			public ChangableValue<float> music = new ChangableValue<float>(1f);
			public ChangableValue<float> voices = new ChangableValue<float>(1f);
			public ChangableValue<float> UI = new ChangableValue<float>(1f);
			public Dictionary<string, float> characterVoices = new Dictionary<string, float>();
		}
		public sealed class Graphics
		{
			public ChangableValue<float> glow = new ChangableValue<float>(1f);
			public ChangableValue<int> graphicSettingIndex = new ChangableValue<int>(0);
			public ChangableValue<int> frameRate = new ChangableValue<int>(69);
			public ChangableValue<string> currentFormat = new ChangableValue<string>(null);
			public bool HasOverride => !string.IsNullOrEmpty(currentFormat.Value);
		}
	}
}