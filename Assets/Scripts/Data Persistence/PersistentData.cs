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

	public class PersistentData : Singleton<PersistentData>
	{
		#region Game Slot Data
		public GameSlotData GameSlotData 
		{
			get => m_slot;
			set
			{
				m_slot = value;
				NewSlotChanged?.Invoke(m_slot);
			} 
		}
		private GameSlotData m_slot;
		public Action<GameSlotData> NewSlotChanged;
		public Dictionary<string, bool> Booleans => GameSlotData.bools;
		public Dictionary<string, string> Strings => GameSlotData.strings;
		public Dictionary<string, int> Integers => GameSlotData.ints;
		public Dictionary<string, float> Singles => GameSlotData.floats;


		/// <summary>
		/// Saves data as a <see cref="GameSlotData"/> and writes it into the saves
		/// folder.
		/// </summary>
		/// <param name="index"> The index for the save. </param>
		public void SaveGame(int index = 0)
		{
			GameSlotData.Serialize(index);
			Debug.Log("Game Saved!");
		}

		/// <summary>
		/// Load data from Binary format into as a <see cref="GameSlotData"/> and 
		/// writes it back into <see cref="PersistentData"/>.
		/// </summary>
		/// <param name="index"> The index for the save. </param>
		public void LoadGame(int index = 0)
		{
			GameSlotData = GameSlotData.LoadExistingData(index);
			GameSlotData.LoadScene();
			Debug.Log("Game Loaded!");
		}

		/// <summary>
		/// Starts a new gameslot to save the data as a binary format.
		/// </summary>
		public void CreateNewSlot()
		{
			GameSlotData = new GameSlotData();
		}
		#endregion
	}
}