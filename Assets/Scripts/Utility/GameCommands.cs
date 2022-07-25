using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public static partial class GameCommands
{
	public const string exceptionLoadName = "OptionalWarningOnException";

	public static void QuitGame()
	{
		Debug.Log("Pressed the quit button!");
		Application.Quit();
	}
}