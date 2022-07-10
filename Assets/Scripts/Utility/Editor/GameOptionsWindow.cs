using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static GameCommands;

public class GameOptionsWindow : EditorWindow
{
	

	private static GameOptionsWindow instance;
	[MenuItem("B1NARY/Game Options", priority = 2)]
	public static void ShowWindow()
	{
		instance = GetWindow<GameOptionsWindow>();
		instance.titleContent = new GUIContent("B1NARY Game Options");
		instance.Show();
	}

	private void OnGUI()
	{
		if (GUILayout.Button("Delete all keys"))
			GamePreferences.DeleteAllKeys();
		FrameLimitBlock();
		ExceptionChoiceBlock();
	}
	private void FrameLimitBlock()
	{
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Frame Limiter", EditorStyles.boldLabel);
		int frameLimitTypeOut = GUILayout.SelectionGrid(FrameLimiter.VSyncCount, FrameLimiter.frameLimitSettings, FrameLimiter.frameLimitSettings.Length);
		if (frameLimitTypeOut != FrameLimiter.VSyncCount)
		{
			FrameLimiter.VSyncCount = frameLimitTypeOut;
			FrameLimiter.ApplySettings();
		}
		if (frameLimitTypeOut == 0)
		{
			int frameLimitTargetOut = EditorGUILayout.IntSlider("FPS Limit", FrameLimiter.Target, 1, 300);
			// Setting a value every time the GUI is loaded is expensive.
			if (frameLimitTargetOut != FrameLimiter.Target)
				FrameLimiter.Target = frameLimitTargetOut;
		}
	}

	
	private void ExceptionChoiceBlock()
	{
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("This turns some options that can be treated as warnings into errors: ");
		EditorGUILayout.LabelField("Exception Handling Settings", EditorStyles.boldLabel);
		bool exceptionType = GUILayout.SelectionGrid(GamePreferences.GetBool(exceptionLoadName, true) ? 0 : 1, new string[] { "Exception", "Warning" }, 2) == 0;
		if (GamePreferences.GetBool(exceptionLoadName, true) != exceptionType)
			GamePreferences.SetBool(exceptionLoadName, exceptionType);
	}
}