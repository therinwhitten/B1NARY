using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class MassRenameAudioExplorerWindow : EditorWindow
{
	// https://stackoverflow.com/questions/9363145/regex-for-extracting-filename-from-path#comment85522328_9363293
	
	// Regex if a string is only numbers
	public static readonly Regex numberRegex = new Regex("^[0-9]*$");
	// Regex for getting the fileName only
	public static readonly Regex fileNameFilterRegex = new Regex(@"[ \w-]+?(?=\.)");
	// Regex for getting the fileName and extension
	public static readonly Regex fileNameAndExtensionRegexFilter = new Regex(@"[^\\]+$");

	public static MassRenameAudioExplorerWindow GetWindow(string[] fullFilePaths)
	{
		var window = GetWindow<MassRenameAudioExplorerWindow>();
		window.titleContent = new GUIContent("Mass Audio Renamer");
		window.ShowTab();
		var fileList = new List<(string fullPath, string fileName, int index)>();
		for (int i = 0; i < fullFilePaths.Length; i++)
		{
			if (!int.TryParse(fileNameFilterRegex.Match(fullFilePaths[i]).Value, out int result))
				continue;
			string fileName = fileNameAndExtensionRegexFilter.Match(fullFilePaths[i]).Value;
			fileList.Add((fullFilePaths[i], fileName, result));
		}
		window.FullFilePaths = AudioExplorerWindow.NumericalSort(fileList.Select(tuple => tuple.fullPath).ToArray());
		window.FileNames = AudioExplorerWindow.NumericalSort(fileList.Select(tuple => tuple.fileName).ToArray());
		window.NumberedRepresentations = fileList.Select(tuple => tuple.index).ToList().ToArray();
		Array.Sort(window.NumberedRepresentations);
		return window;
	}

	public string[] FullFilePaths { get; private set; }
	public string[] FileNames { get; private set; }
	public int[] NumberedRepresentations { get; private set; }
	private int startIndex = 0, endIndex = 0;
	private Vector2 Vector2 = Vector2.zero;
	private bool onStartIndex = true;

	private int lowerBy = 0, raiseBy = 0, selection = 0;
	private void OnGUI()
	{
		startIndex = EditorGUILayout.IntSlider("Start Index", startIndex, 0, FullFilePaths.Length);
		endIndex = EditorGUILayout.IntSlider("End Index", endIndex, 0 + startIndex, FullFilePaths.Length);
		float startFloat = startIndex, endFloat = endIndex;
		EditorGUILayout.MinMaxSlider(ref startFloat, ref endFloat, 0, FullFilePaths.Length);
		startIndex = (int)startFloat;
		endIndex = (int)endFloat;

		Vector2 = EditorGUILayout.BeginScrollView(Vector2);
		for (int i = 0; i < FullFilePaths.Length; i++)
		{
			bool isSelected = false;
			if (InRange(i, startIndex, endIndex))
			{
				GUI.color = Color.yellow;
				isSelected = true;
			}
			else
				GUI.color = Color.white;
			var stringBuilder = new StringBuilder(i.ToString());
			stringBuilder.Append(new string(' ', 4 + (FullFilePaths.Length.ToString().Length - i.ToString().Length)));
			stringBuilder.Append(FileNames[i]);
			EditorGUILayout.LabelField(stringBuilder.ToString(), isSelected ? EditorStyles.boldLabel : EditorStyles.label);
			if (GUI.Button(GUILayoutUtility.GetLastRect(), "", GUIStyle.none))
			{
				if (onStartIndex || i < startIndex)
				{
					startIndex = i;
					onStartIndex = false;
				}
				else
				{
					endIndex = i;
					onStartIndex = true;
				}
			}
		}
		GUI.color = Color.white;
		EditorGUILayout.EndScrollView();
		selection = GUILayout.SelectionGrid(selection, new string[] { "Decrease", "Increase" }, 2);
		switch (selection)
		{
			case 0: // Decrease
					// Get the highest value that is sitting below the current start,
					// - and cap there.
				var numEnumDecrease = NumberedRepresentations.Where(num => num < NumberedRepresentations[startIndex]);
				int belowMin = startIndex == 0 ? 0 : numEnumDecrease.Max(),
					differenceDec = NumberedRepresentations[startIndex] - belowMin - 1;
				lowerBy = EditorGUILayout.IntSlider(lowerBy, 0, differenceDec);
				if (GUILayout.Button("Commit"))
				{
					for (int i = startIndex; i <= endIndex; i++)
					{
						File.Move(FullFilePaths[i], FullFilePaths[i]
							.Replace(FileNames[i], FileNames[i]
							.Replace(NumberedRepresentations[i].ToString(), 
							(NumberedRepresentations[i] - lowerBy).ToString())));
						File.Move(FullFilePaths[i] + ".meta", FullFilePaths[i]
							.Replace(FileNames[i], FileNames[i]
							.Replace(NumberedRepresentations[i].ToString(),
							(NumberedRepresentations[i] - lowerBy).ToString())) + ".meta");
					}
					Debug.Log($"Shifted {endIndex - startIndex + 1} files with index lowered by {lowerBy}");
					OnLostFocus();
				}
				break;
			case 1: // Increase
				var numEnumIncrease = NumberedRepresentations.Where(num => num > NumberedRepresentations[endIndex]);
				int aboveMin = endIndex == NumberedRepresentations.Length - 1 ? int.MaxValue
					: numEnumIncrease.Min(),
					differenceInc = aboveMin - NumberedRepresentations[endIndex] - 1;
				raiseBy = EditorGUILayout.IntSlider(raiseBy, 0, differenceInc);
				if (GUILayout.Button("Commit"))
				{
					for (int i = startIndex; i <= endIndex; i++)
					{
						File.Move(FullFilePaths[i], FullFilePaths[i]
							.Replace(FileNames[i], FileNames[i]
							.Replace(NumberedRepresentations[i].ToString(),
							(NumberedRepresentations[i] + raiseBy).ToString())));
						File.Move(FullFilePaths[i] + ".meta", FullFilePaths[i]
							.Replace(FileNames[i], FileNames[i]
							.Replace(NumberedRepresentations[i].ToString(),
							(NumberedRepresentations[i] + raiseBy).ToString())) + ".meta");
					}
					Debug.Log($"Shifted {endIndex - startIndex + 1} files with index raised by {raiseBy}");
					Debug.Log("{0} Files changed");
					OnLostFocus();
				}
				break;
		}
		

		bool InRange(int value, int min, int max)
			=> value > max ? false : value < min ? false : true;
	}


	private void OnLostFocus()
	{
		Close();
		GetWindow<AudioExplorerWindow>().needsRefresh = true;
	}
}