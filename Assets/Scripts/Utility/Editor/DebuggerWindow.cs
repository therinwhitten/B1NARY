using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Text.RegularExpressions;

public class DebuggerWindow : EditorWindow
{
	// regex for grabbing expressions
	private static readonly Regex emoteRegex = new Regex("\\[(.*?)\\]");
	// regex for commands
	private static readonly Regex commandRegex = new Regex("\\{(.*?)\\}");
	private enum Tabs : int
	{
		Scripts,
		Audio,
		Options
	}

	private static ScriptParser _parser;
	private static bool TryGetScriptParser(out ScriptParser scriptParser)
	{
		if (_parser == null)
			_parser = FindObjectOfType<ScriptParser>();
		if (_parser == null)
		{
			scriptParser = null;
			return false;
		}
		scriptParser = _parser;
		return true;
	}

	private static DialogueSystem _charScript;
	private static bool TryGetDialogueSystem(out DialogueSystem dialogueSys)
	{
		if (_charScript == null)
			_charScript = FindObjectOfType<DialogueSystem>();
		if (_charScript == null)
		{
			dialogueSys = null;
			return false;
		}
		dialogueSys = _charScript;
		return true;
	}

	private static readonly (Color color, string name)[] colors =
	{
		(Color.white, "White"),
		(Color.red, "Red"),
		(Color.cyan, "Cyan"),
		(Color.yellow, "Yellow"),
		(Color.magenta, "Magenta"),
		(Color.green, "Green"),
		(Color.blue, "Blue"),
	};

	private enum ColorType
	{
		Normal = 0,
		Selected = 1,
		Speaker = 2,
		Command = 3,
		Emote = 4,
	}


	[MenuItem("B1NARY/Debugger")]
	public static void ShowWindow()
	{
		
		// Get existing open window or if none, make a new one:
		DebuggerWindow window = (DebuggerWindow)GetWindow(typeof(DebuggerWindow));
		window.titleContent = new GUIContent("B1NARY Debugger");
		window.Show();
	}
	private static Dictionary<ColorType, int> GetNewDictionary()
	{
		Dictionary<ColorType, int> output = new Dictionary<ColorType, int>();
		foreach (ColorType colorType in Enum.GetValues(typeof(ColorType)))
			output.Add(colorType, 
				EditorPrefs.GetInt($"{colorType} B1NARY ColorLine", (int)colorType));
		return output;
	}



	private Dictionary<ColorType, int> currentColors = new Dictionary<ColorType, int>();
	private void OnGUI()
	{
		if (currentColors.Count != 5) // ColorType Enum Size
			currentColors = GetNewDictionary();
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
				ScriptsTab();
					//parser != null && parser.currentNode != null ? parser.currentNode.index : -1); 
				break;
			case Tabs.Audio: AudioTab(); break;
			case Tabs.Options: OptionsTab(); break;
		}
	}


	
	string currentScript = "";
	string[] currentScriptContents = Array.Empty<string>();
	Vector2 scrollPos = Vector2.zero;
	private void ScriptsTab()
	{
		SceneNameShow();
		ScriptNameShow();
		Rect lastRect = GUILayoutUtility.GetLastRect();
		lastRect.y += 4 + lastRect.height;
		lastRect.height = Screen.height - lastRect.y - 4;

		ScriptParser parser;
		if (!TryGetScriptParser(out parser))
			return;
		DisplayScrollView();
		

		void DisplayScrollView()
		{
			scrollPos = GUILayout.BeginScrollView(scrollPos);
			if (currentScript != parser.scriptName)
			{
				currentScript = parser.scriptName;
				currentScriptContents = File.ReadAllLines($"{Application.streamingAssetsPath}/Docs/{parser.scriptName}.txt");
			}
			int space = currentScriptContents.Length.ToString().Length;
			int onLine = parser.currentNode != null ? parser.currentNode.index : -1;


			for (int i = 0; i < currentScriptContents.Length; i++)
			{
				Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - 26, 16);
				rect.x += 6;
				if (onLine == i)
					GUI.color = colors[currentColors[ColorType.Selected]].color;
				else if (commandRegex.IsMatch(currentScriptContents[i]))
					GUI.color = colors[currentColors[ColorType.Command]].color;
				else if (emoteRegex.IsMatch(currentScriptContents[i]))
					GUI.color = colors[currentColors[ColorType.Emote]].color;
				else if (currentScriptContents[i].EndsWith("::"))
					GUI.color = colors[currentColors[ColorType.Speaker]].color;
				else
					GUI.color = colors[currentColors[ColorType.Normal]].color;
				GUI.Label(rect, $"{i + 1} {new string(' ', (space - (i + 1).ToString().Length) * 2)}    {currentScriptContents[i]}");
			}
			GUILayout.EndScrollView();
		}
		
	}
	private void AudioTab()
	{

	}
	private void OptionsTab()
	{
		try // Causes issues if you change values with a foreach
		{
			foreach (ColorType type in currentColors.Keys)
			{
				int @out = EditorGUILayout.Popup($"{type} Line Color", currentColors[type], colors.Select(tuple => tuple.name).ToArray());
				if (@out != currentColors[type])
				{
					currentColors[type] = @out;
					EditorPrefs.SetInt($"{type} B1NARY ColorLine", currentColors[type]);
				}
			}
		}
		catch (InvalidOperationException) { }
		bool pressedDeleteAllButton = GUILayout.Button("Reset Settings");
		if (pressedDeleteAllButton)
		{
			EditorPrefs.DeleteAll();
			foreach (ColorType colorType in Enum.GetValues(typeof(ColorType)))
				currentColors[colorType] = EditorPrefs.GetInt($"{colorType} B1NARY ColorLine", (int)colorType);
		}
	}

	private void SceneNameShow()
	{
		const string startingLine = "On Scene: ";
		EditorGUILayout.LabelField(startingLine + SceneManager.GetActiveScene().name);
	}
	private void ScriptNameShow()
	{
		const string startingLine = "On Script: ";
		if (TryGetScriptParser(out var scriptParser) && !string.IsNullOrEmpty(scriptParser.scriptName))
			EditorGUILayout.LabelField(startingLine + scriptParser.scriptName, EditorStyles.boldLabel);
		else
			EditorGUILayout.LabelField(startingLine + "NaN", EditorStyles.boldLabel);
	}
	private void CurrentLineShow()
	{
		const string startingLine = "On line: ";
		if (TryGetScriptParser(out var scriptParser) && scriptParser.currentNode != null)
			EditorGUILayout.LabelField(startingLine + (scriptParser.currentNode.index + 1), EditorStyles.boldLabel);
		else
			EditorGUILayout.LabelField(startingLine + "NaN", EditorStyles.boldLabel);
	}
	private void SpeakerShow()
	{
		const string startingLine = "On Character: ";
		if (TryGetDialogueSystem(out var charScript) && charScript.currentSpeaker.Length != 0)
			EditorGUILayout.LabelField(startingLine + charScript.currentSpeaker);
		else
			EditorGUILayout.LabelField(startingLine + "NaN");
	}
}
