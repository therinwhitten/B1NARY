namespace B1NARY.DataPersistence
{
	using B1NARY.UI;
	using B1NARY.Audio;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;
	using B1NARY.Scripting;
	using B1NARY.DesignPatterns;

	/// <summary>
	/// This stores data about the player specificially.
	/// </summary>
	[Serializable]
	public class GameSlotData
	{
		public static string FileSavePath(int index) => $"{Application.persistentDataPath}/Saves/Save{index}.amogus";
		public static GameSlotData LoadExistingData(int index)
		{
			using (var stream = new FileStream(FileSavePath(index), FileMode.Open))
				return new BinaryFormatter().Deserialize(stream) as GameSlotData;
		}

		#region About
		public TimeSpan timePlayed = TimeSpan.Zero;
		public DateTime lastSaved;
		#endregion


		public Dictionary<string, string> strings;
		public Dictionary<string, bool> bools;
		public Dictionary<string, int> ints;
		public Dictionary<string, float> floats;

		public string playerName;
		public int sceneIndex;
		public string documentPath;
		private ScriptLine lastLine;

		public GameSlotData()
		{
			documentPath = ScriptHandler.Instance.ScriptDocument.documentPath;
			sceneIndex = SceneManager.GetActiveScene().buildIndex;
			strings = PersistentData.Instance.strings;
			bools = PersistentData.Instance.bools;
			ints = PersistentData.Instance.ints;
			floats = PersistentData.Instance.floats;
			playerName = PersistentData.Instance.playerName;
			lastSaved = DateTime.Now;
			timePlayed += lastSaved - ScriptHandler.Instance.playedTime;
		}
		public void Save(int index)
		{
			string path = FileSavePath(index);
			string savesPath = path.Remove(path.LastIndexOf('/'));
			if (!Directory.Exists(savesPath))
				Directory.CreateDirectory(savesPath);
			using (var stream = new FileStream(path, FileMode.Create))
				new BinaryFormatter().Serialize(stream, this);
		}
		public void Load()
		{
			PersistentData.Instance.playerName = playerName;
			PersistentData.Instance.strings = strings;
			PersistentData.Instance.bools = bools;
			PersistentData.Instance.ints = ints;
			PersistentData.Instance.floats = floats;
			SceneManager.LoadScene(sceneIndex);
			ScriptHandler.Instance.InitializeNewScript(documentPath);
			while (ScriptHandler.Instance.CurrentLine != lastLine)
				ScriptHandler.Instance.NextLine();
		}
	}
}