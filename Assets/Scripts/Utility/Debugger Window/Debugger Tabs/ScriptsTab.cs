#if UNITY_EDITOR
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
	using B1NARY.DataPersistence;

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


		public override GUIContent Name => new("Scripts");
		public override bool ConstantlyRepaint => false;
		public override void DisplayTab()
		{
			if (!Application.isPlaying)
			{
				EditorGUILayout.HelpBox("Cannot show script when it is not in " +
					"play mode.", MessageType.Info);
				return;
			}
			if (!ScriptHandler.HasInstance)
			{
				EditorGUILayout.HelpBox($"{nameof(SaveSlot)} does not exist!" +
					" Make sure to add it to your scene!", MessageType.Error);
				return;

			}
			if (ScriptHandler.Instance.HasDocument)
			{
				EditorGUILayout.HelpBox("Script Document Contents cannot be " +
					"loaded due to not being properly intialized yet.", MessageType.Info);
				return;
			}
			scroll = EditorGUILayout.BeginScrollView(scroll);
			ShowScriptScroll(ScriptHandler.Instance);
			EditorGUILayout.EndScrollView();
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
			for (int i = 0; i < scriptHandler.document.Lines.Count; i++)
			{
				Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - 26, 16);
				rect.x += 6;
				ScriptLine onLine =  scriptHandler.document.Lines[i].PrimaryLine;
				if (onLine.Index == i)
					GUI.color = assignedColors[1];
				else GUI.color = onLine.Type switch
				{
					ScriptLine.LineType.Entry => assignedColors[2],
					ScriptLine.LineType.Command => assignedColors[3],
					ScriptLine.LineType.Attribute => assignedColors[4],
					_ => assignedColors[0],
				};
				GUI.Label(rect, $"{(onLine.Index == i && EditorPrefs.GetBool("Script B1NARY Pointer", true) ? ">" : (i + 1).ToString())}\t{onLine.RawLine}");
			}
			GUILayout.EndScrollView();
		}


		public override int Order => 5;
		public override DebuggerPreferences DebuggerPreferences => new()
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
	}
}
#endif