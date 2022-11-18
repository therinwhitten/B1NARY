namespace B1NARY.DataPersistence
{
	using B1NARY.DesignPatterns;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Threading.Tasks;
	using UnityEngine;

	public class PersistentData : Singleton<PersistentData>
	{
		#region Game Slot Data
		[HideInInspector] public string playerName = string.Empty;
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
	}
}