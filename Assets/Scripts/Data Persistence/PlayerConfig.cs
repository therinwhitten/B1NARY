namespace B1NARY
{
	using HideousDestructor.DataPersistence;
	using System;
	using System.IO;

	[Serializable]
	public sealed class PlayerConfig : SerializableSlot
	{
		public static FileInfo ConfigLocation { get; } = PersistentData.GetFile("PlayerConfig.cfg");//new FileInfo($"{PersistentData.FullName}/playerConfig.xml");
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

		[Serializable]
		public sealed class Audio
		{
			public float master = 1f;
		}
	}
}