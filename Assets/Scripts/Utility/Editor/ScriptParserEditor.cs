using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScriptParser))]
public class ScriptParserEditor : Editor
{
	private static string basePath => $"{Application.streamingAssetsPath}/Docs/";

	public override void OnInspectorGUI()
	{
		var scriptParser = (ScriptParser)target;
		string[] paths = GetDocs(basePath).Select(str => str.Replace(basePath, "").Replace(".txt", "")).ToArray();
		int oldIndex = Array.IndexOf(paths, scriptParser.scriptName);
		int newIndex = EditorGUILayout.Popup("Starting Script", oldIndex, paths);
		if (oldIndex != newIndex)
			scriptParser.scriptName = paths[newIndex];
		EditorGUILayout.LabelField($"Is Paused: {scriptParser.paused}", EditorStyles.boldLabel);
	}

	private static List<string> GetDocs(string currentPath)
	{
		var output = new List<string>();
		output.AddRange(Directory.GetFiles(currentPath).Where(path => path.EndsWith(".txt")));
		IEnumerable<string> directories = Directory.GetDirectories(currentPath);
		if (directories.Any())
			foreach (string directory in directories)
				output.AddRange(GetDocs(directory));
		return output;
	}
}