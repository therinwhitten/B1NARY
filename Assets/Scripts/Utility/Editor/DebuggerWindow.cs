using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;


public class DebuggerWindow : EditorWindow
{


	[MenuItem("B1NARY/Debugger")]
	public static void ShowWindow()
	{
		// Get existing open window or if none, make a new one:
		DebuggerWindow window = (DebuggerWindow)GetWindow(typeof(DebuggerWindow));
		window.titleContent = new GUIContent("B1NARY Debugger");
		window.Show();
	}

	private void OnGUI()
	{
		EditorGUILayout.LabelField("GUI Built by AnOddDoorKnight, in beta", EditorStyles.miniLabel);
		TopBar();
		EditorGUILayout.Space(10);
		ShowTabs();
	}

	private void OnInspectorUpdate()
	{
		Repaint();
	}

	private void TopBar()
	{
		CurrentLineShow();
		SpeakerShow();
	}

	enum Tabs : int
	{
		Scripts,
		Audio,
		Options
	}
	int selected = 0;
	private void ShowTabs()
	{
		Rect guiRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - 10, 20);
		guiRect.width -= 10;
		guiRect.x += 5;
		string[] array = Enum.GetNames(typeof(Tabs));
		selected = GUI.SelectionGrid(guiRect, selected, array, array.Length);
		switch ((Tabs)selected)
		{
			case Tabs.Scripts:
				ScriptsTab(parser != null && parser.scriptName != null ? parser.scriptName : "", 
					parser != null && parser.currentNode != null ? parser.currentNode.index : -1); 
				break;
			case Tabs.Audio: AudioTab(); break;
			case Tabs.Options: OptionsTab(); break;
		}
	}

	string currentScript = "";
	string[] currentScriptContents = Array.Empty<string>();
	Vector2 scrollPos = Vector2.zero;
	private void ScriptsTab(string scriptName, int onLine)
	{
		SceneNameShow();
		ScriptNameShow();
		Rect lastRect = GUILayoutUtility.GetLastRect();
		lastRect.y += 4 + lastRect.height;
		lastRect.height = Screen.height - lastRect.y - 4;

		scrollPos = GUILayout.BeginScrollView(scrollPos);
		if (currentScript != parser.scriptName)
		{
			currentScript = parser.scriptName;
			currentScriptContents = File.ReadAllLines($"{Application.streamingAssetsPath}/Docs/{parser.scriptName}.txt");
		}
		// Spaces
		int space = currentScriptContents.Length.ToString().Length;


		for (int i = 0; i < currentScriptContents.Length; i++)
		{
			Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - 26, 16);
			rect.x += 6;
			if (onLine == i)
				GUI.color = Color.red;
			else if (currentScriptContents[i].Contains("{"))
				GUI.color = Color.yellow;
			else if (currentScriptContents[i].Contains("["))
				GUI.color = Color.cyan;
			else if (currentScriptContents[i].EndsWith("::"))
				GUI.color = Color.magenta;
			else
				GUI.color = Color.white;
			GUI.Label(rect, $"{i + 1} {new string(' ', (space - (i + 1).ToString().Length) * 2)}    {currentScriptContents[i]}");
		}
		GUILayout.EndScrollView();
	}
	private void AudioTab()
	{

	}
	private void OptionsTab()
	{

	}

	private void SceneNameShow()
	{
		const string startingLine = "On Scene: ";
		EditorGUILayout.LabelField(startingLine + SceneManager.GetActiveScene().name);
	}

	private ScriptParser parser;
	private void ScriptNameShow()
	{
		const string startingLine = "On Script: ";
		if (parser == null)
			parser = FindObjectOfType<ScriptParser>();
		if (parser == null || parser.scriptName == null)
			EditorGUILayout.LabelField(startingLine + "NaN", EditorStyles.boldLabel);
		else
			EditorGUILayout.LabelField(startingLine + parser.scriptName, EditorStyles.boldLabel);
	}
	private void CurrentLineShow()
	{
		const string startingLine = "On line: ";
		if (parser == null)
			parser = FindObjectOfType<ScriptParser>();
		if (parser == null || parser.currentNode == null)
			EditorGUILayout.LabelField(startingLine + "NaN", EditorStyles.boldLabel);
		else
			EditorGUILayout.LabelField(startingLine + (parser.currentNode.index + 1), EditorStyles.boldLabel);
	}

	private DialogueSystem charScript;
	private void SpeakerShow()
	{
		const string startingLine = "On character: ";
		if (charScript == null)
			charScript = FindObjectOfType<DialogueSystem>();
		if (charScript == null || charScript.currentSpeaker.Length == 0)
			EditorGUILayout.LabelField(startingLine + "NaN");
		else
			EditorGUILayout.LabelField(startingLine + charScript.currentSpeaker);
	}


}
