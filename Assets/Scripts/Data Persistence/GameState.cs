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

	[Serializable]
	public class GameState
	{
		public static string FileSavePath(int index) => $"{Application.persistentDataPath}/Saves/Save{index}";
		public static GameState LoadExistingData(int index)
		{
			using (var stream = new FileStream(FileSavePath(index), FileMode.Open))
				return new BinaryFormatter().Deserialize(stream) as GameState;
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

		public GameState()
		{
			documentPath = ScriptHandler.Instance.ScriptDocument.documentPath;
			sceneIndex = SceneManager.GetActiveScene().buildIndex;
			strings = PersistentData.strings;
			bools = PersistentData.bools;
			ints = PersistentData.ints;
			floats = PersistentData.floats;
			playerName = PersistentData.playerName;
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
			PersistentData.playerName = playerName;
			PersistentData.strings = strings;
			PersistentData.bools = bools;
			PersistentData.ints = ints;
			PersistentData.floats = floats;
			SceneManager.LoadScene(sceneIndex);
			ScriptHandler.Instance.InitializeNewScript(documentPath);
			while (ScriptHandler.Instance.CurrentLine != lastLine)
				ScriptHandler.Instance.NextLine();
		}
	}
}