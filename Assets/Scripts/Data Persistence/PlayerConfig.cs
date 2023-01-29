﻿namespace B1NARY
{
	using B1NARY.DataPersistence;
	using HideousDestructor.DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.IO;

	[Serializable]
	public sealed class PlayerConfig : SerializableSlot
	{
		public static FileInfo ConfigLocation { get; } = PersistentData.GetFile("config.cfg");//new FileInfo($"{PersistentData.FullName}/playerConfig.xml");
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

		private PlayerConfig() : base(ConfigLocation)
		{

		}

		public Audio audio = new Audio();
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

		[Serializable]
		public sealed class Audio
		{
			public float master = 1f;
			public float voices = 1f;
			public Dictionary<string, float> characterVoices = new Dictionary<string, float>();
		}
	}
}