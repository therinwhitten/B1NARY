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
		SwitchingScenes?.Invoke();
		SwitchingScenes = null;
	}
	public static event Action SwitchingScenes;
	public static bool PreppedSwitchScenes { get; private set; } = false;

	public static AsyncOperation SwitchScenes(string sceneName)
	{
		SwitchedScenes?.Invoke(sceneName);
		SwitchedScenes = null;
		AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
		return operation;
	}
	public static event Action<string> SwitchedScenes;

	public const string exceptionLoadName = "OptionalWarningOnException";

	public static void QuitGame()
	{
		Application.Quit();
	}
}