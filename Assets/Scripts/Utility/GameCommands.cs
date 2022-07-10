using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public static class GameCommands
{
	public static void PrepareSwitchScenes()
	{
		if (PreppedSwitchScenes)
			return;
		PreppedSwitchScenes = true;
		SwitchingScenes?.Invoke(null, EventArgs.Empty);
		SwitchingScenes = null;
	}
	public static event EventHandler SwitchingScenes;
	public static bool PreppedSwitchScenes { get; private set; } = false;

	public static AsyncOperation SwitchScenes(string sceneName)
	{
		SwitchedScenes?.Invoke(null, sceneName);
		SwitchedScenes = null;
		var operation = SceneManager.LoadSceneAsync(sceneName);
		return operation;
	}
	public static event EventHandler<string> SwitchedScenes;

	public const string exceptionLoadName = "tfhdthrdygujvukhgy";

	public static void QuitGame()
	{
		Application.Quit();
	}
}