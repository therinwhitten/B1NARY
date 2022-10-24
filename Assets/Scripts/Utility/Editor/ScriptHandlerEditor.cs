namespace B1NARY.Editor
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;
	using B1NARY.Scripting;

	[CustomEditor(typeof(ScriptHandler))]
	public class ScriptHandlerEditor : Editor
	{
		private static string BasePath => $"{Application.streamingAssetsPath}/Docs/";

		public override void OnInspectorGUI()
		{
			var serializedObj = new SerializedObject(target);
			var scriptHandler = (ScriptHandler)target;
			string[] allFullPaths = GetFullDocumentsPath(BasePath).ToArray(),
				visualPaths = allFullPaths.Select(str => str.Replace(BasePath, "").Replace(".txt", "")).ToArray();
			int oldIndex = Array.IndexOf(allFullPaths, scriptHandler.StartupScriptPath),
				newIndex = DirtyAuto.Popup(scriptHandler, new GUIContent("Starting Script"), oldIndex, visualPaths);
			if (oldIndex != newIndex)
				scriptHandler.StartupScriptPath = allFullPaths[newIndex];
			InputActions(scriptHandler);
			if (scriptHandler.IsActive)
				EditorGUILayout.LabelField($"Current Line: {scriptHandler.ScriptDocument.CurrentLine}");
		}
		public List<string> GetFullDocumentsPath(string currentPath)
		{
			var output = new List<string>(Directory.GetFiles(currentPath).Where(path => path.EndsWith(".txt")));
			IEnumerable<string> directories = Directory.GetDirectories(currentPath);
			if (directories.Any())
				foreach (string directory in directories)
					output.AddRange(GetFullDocumentsPath(directory));
			return output;
		}
		private void InputActions(in ScriptHandler scriptHandler)
		{
			serializedObject.Update();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ScriptHandler.playerInput)));
			if (scriptHandler.playerInput != null)
			{
				EditorGUI.indentLevel++;
				if (GUILayout.Button("Add"))
					Array.Resize(ref scriptHandler.nextLineButtons, scriptHandler.nextLineButtons.Length + 1);
				for (int i = 0; i < scriptHandler.nextLineButtons.Length; i++)
				{
					Rect fullRect = GUILayoutUtility.GetRect(Screen.width, 20),
						textEditorRect = new Rect(fullRect) { xMax = fullRect.xMax / 5 * 4 },
						deleteButtonRect = new Rect(fullRect) { xMin = textEditorRect.xMax + 2 };
					try { scriptHandler.playerInput.actions
						.FindAction(scriptHandler.nextLineButtons[i], true); }
					catch { EditorGUILayout.HelpBox(
						$"'{scriptHandler.nextLineButtons[i]}' may not exist in the player input!", 
						MessageType.Error); }
					scriptHandler.nextLineButtons[i] = DirtyAuto
						.Field(textEditorRect, scriptHandler, new GUIContent($"Action {i}"), 
						scriptHandler.nextLineButtons[i]);
					if (GUI.Button(deleteButtonRect, "Remove"))
					{
						EditorUtility.SetDirty(scriptHandler);
						List<string> newArray = scriptHandler.nextLineButtons.ToList();
						newArray.RemoveAt(i);
						scriptHandler.nextLineButtons = newArray.ToArray();
					}
				}
				EditorGUI.indentLevel--;
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}