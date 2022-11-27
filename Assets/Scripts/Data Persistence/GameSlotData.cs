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
		public DateTime lastSaved = default;
		#endregion


		public Dictionary<string, string> strings;
		public Dictionary<string, bool> bools;
		public Dictionary<string, int> ints;
		public Dictionary<string, float> floats;

		public string PlayerName
		{
			get => strings["Player Name"];
			set => strings["Player Name"] = value;
		}
		public int sceneIndex;
		public string documentPath;
		private ScriptLine lastLine;

		public GameSlotData()
		{
			strings = new Dictionary<string, string>();
			PlayerName = string.Empty;
			bools = new Dictionary<string, bool>();
			ints = new Dictionary<string, int>();
			floats = new Dictionary<string, float>();
		}
		public void CaptureDocument()
		{
			documentPath = ScriptHandler.Instance.ScriptDocument.documentPath;
			sceneIndex = SceneManager.GetActiveScene().buildIndex;
			lastLine = ScriptHandler.Instance.CurrentLine;
		}
		public void Serialize(int index = 0)
		{
			string path = FileSavePath(index);
			string savesPath = path.Remove(path.LastIndexOf('/'));
			if (!Directory.Exists(savesPath))
				Directory.CreateDirectory(savesPath);
			using (var stream = new FileStream(path, FileMode.Create))
				new BinaryFormatter().Serialize(stream, this);
		}
		public void LoadScene()
		{
			SceneManager.LoadScene(sceneIndex);
			ScriptHandler.Instance.InitializeNewScript(documentPath);
			while (ScriptHandler.Instance.CurrentLine != lastLine)
				ScriptHandler.Instance.NextLine();
		}
	}
}