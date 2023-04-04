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

	public static class PlayerConfig
	{
		public const string PRE_GRAPHICS = "gra_";
		public const string PRE_SOUND = "snd_";

		public static FileInfo ConfigLocation { get; } = SerializableSlot.PersistentData.GetFile("config.xml");
		private static XmlSerializer<Dictionary<string, object>> XmlSerializer { get; }
			= new XmlSerializer<Dictionary<string, object>>(new XmlSerializerConfig()
			{
				TypeHandling = IncludeTypes.SmartTypes,
				indent = true,
				indentChars = "\t"
			});

		static PlayerConfig()
		{
			if (ConfigLocation.Exists)
				using (var fileStream = ConfigLocation.OpenRead())
					m_values = XmlSerializer.Deserialize(fileStream);//XmlSerializerOld.DeserializeDictionary(fileStream);
			Debug.Log("Player Config Active!");
#if UNITY_EDITOR
			UnityEditor.EditorApplication.playModeStateChanged += EditorQuit;
			void EditorQuit(UnityEditor.PlayModeStateChange state)
			{
				if (state != UnityEditor.PlayModeStateChange.ExitingPlayMode)
					return;
				Save();
				//UnityEditor.EditorApplication.playModeStateChanged -= EditorQuit;
			}
#else
			Application.quitting += Quit;
			void Quit()
			{
				Save();
				Application.quitting -= Quit;
			}
#endif
		}


		internal static Dictionary<string, object> m_values = new Dictionary<string, object>();

		public static T GetValue<T>(string key) => (T)m_values[key];
		public static T GetValue<T>(string key, T @default)
		{
			if (m_values.TryGetValue(key, out var value))
				return (T)value;
			m_values[key] = @default;
			return @default;
		}
		public static T SetValue<T>(string key, T value)
		{
			m_values[key] = value;
			return value;
		}

		public static void Save()
		{
			using (var stream = XmlSerializer.Serialize(m_values, "PlayerConfig"))
				using (var fileStream = ConfigLocation.Create())
				{
					stream.CopyTo(fileStream);
					fileStream.Flush();
				}
			Debug.Log("Saved Player Config!");
		}



		/// <summary>
		/// Any Audio settings that first configures the typical master volume
		/// group, the voice group, but also contains any specific characters that
		/// are added to the game by the creator.
		/// </summary>
		[Serializable]
		public sealed class Audio
		{
			public float master = 1f;
			public float SFX = 1f;
			public float music = 1f;
			public float voices = 1f;
			public Dictionary<string, float> characterVoices = new Dictionary<string, float>();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// This can be considered as a facade; a way to easily manage inputs in a more
	/// Object-Oriented way.
	/// </remarks>
	public static class B1NARYConfig
	{
		public const string PRE_B1NARY = "b1n_";
		private const string B1NARY_HENABLE = PRE_B1NARY + "hEnable";
		public static ChangableValue<bool> HEnable
		{
			get => PlayerConfig.GetValue(B1NARY_HENABLE, new ChangableValue<bool>(false));
			set => PlayerConfig.SetValue(B1NARY_HENABLE, value);
		}


		private static void Init()
		{

		}


		public static class Graphics
		{
			private const string GRAPHICS_GLOW = PlayerConfig.PRE_GRAPHICS + "bloom";
			public static ChangableValue<float> Glow
			{
				get => PlayerConfig.GetValue(GRAPHICS_GLOW, new ChangableValue<float>(1f));
				set => PlayerConfig.SetValue(GRAPHICS_GLOW, value);
			}
			private const string GRAPHICS_INDEX = PlayerConfig.PRE_GRAPHICS + "graphicConfig";
			public static int GraphicSettingIndex
			{
				get => PlayerConfig.GetValue(GRAPHICS_INDEX, 0);
				set => PlayerConfig.SetValue(GRAPHICS_INDEX, value);
			}
			private const string GRAPHICS_FRAME_RATE = PlayerConfig.PRE_GRAPHICS + "frameRate";
			public static ChangableValue<int> FrameRate
			{
				get => PlayerConfig.GetValue(GRAPHICS_FRAME_RATE, new ChangableValue<int>(69));
				set => PlayerConfig.SetValue(GRAPHICS_FRAME_RATE, value);
			}
			private const string GRAPHICS_THEME = PlayerConfig.PRE_GRAPHICS + "theme";
			public static string Theme
			{
				get => PlayerConfig.GetValue(GRAPHICS_THEME, "Default");
				set => PlayerConfig.SetValue(GRAPHICS_THEME, value);
			}
		}
		public static class Sound
		{
			public const string MIXER_MASTER = PlayerConfig.PRE_SOUND + SoundOptionsBehaviour.MIXER_MASTER;
			public static float Master
			{
				get => PlayerConfig.GetValue(MIXER_MASTER, 1f);
				set => PlayerConfig.SetValue(MIXER_MASTER, value);
			}
			public const string MIXER_SFX = PlayerConfig.PRE_SOUND + SoundOptionsBehaviour.MIXER_SFX;
			public static float SFX
			{
				get => PlayerConfig.GetValue(MIXER_SFX, 1f);
				set => PlayerConfig.SetValue(MIXER_SFX, value);
			}
			public const string MIXER_MUSIC = PlayerConfig.PRE_SOUND + SoundOptionsBehaviour.MIXER_MUSIC;
			public static float Music
			{
				get => PlayerConfig.GetValue(MIXER_MUSIC, 1f);
				set => PlayerConfig.SetValue(MIXER_MUSIC, value);
			}
			public const string MIXER_NPC = PlayerConfig.PRE_SOUND + SoundOptionsBehaviour.MIXER_NPC;
			public static float NPC
			{
				get => PlayerConfig.GetValue(MIXER_NPC, 1f);
				set => PlayerConfig.SetValue(MIXER_NPC, value);
			}
			public const string MIXER_CHARACTERS = PlayerConfig.PRE_SOUND + "Characters";
			public static IReadOnlyDictionary<string, float> Characters
			{
				get
				{
					return PlayerConfig.GetValue(MIXER_CHARACTERS, new Dictionary<string, float>());
				}
				set
				{
					Dictionary<string, float> dictionary;
					if (value is Dictionary<string, float> dictionaryValue)
						dictionary = dictionaryValue;
					else
					{
						dictionary = new Dictionary<string, float>();
						using (var enumerator = value.GetEnumerator())
							while (enumerator.MoveNext())
								dictionary.Add(enumerator.Current.Key, enumerator.Current.Value);
					}
					PlayerConfig.SetValue(MIXER_CHARACTERS, dictionary);
				}
			}
		}
	}
}