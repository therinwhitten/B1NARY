namespace B1NARY.DataPersistence
{
	using B1NARY.DesignPatterns;
	using System.Collections.Generic;
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Threading.Tasks;
	using UnityEngine;

	public class PersistentData : Singleton<PersistentData>
	{
		#region Game Slot Data
		public string playerName = string.Empty;
		public Dictionary<string, string> strings = new Dictionary<string, string>();
		public Dictionary<string, bool> bools = new Dictionary<string, bool>();
		public Dictionary<string, int> ints = new Dictionary<string, int>();
		public Dictionary<string, float> floats = new Dictionary<string, float>();

		/// <summary>
		/// Saves data as a <see cref="GameSlotData"/> and writes it into the saves
		/// folder.
		/// </summary>
		/// <param name="index"> The index for the save. </param>
		public static void SaveGame(int index = 0)
		{
			new GameSlotData().Save(index);
			Debug.Log("Game Saved!");
		}

		/// <summary>
		/// Load data from Binary format into as a <see cref="GameSlotData"/> and 
		/// writes it back into <see cref="PersistentData"/>.
		/// </summary>
		/// <param name="index"> The index for the save. </param>
		public static void LoadGame(int index = 0)
		{
			GameSlotData.LoadExistingData(index).Load();
			Debug.Log("Game Loaded!");
		}
		#endregion

		#region Player Data
		private static string FileLocation => $"{Application.persistentDataPath}/Static.amogus2";
		private static bool PlayerFileExists => File.Exists(FileLocation);
		private static FileStream GetPlayerDataFile(FileMode mode) => new FileStream(FileLocation, mode);

		protected virtual void Awake()
		{
			if (!PlayerFileExists)
			{
				playerStrings = new Dictionary<string, string>();
				playerBools = new Dictionary<string, bool>();
				playerFloats = new Dictionary<string, float>();
				playerInts = new Dictionary<string, int>();
				return;
			}

			using (FileStream fileStream = GetPlayerDataFile(FileMode.OpenOrCreate))
			{
				object[] objects = (object[])new BinaryFormatter().Deserialize(fileStream);
				playerStrings = (Dictionary<string, string>)objects[0];
				playerBools = (Dictionary<string, bool>)objects[1];
				playerFloats = (Dictionary<string, float>)objects[2];
				playerInts = (Dictionary<string, int>)objects[3];
			}
		}

		public Dictionary<string, string> playerStrings;
		public Dictionary<string, bool> playerBools;
		public Dictionary<string, int> playerInts;
		public Dictionary<string, float> playerFloats;

		protected virtual void OnDestroy() => new BinaryFormatter().Serialize(GetPlayerDataFile(FileMode.Create),
			new object[]
			{
				playerStrings,
				playerBools,
				playerFloats,
				playerInts
			});
		#endregion
	}
}