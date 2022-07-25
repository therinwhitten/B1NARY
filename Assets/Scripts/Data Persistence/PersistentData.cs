using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class PersistentData
{
	public const string SaveFileName = "Quicksave.sav";
	public static readonly string savePath = Application.persistentDataPath + "/Saves";
	public static string FullSavePath => savePath + SaveFileName;

	public static Dictionary<string, string> strings = new Dictionary<string, string>();
	public static Dictionary<string, bool> bools = new Dictionary<string, bool>();
	public static Dictionary<string, int> ints = new Dictionary<string, int>();
	public static Dictionary<string, float> floats = new Dictionary<string, float>();

	public static void SaveGame()
	{
		var gameState = new GameState()
		{
			strings = strings,
			bools = bools,
			floats = floats,
			ints = ints,
		};
		gameState.SaveDataIntoMemory(FullSavePath);
		Debug.Log("Game Saved!");
	}

	public static void LoadGame()
	{
		var state = GameState.LoadExistingData(FullSavePath);
		TransitionHandler.Instance.TransitionToNextScene(state.scene, 0.5f).Wait();
		state.LoadDataIntoMemory().Wait();
	}
	// Update is called once per frame
	/*
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F5))
		{
			SaveGame();
		}
		if (Input.GetKeyDown(KeyCode.F8))
		{
			_ = LoadGame();
		}
	}
	*/
}