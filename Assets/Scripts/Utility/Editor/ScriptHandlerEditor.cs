namespace B1NARY.Editor
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;
	using B1NARY.Scripting.Experimental;

	[CustomEditor(typeof(ScriptHandler))]
	public class ScriptHandlerEditor : Editor
	{
		private static string basePath => $"{Application.streamingAssetsPath}/Docs/";

		public override void OnInspectorGUI()
		{
			var scriptHandler = (ScriptHandler)target;
			EditorUtility.SetDirty(scriptHandler);
			string[] allFullPaths = GetFullDocumentsPath(basePath).ToArray(),
				visualPaths = allFullPaths.Select(str => str.Replace(basePath, "").Replace(".txt", "")).ToArray();
			int oldIndex = Array.IndexOf(allFullPaths, scriptHandler.StartupScriptPath),
				newIndex = EditorGUILayout.Popup("Starting Script", oldIndex, visualPaths);
			if (oldIndex != newIndex)
				scriptHandler.StartupScriptPath = allFullPaths[newIndex];
			if (scriptHandler.IsActive)
				Inspect(scriptHandler.ScriptDocument.documentData.ToArray());
			EditorUtility.ClearDirty(scriptHandler);
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

		private Vector2 scroll = Vector2.zero;
		private void Inspect(ScriptLine[] data)
		{
			Rect scrollWindowRect = GUILayoutUtility.GetRect(Screen.width, 140),
				viewBoxRect = scrollWindowRect;
			viewBoxRect.height = data.Length * 20;
			scroll = GUI.BeginScrollView(scrollWindowRect, scroll, viewBoxRect, true, false);
			for (int i = 0; i < data.Length; i++)
			{
				EditorGUILayout.LabelField(data[i].ToString());
			}
			GUI.EndScrollView();
		}
	}
}