namespace B1NARY.DataPersistence
{
	using B1NARY.DesignPatterns;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UnityEngine;

	public static class PersistentData
	{
		public static string playerName = string.Empty;
		public static Dictionary<string, string> strings = new Dictionary<string, string>();
		public static Dictionary<string, bool> bools = new Dictionary<string, bool>();
		public static Dictionary<string, int> ints = new Dictionary<string, int>();
		public static Dictionary<string, float> floats = new Dictionary<string, float>();

		/// <summary>
		/// Saves data as a <see cref="GameState"/> and writes it into the saves
		/// folder.
		/// </summary>
		/// <param name="index"> The index for the save. </param>
		public static void SaveGame(int index = 0)
		{
			new GameState().Save(index);
			Debug.Log("Game Saved!");
		}

		/// <summary>
		/// Load data from Binary format into as a <see cref="GameState"/> and 
		/// writes it back into <see cref="PersistentData"/>.
		/// </summary>
		/// <param name="index"> The index for the save. </param>
		public static void LoadGame(int index = 0)
		{
			GameState.LoadExistingData(index).Load();
			Debug.Log("Game Loaded!");
		}
	}
}