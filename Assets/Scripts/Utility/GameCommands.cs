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
		// Some events wants to constantly listen to gamecommands, so they usually
		// - re-assign the same method. This allows them to have the ability to
		// - easily do so without having a 'buffer' or wait until it is finished
		// - emptying out the event.
		if (SwitchingScenes == null)
			return;
		Action[] events = SwitchingScenes.GetInvocationList()
			.Cast<Action>().ToArray();
		SwitchingScenes = null;
		Array.ForEach(events, del => del.Invoke());
	}
	public static event Action SwitchingScenes;
	public static bool PreppedSwitchScenes { get; private set; } = false;

	public static AsyncOperation SwitchScenes(string sceneName)
	{
		if (!PreppedSwitchScenes)
			return null;
		PreppedSwitchScenes = false;
		AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
		if (SwitchedScenes != null)
		{
			Action<string>[] events = SwitchedScenes.GetInvocationList()
				.Cast<Action<string>>().ToArray();
			SwitchedScenes = null;
			Array.ForEach(events, del => del.Invoke(sceneName));
		}
		return operation;
	}
	public static event Action<string> SwitchedScenes;

	public const string exceptionLoadName = "OptionalWarningOnException";

	public static void QuitGame()
	{
		Application.Quit();
	}
}