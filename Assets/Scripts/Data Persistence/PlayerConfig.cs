namespace B1NARY
{
	using B1NARY.DataPersistence;
	using HideousDestructor.DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Runtime.Serialization;
	using UnityEngine;

	[Serializable]
	public sealed class PlayerConfig : SerializableSlot, IDisposable
#if UNITY_EDITOR
		, IDeserializationCallback
#endif
	{
		public static FileInfo ConfigLocation { get; } = PersistentData.GetFile("config.cfg");
		/// <summary>
		/// An instance of the player config, retrieves a blank if there is none,
		/// or gets the file in <see cref="ConfigLocation"/>.
		/// </summary>
		public static PlayerConfig Instance
		{
			get
			{
				if (m_config == null)
					if (ConfigLocation.Exists)
					{
						m_config = Deserialize<PlayerConfig>(ConfigLocation);
					}
					else
					{
						m_config = new PlayerConfig();
						m_config.Serialize();
					}
				return m_config;
			}
		}
		private static PlayerConfig m_config;

		public override bool UsesThumbnail => false;

		private PlayerConfig() : base(ConfigLocation)
		{
			OnDeserialization(this);
		}
		// Garbage collection for play mode editor stuff.
#if UNITY_EDITOR
		private void EditorApplicationCommit(UnityEditor.PlayModeStateChange state)
		{
			if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
			{
				Dispose();
			}
		}
		~PlayerConfig()
		{
			UnityEditor.EditorApplication.playModeStateChanged -= EditorApplicationCommit;
		}
#endif
		public new void OnDeserialization(object sender)
		{
			base.OnDeserialization(sender);

			// It effectively has the same ability to save either in the editor, 
			// - or quitting the game.
#if UNITY_EDITOR
			UnityEditor.EditorApplication.playModeStateChanged += EditorApplicationCommit;
#else
			Application.wantsToQuit += () => { Dispose(); return true; };
#endif
		}
		public void Dispose()
		{
			Instance.Serialize();
		}

		public Audio audio = new Audio();
		public Graphics graphics = new Graphics();
		public bool HentaiEnabled
		{
			get => m_hentaiEnabled;
			set
			{
				if (m_hentaiEnabled == value)
					return;
				m_hentaiEnabled = value;
			}
		}
		private bool m_hentaiEnabled = false;

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
		/// <summary>
		/// Any graphics settings that is found for the player. Contains serialized
		/// resolutions and visual settings.
		/// </summary>
		[Serializable]
		public sealed class Graphics
		{
			public B1NARYResolution resolution = (B1NARYResolution)Screen.currentResolution;
			public float glow = 1f;//new StoredRange(1f, new Range(0f, 10f));
		}
	}
}