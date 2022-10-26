namespace B1NARY.DataPersistence
{
	using B1NARY.DesignPatterns;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UnityEngine;

	public static class PersistentData
	{
		public const string SaveFileName = "Quicksave.sav";
		public static readonly string savePath = Application.persistentDataPath + "/Saves";
		public static string FullSavePath => savePath + SaveFileName;

		public static string name = string.Empty;
		public static Dictionary<string, string> strings = new Dictionary<string, string>();
		public static Dictionary<string, bool> bools = new Dictionary<string, bool>();
		public static Dictionary<string, int> ints = new Dictionary<string, int>();
		public static Dictionary<string, float> floats = new Dictionary<string, float>();

		//public static void SaveGame()
		//{
		//	var gameState = new GameState()
		//	{
		//		playerName = name,
		//		strings = strings,
		//		bools = bools,
		//		floats = floats,
		//		ints = ints,
		//	};
		//	gameState.SaveDataIntoMemory(FullSavePath);
		//	Debug.Log("Game Saved!");
		//}
		//
		//public static void LoadGame()
		//{
		//	throw new System.NotImplementedException();
		//	var state = GameState.LoadExistingData(FullSavePath);
		//	//await SceneManager.FadeToNextScene(state.scene);
		//	await state.LoadDataIntoMemory();
		//}
	}
}