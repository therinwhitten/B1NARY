using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class AudioExplorerWindow : EditorWindow
{
	private enum Behaviour
	{
		Selection,
		Rename_Files,
		Mass_Rename_Files,
		Delete_Files,
		Rename_Directories,
	}
	private readonly static string[] BehaviourNames = Enum.GetNames(typeof(Behaviour))
		.Select(@string => @string.Replace('_', ' ')).ToArray();

	private static readonly HashSet<string> availibleAudioExtensions = new HashSet<string>()
	{
		".wav",
		".mp3"
	};
	private static bool VerifyFileExtension(string input) =>
		availibleAudioExtensions.Contains(input.Substring(input.LastIndexOf('.')));

	[MenuItem("B1NARY/Audio Explorer", priority = 1)]
	public static void ShowWindow()
	{
		// Get existing open window or if none, make a new one:
		var window = (AudioExplorerWindow)GetWindow(typeof(AudioExplorerWindow));
		window.titleContent = new GUIContent("B1NARY Audio Explorer");
		window.minSize = new Vector2(350, 200);
		window.Show();
	}

	private void OnEnable()
	{
		needsRefresh = true;
	}

	public bool needsRefresh = false;
	private static string ResourcesDirectory => Application.dataPath + "/Resources/";
	private bool showInefficientSoundFiltering = false;
	private string[] fileNames, visualNames,
		directoryNames, directoryVisualNames;
	private string resourcesDirSounds = "";
	private Vector2 scrollPos = Vector2.zero;
	public void OnGUI()
	{
		EditorGUILayout.Space();
		SelectionGrid();
		bool outputExperimental = EditorGUILayout.ToggleLeft("Filter Folders with sounds", showInefficientSoundFiltering);
		if (outputExperimental != showInefficientSoundFiltering)
		{
			needsRefresh = true;
			showInefficientSoundFiltering = outputExperimental;
		}
		bool canShowData = GetDataDirectory();
		if (!canShowData)
			return;
		EditorGUI.indentLevel++;
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		ShowDirectories(true, (Behaviour)selection);
		EditorGUILayout.Space();
		ShowFiles((Behaviour)selection);
		EditorGUI.indentLevel--;
		EditorGUILayout.EndScrollView();
		ShowOpenInExplorerButton();
	}

	

	private int selection = 0;
	private int SelectionGrid()
	{
		Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 20);
		rect.x += 5;
		rect.width -= 10;
		string[] behaviourNamed = BehaviourNames;
		int newSelection = GUI.SelectionGrid(rect, selection, behaviourNamed, behaviourNamed.Length);
		if ((Behaviour)newSelection == Behaviour.Mass_Rename_Files)
			MassRenameAudioExplorerWindow.GetWindow(fileNames);
		else if (newSelection != selection)
		{
			needsRefresh = true;
			lastPressed = -1;
			selection = newSelection;
		}
		return selection;
	}
	private bool GetDataDirectory()
	{
		EditorGUILayout.LabelField($"Searching in {ResourcesDirectory}");
		string newName = EditorGUILayout.DelayedTextField("Directory", resourcesDirSounds);
		if (newName != resourcesDirSounds || needsRefresh)
		{
			resourcesDirSounds = newName;
			if (Directory.Exists(ResourcesDirectory + resourcesDirSounds))
			{
				fileNames = Directory.GetFiles(ResourcesDirectory + resourcesDirSounds)
					.Where(fileName => VerifyFileExtension(fileName)).ToArray();
				visualNames = fileNames.Select(fileName =>
					fileName.Remove(0, fileName.LastIndexOfAny(new char[] { '/', '\\' }) + 1)).ToArray();
				Array.Sort(visualNames, (x, y) => string.Compare(x, y));
				try { NumericalSort(visualNames); } // Having numbers in strings while others are not causes issues
				catch { }
				
				directoryNames = Directory.GetDirectories(ResourcesDirectory + resourcesDirSounds);
				if (showInefficientSoundFiltering)
					directoryNames = DoInefficientSoundFiltering(directoryNames).Result;
				directoryVisualNames = directoryNames.Select(fileName => 
					fileName.Remove(0,fileName.LastIndexOfAny(new char[] { '/', '\\' }) + 1)).ToArray();
				Array.Sort(directoryVisualNames, (x, y) => string.Compare(x, y));
				needsRefresh = false;
				lastPressed = -1;
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
	private void ShowDirectories(bool showGoBackDirectory, Behaviour selection)
	{
		if (showGoBackDirectory)
			GoBackDirectory();
		if (selection == Behaviour.Rename_Directories)
		{
			for (int i = 0; i < directoryNames.Length; i++)
			{
				string newName = EditorGUI.DelayedTextField(EditorGUI.IndentedRect(
						GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 18)),
					directoryVisualNames[i]);
				if (newName != directoryVisualNames[i])
				{
					Directory.Move(directoryNames[i], directoryNames[i].Replace(directoryVisualNames[i], newName));
					Directory.Move(directoryNames[i] + ".meta", (directoryNames[i] + ".meta").Replace(directoryVisualNames[i], newName));
				}

				EditorGUILayout.Space(2);
			}
			return;
		}
		for (int i = 0; i < directoryNames.Length; i++)
		{
			Rect rect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 16));
			EditorGUI.LabelField(rect, directoryVisualNames[i], EditorStyles.foldout);
			bool clickedFoldout = GUI.Button(rect, "", GUIStyle.none);
			if (clickedFoldout)
			{
				resourcesDirSounds += $"/{directoryVisualNames[i]}";
				needsRefresh = true;
				lastPressed = -1;
			}
		}
	}
	private void GoBackDirectory()
	{
		string directorySoundsWithNoBrim = resourcesDirSounds.TrimStart('/', '\\');
		if (string.IsNullOrWhiteSpace(directorySoundsWithNoBrim))
			return;
		Rect rect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 16));
		EditorGUI.LabelField(rect, "../", EditorStyles.foldout);
		bool goBack = GUI.Button(rect, "", GUIStyle.none);
		if (goBack)
		{
			if (resourcesDirSounds.LastIndexOfAny(new char[] { '\\', '/' }) == -1)
				resourcesDirSounds = '/' + resourcesDirSounds;
			resourcesDirSounds = resourcesDirSounds.Remove(resourcesDirSounds.LastIndexOfAny(new char[] { '\\', '/' }));
			needsRefresh = true;
		}
		EditorGUILayout.Space();
	}
	private async Task<string[]> DoInefficientSoundFiltering(IEnumerable<string> directoryInput)
	{
		var directoryRemove = new Dictionary<string, bool>();
		var tasks = new List<Task<(string name, bool hasSoundFiles)>>();
		foreach (string directory in directoryNames)
		{
			tasks.Add(HasSoundFiles(directory));
			//directoryRemove.Add(directory, );
		}
		await Task.WhenAll(tasks);
		foreach (var (name, hasSoundFile) in tasks.Select(task => task.Result))
			directoryRemove.Add(name, hasSoundFile);
		return directoryInput.Where(dir => directoryRemove[dir] == true).ToArray();

		async Task<(string name, bool hasSoundFiles)> HasSoundFiles(string directory)
		{
			if (Directory.GetFiles(directory).Where(fileName => VerifyFileExtension(fileName)).Any())
				return (directory, true);
			var boolEnum = Directory.GetDirectories(directory);
			var tasksRecursion = new List<Task<(string name, bool hasSoundFiles)>>();
			for (int i = 0; i < boolEnum.Length; i++)
				tasksRecursion.Add(HasSoundFiles(boolEnum[i]));
			await Task.WhenAll(tasksRecursion);
			if (boolEnum.Any())
				return (directory, tasksRecursion.Select(task => task.Result.hasSoundFiles).Contains(true));
			return (directory, false);
		}
	}



	private int lastPressed = -1;
	private void ShowFiles(Behaviour behaviour)
	{
		switch (behaviour)
		{
			case Behaviour.Rename_Directories:
				for (int i = 0; i < fileNames.Length; i++)
				{
					Rect hitbox = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 16));
					EditorGUI.LabelField(hitbox, visualNames[i], EditorStyles.label);
				}
				break;
			case Behaviour.Selection:
				for (int i = 0; i < fileNames.Length; i++)
				{
					Rect hitbox = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 16));
					bool pressed = GUI.Button(hitbox, "", GUIStyle.none);
					EditorGUI.LabelField(hitbox, visualNames[i], lastPressed == i ? EditorStyles.boldLabel : EditorStyles.label);
					if (pressed)
					{
						if (lastPressed.Equals(i))
							Process.Start(fileNames[i]);
						else
							lastPressed = i;
					}
				}
				break;
			case Behaviour.Rename_Files:
				for (int i = 0; i < visualNames.Length; i++)
				{
					string file = visualNames[i].Remove(visualNames[i].LastIndexOf('.')),
						fileExtension = visualNames[i].Substring(visualNames[i].LastIndexOf('.'));
					string newName = EditorGUI.DelayedTextField(EditorGUI.IndentedRect(
						GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 18)),
						file);
					if (file != newName)
					{
						File.Move($"{ResourcesDirectory}{resourcesDirSounds}/{file}{fileExtension}", 
							$"{ResourcesDirectory}{resourcesDirSounds}/{newName}{fileExtension}");
						File.Move($"{ResourcesDirectory}{resourcesDirSounds}/{file}{fileExtension}.meta",
							$"{ResourcesDirectory}{resourcesDirSounds}/{newName}{fileExtension}.meta");
						needsRefresh = true;
					}

					EditorGUILayout.Space(2);
				}
				break;
			case Behaviour.Delete_Files:
				for (int i = 0; i < fileNames.Length; i++)
				{
					Rect hitbox = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 16));
					bool pressed = GUI.Button(hitbox, "", GUIStyle.none);
					if (i == lastPressed)
						GUI.color = Color.red;
					EditorGUI.LabelField(hitbox, visualNames[i], lastPressed == i ? EditorStyles.foldoutHeader : EditorStyles.foldout);
					if (pressed)
					{
						if (lastPressed.Equals(i))
						{
							File.Delete(fileNames[i]);
							File.Delete(fileNames[i] + ".meta");
							needsRefresh = true;
						}
						else
							lastPressed = i;
					}
					GUI.color = Color.white;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException(behaviour.ToString());
		}
		
	}
	private void ShowOpenInExplorerButton()
	{
		Rect rectButton = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 24);
		rectButton.x += 5;
		rectButton.width -= 10;
		rectButton.height -= 4;
		bool openExplorer = GUI.Button(rectButton, "Open in file explorer");
		if (openExplorer)
			Process.Start($"{ResourcesDirectory}{resourcesDirSounds}");
	}


	// https://stackoverflow.com/questions/6723487/how-to-sort-a-string-array-by-numeric-style
	public static string[] NumericalSort(string[] array)
	{
		Regex rgx = new Regex("([^0-9]*)([0-9]+)");
		Array.Sort(array, (a, b) =>
		{
			var ma = rgx.Matches(a);
			var mb = rgx.Matches(b);
			for (int i = 0; i < ma.Count; ++i)
			{
				int ret = ma[i].Groups[1].Value.CompareTo(mb[i].Groups[1].Value);
				if (ret != 0)
					return ret;

				ret = int.Parse(ma[i].Groups[2].Value) - int.Parse(mb[i].Groups[2].Value);
				if (ret != 0)
					return ret;
			}

			return 0;
		});
		return array;
	}
}
