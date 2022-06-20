using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AudioExplorerWindow : EditorWindow
{
	[MenuItem("B1NARY/Audio Explorer", priority = 1)]
	public static void ShowWindow()
	{
		// Get existing open window or if none, make a new one:
		var window = (AudioExplorerWindow)GetWindow(typeof(AudioExplorerWindow));
		window.titleContent = new GUIContent("B1NARY Audio Explorer");
		window.Show();
	}


	private static string ResourcesDirectory => Application.dataPath + "/Resources/";
	private bool clickedOnFolderView = false;
	private string[] fileNames, visualNames,
		directoryNames, directoryVisualNames;
	private string resourcesDirSounds = "";
	public void OnGUI()
	{
		EditorGUILayout.Space();
		SelectionGrid();
		bool canShowData = GetDataDirectory();
		if (canShowData == false)
			return;
		EditorGUI.indentLevel++;
		ShowDirectories(true);
		EditorGUILayout.Space();
		ShowFiles();
		EditorGUI.indentLevel--;
	}

	

	private int selection = 0;
	private static readonly string[] selectionArrayNames =
	{
		"Select",
		"Rename",
		"Mass Index Rename"
	};
	private int SelectionGrid()
	{
		Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 20);
		rect.x += 5;
		rect.width -= 10;
		selection = GUI.SelectionGrid(rect, selection, selectionArrayNames, selectionArrayNames.Length);
		return selection;
	}
	private bool GetDataDirectory()
	{
		EditorGUILayout.LabelField($"Searching in {ResourcesDirectory}");
		string newName = EditorGUILayout.DelayedTextField("Directory", resourcesDirSounds);
		if (newName != resourcesDirSounds || resourcesDirSounds == "" || clickedOnFolderView)
		{
			resourcesDirSounds = newName;
			if (Directory.Exists(ResourcesDirectory + resourcesDirSounds))
			{
				fileNames = Directory.GetFiles(ResourcesDirectory + resourcesDirSounds)
					.Where(fileName => fileName.EndsWith(".wav") || fileName.EndsWith(".mp4")).ToArray();
				visualNames = fileNames.Select(fileName => 
				fileName.Remove(0, fileName.LastIndexOfAny(new char[] { '/', '\\' }) + 1)).ToArray();
				directoryNames = Directory.GetDirectories(ResourcesDirectory + resourcesDirSounds);
				directoryVisualNames = directoryNames.Select(fileName => 
				fileName.Remove(0,fileName.LastIndexOfAny(new char[] { '/', '\\' }) + 1)).ToArray();
			}
			else
				fileNames = null;
		}
		if (fileNames == null)
		{
			EditorGUILayout.HelpBox("Not a valid directory!", MessageType.Error);
			return false;
		}
		return true;
	}
	private void ShowDirectories(bool showGoBackDirectory)
	{
		if (showGoBackDirectory)
			GoBackDirectory();
		for (int i = 0; i < directoryNames.Length; i++)
		{
			bool clickedFoldout = EditorGUI.Foldout(
				EditorGUI.IndentedRect(
					GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 16)),
				false, directoryVisualNames[i]);
			if (clickedFoldout)
			{
				resourcesDirSounds += $"/{directoryVisualNames[i]}";
				clickedOnFolderView = true;
			}
		}
	}
	private void GoBackDirectory()
	{
		string directorySoundsWithNoBrim = resourcesDirSounds.TrimStart('/', '\\');
		if (string.IsNullOrWhiteSpace(directorySoundsWithNoBrim))
			return;
		bool goBack = EditorGUI.Foldout(EditorGUI.IndentedRect(
			GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 16)),
			false, "../");
		if (goBack)
		{
			if (resourcesDirSounds.LastIndexOfAny(new char[] { '\\', '/' }) == -1)
				resourcesDirSounds = '/' + resourcesDirSounds;
			resourcesDirSounds = resourcesDirSounds.Remove(resourcesDirSounds.LastIndexOfAny(new char[] { '\\', '/' }));
		}
		EditorGUILayout.Space();
	}
	private void ShowFiles()
	{
		foreach (string file in visualNames)
		{
			EditorGUI.LabelField(EditorGUI.IndentedRect(
				GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 16)),
				file);
		}
	}


}