﻿using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameCommands
{
	public static void PrepareSwitchScenes()
	{
		if (preppedSwitchScenes)
			return;
		preppedSwitchScenes = true;
		SwitchingScenes?.Invoke(null, EventArgs.Empty);
	}
	public static event EventHandler SwitchingScenes;
	private static bool preppedSwitchScenes = false;

	public static AsyncOperation SwitchScenes(string sceneName)
	{
		var operation = SceneManager.LoadSceneAsync(sceneName);
		SwitchedScenes?.Invoke(null, EventArgs.Empty);
		return operation;
	}
	public static event EventHandler SwitchedScenes;
}