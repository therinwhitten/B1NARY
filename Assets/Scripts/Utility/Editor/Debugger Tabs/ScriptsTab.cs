namespace B1NARY.Editor.Debugger
{
	using System;
	using System.IO;
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using UnityEditor;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using B1NARY.Scripting;

	public sealed class ScriptsTab : DebuggerTab
	{
		private static readonly string[] colorNames =
		{
			"White",
			"Red",
			"Cyan",
			"Yellow",
			"Magenta",
			"Green",
			"Blue",
		};
		private static readonly Color[] colors =
		{
			Color.white,
			Color.red,
			Color.cyan,
			Color.yellow,
			Color.magenta,
			Color.green,
			Color.blue,
		};


		public override GUIContent Name => new GUIContent("Scripts");
		public override bool ConstantlyRepaint => false;
		public override void DisplayTab()
		{

		}

		private Vector2 scroll = Vector2.zero;
		private void ShowScriptScroll(ScriptHandler scriptHandler)
		{
			if (!scriptHandler.IsActive)
				return;
			Color[] assignedColors =
			{
				colors[EditorPrefs.GetInt("Normal B1NARY Color", 0)],
				colors[EditorPrefs.GetInt("Selected B1NARY Color", 1)],
				colors[EditorPrefs.GetInt("Speaker B1NARY Color", 2)],
				colors[EditorPrefs.GetInt("Command B1NARY Color", 3)],
				colors[EditorPrefs.GetInt("Emote B1NARY Color", 4)],
			};
			/* TODO: Fix later
			for (int i = 0; i < scriptHandler.documentData.Length; i++)
			{
				Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - 26, 16);
				rect.x += 6;
				if (onLine == i)
					GUI.color = assignedColors[1];
				else if (commandRegex.IsMatch(currentScriptContents[i]))
					GUI.color = assignedColors[3];
				else if (emoteRegex.IsMatch(currentScriptContents[i]))
					GUI.color = assignedColors[4];
				else if (currentScriptContents[i].EndsWith("::"))
					GUI.color = assignedColors[2];
				else
					GUI.color = assignedColors[0];
				GUI.Label(rect, $"{(onLine == i && EditorPrefs.GetBool("Script B1NARY Pointer", true) ? ">" : (i + 1).ToString())}\t{currentScriptContents[i]}");
			}
			GUILayout.EndScrollView();
			*/
		}


		public override int Order => 5;
		public override DebuggerPreferences DebuggerPreferences => new DebuggerPreferences()
		{
			[DebuggerPreferences.DataType.Bool] = new List<(string name, object @default)>()
			{
				("Script B1NARY Pointer", true)
			},
			[DebuggerPreferences.DataType.StringPopup] = new List<(string name, object @default)>()
			{
				("Normal B1NARY Color", (colorNames, 0)),
				("Selected B1NARY Color", (colorNames, 1)),
				("Speaker B1NARY Color", (colorNames, 2)),
				("Command B1NARY Color", (colorNames, 3)),
				("Emote B1NARY Color", (colorNames, 4))
			}
		};
		/*
		private static readonly string[] colorNames =
		{
		"White",
		"Red",
		"Cyan",
		"Yellow",
		"Magenta",
		"Green",
		"Blue",
	};
		private static readonly Color[] colors =
		{
		Color.white,
		Color.red,
		Color.cyan,
		Color.yellow,
		Color.magenta,
		Color.green,
		Color.blue,
	};
		// regex for grabbing expressions
		private static readonly Regex emoteRegex = new Regex("\\[(.*?)\\]");
		// regex for commands
		private static readonly Regex commandRegex = new Regex("\\{(.*?)\\}");



		public override string Name => "Scripts";
		public override void DisplayTab()
		{
			SceneNameShow();
			ScriptNameShow();
			Rect lastRect = GUILayoutUtility.GetLastRect();
			lastRect.y += 4 + lastRect.height;
			lastRect.height = Screen.height - lastRect.y - 4;

			if (DebuggerWindow.TryGetter<ScriptHandler>.TryGetObject(out ScriptHandler parser))
				ShowScriptScroll(parser).Wait();
		}

		string currentScript = "";
		string[] currentScriptContents = Array.Empty<string>();
		Vector2 scrollPos = Vector2.zero;
		private async Task ShowScriptScroll(ScriptHandler parser)
		{
			scrollPos = GUILayout.BeginScrollView(scrollPos);
			Task waitForScriptContentsTask = Task.CompletedTask;
			if (currentScript != parser.scriptName)
			{
				currentScript = parser.scriptName;
				waitForScriptContentsTask = Task.Factory.StartNew(
					() => currentScriptContents = File.ReadAllLines($"{Application.streamingAssetsPath}/Docs/{parser.scriptName}.txt"));
			}
			int onLine = parser.currentNode != null && parser.currentNode.GetCurrentLine() != null ? parser.currentNode.GetCurrentLine().index - 1 : -1;
			Color[] assignedColors =
			{
			colors[EditorPrefs.GetInt("Normal B1NARY Color", 0)],
			colors[EditorPrefs.GetInt("Selected B1NARY Color", 1)],
			colors[EditorPrefs.GetInt("Speaker B1NARY Color", 2)],
			colors[EditorPrefs.GetInt("Command B1NARY Color", 3)],
			colors[EditorPrefs.GetInt("Emote B1NARY Color", 4)],
		};
			await waitForScriptContentsTask;
			for (int i = 0; i < currentScriptContents.Length; i++)
			{
				Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - 26, 16);
				rect.x += 6;
				if (onLine == i)
					GUI.color = assignedColors[1];
				else if (commandRegex.IsMatch(currentScriptContents[i]))
					GUI.color = assignedColors[3];
				else if (emoteRegex.IsMatch(currentScriptContents[i]))
					GUI.color = assignedColors[4];
				else if (currentScriptContents[i].EndsWith("::"))
					GUI.color = assignedColors[2];
				else
					GUI.color = assignedColors[0];
				GUI.Label(rect, $"{(onLine == i && EditorPrefs.GetBool("Script B1NARY Pointer", true) ? ">" : (i + 1).ToString())}\t{currentScriptContents[i]}");
			}
			GUILayout.EndScrollView();
			GUI.color = Color.white;
			if (GUILayout.Button("Open scriptName"))
				Process.Start($"{Application.streamingAssetsPath}/Docs/{parser.scriptName}.txt", EditorPrefs.GetString("ExternalScriptEditor")); // TODO: somehow make it launch the external scripts editor as well
		}
		private void SceneNameShow()
		{
			const string startingLine = "On Scene: ";
			EditorGUILayout.LabelField(startingLine + SceneManager.GetActiveScene().name);
		}
		private void ScriptNameShow()
		{
			const string startingLine = "On Script: ";
			if (DebuggerWindow.TryGetter<ScriptParser>.TryGetObject(out var scriptParser) && !string.IsNullOrEmpty(scriptParser.scriptName))
				EditorGUILayout.LabelField(startingLine + scriptParser.scriptName, EditorStyles.boldLabel);
			else
				EditorGUILayout.LabelField(startingLine + "NaN", EditorStyles.boldLabel);
		}

		public override int Order => 5;
		public override DebuggerPreferences DebuggerPreferences => new DebuggerPreferences()
		{
			[DebuggerPreferences.DataType.Bool] = new List<(string name, object @default)>()
		{
			("Script B1NARY Pointer", true)
		},
			[DebuggerPreferences.DataType.StringPopup] = new List<(string name, object @default)>()
		{
			("Normal B1NARY Color", (colorNames, 0)),
			("Selected B1NARY Color", (colorNames, 1)),
			("Speaker B1NARY Color", (colorNames, 2)),
			("Command B1NARY Color", (colorNames, 3)),
			("Emote B1NARY Color", (colorNames, 4))
		}
		};
		*/
	}
}