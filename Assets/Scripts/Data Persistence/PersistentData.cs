namespace B1NARY.DataPersistence
{
	using B1NARY.DesignPatterns;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Threading.Tasks;
	using UnityEngine;

	public static class PersistentData //: Singleton<PersistentData>
	{
		#region Game Slot Data
		public static GameSlotData GameSlotData 
		{
			get => m_slot;
			set
			{
				m_slot = value;
				NewSlotChanged?.Invoke(m_slot);
			} 
		}
		private static GameSlotData m_slot;
		public static Action<GameSlotData> NewSlotChanged;
		public static Dictionary<string, bool> Booleans => GameSlotData.bools;
		public static Dictionary<string, string> Strings => GameSlotData.strings;
		public static Dictionary<string, int> Integers => GameSlotData.ints;
		public static Dictionary<string, float> Singles => GameSlotData.floats;
		public static bool IsLoading { get; private set; } = false;


		/// <summary>
		/// Saves data as a <see cref="GameSlotData"/> and writes it into the saves
		/// folder.
		/// </summary>
		/// <param name="index"> The index for the save. </param>
		public static void SaveGame(int index = 0)
		{
			GameSlotData.Serialize(index);
			Debug.Log("Game Saved!");
		}

		/// <summary>
		/// Load data from Binary format into as a <see cref="GameSlotData"/> and 
		/// writes it back into <see cref="PersistentData"/>.
		/// </summary>
		/// <param name="index"> The index for the save. </param>
		public static void LoadGame(int index = 0)
		{
			GameSlotData = GameSlotData.LoadExistingData(index);
			IsLoading = true;
			GameSlotData.LoadScene();
			IsLoading = false;
			Debug.Log("Game Loaded!");
		}

		/// <summary>
		/// Starts a new gameslot to save the data as a binary format.
		/// </summary>
		public static void CreateNewSlot()
		{
			GameSlotData = new GameSlotData();
		}
		#endregion
	}
}