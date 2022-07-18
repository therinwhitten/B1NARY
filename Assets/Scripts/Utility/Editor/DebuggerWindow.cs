using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class DebuggerWindow : EditorWindow
{
	public static class TryGetter<T> where T : MonoBehaviour
	{
		private static T valueCache;
		public static bool TryGetObject(out T value)
		{
			if (valueCache == null)
				valueCache = FindObjectOfType<T>();
			if (valueCache == null)
			{
				value = null;
				return false;
			}
			value = valueCache;
			return true;
		}
	}



	private static readonly Vector2Int defaultMinSize = new Vector2Int(300, 350);

	[MenuItem("B1NARY/Debugger", priority = 1)]
	public static void ShowWindow()
	{
		
		// Get existing open window or if none, make a new one:
		DebuggerWindow window = GetWindow<DebuggerWindow>();
		window.titleContent = new GUIContent("B1NARY Debugger");
		window.minSize = defaultMinSize;
		window.Show();
	}

	private void OnGUI()
	{
		TopBar();
		//EditorGUILayout.Space(10);
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


	Vector2 scrollPos = Vector2.zero;
	int selected = 0, oldTabLength = -1;
	private void ShowTabs()
	{
		const int slotHeight = 20;
		int slotsEach = EditorPrefs.GetInt("B1NARY Slots Debugger", 3);
		string[] tabNames = DebuggerTab.ShownTabs.Select(tab => tab.Name).ToArray();
		if (oldTabLength != tabNames.Length)
			if (oldTabLength == -1)
				oldTabLength = tabNames.Length;
			else
			{
				selected += tabNames.Length - oldTabLength;
				oldTabLength += tabNames.Length - oldTabLength;
			}
		int RectHeight = 0;
		for (int i = 0; i < tabNames.Length; i += slotsEach)
			RectHeight += slotHeight;
		Rect guiRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, RectHeight);
		guiRect.width -= 10;
		guiRect.x += 5;
		selected = GUI.SelectionGrid(guiRect, selected, tabNames, slotsEach);
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		DebuggerTab.ShownTabs[selected].DisplayTab();
		EditorGUILayout.EndScrollView();
	}

	
	private void CurrentLineShow()
	{
		const string startingLine = "On line: ";
		if (TryGetter<ScriptParser>.TryGetObject(out var scriptParser) && scriptParser.currentNode != null && scriptParser.currentNode.GetCurrentLine() != null)
			EditorGUILayout.LabelField(startingLine + (scriptParser.currentNode.GetCurrentLine().index + 1), EditorStyles.boldLabel);
		else
			EditorGUILayout.LabelField(startingLine + "NaN", EditorStyles.boldLabel);
	}
	private void SpeakerShow()
	{
		string startingLine = "On Character: Empty";
		string[] bottomLengthLabel = { "NaN", "/", "NaN" };
		float percentage = 0;
		if (TryGetter<DialogueSystem>.TryGetObject(out var charScript) && charScript.currentSpeaker.Length != 0)
			startingLine = startingLine.Replace("Empty", charScript.currentSpeaker);
		if (TryGetter<AudioHandler>.TryGetObject(out var audioHandler))
		{
			try // Try/Catching is easier than trying to fit all the bool requirements.
			{
				percentage = audioHandler.VoiceActorHandler.audioSource.time / audioHandler.VoiceActorHandler.audioSource.clip.length;
				bottomLengthLabel[0] = audioHandler.VoiceActorHandler.audioSource.time.ToString("N2");
				bottomLengthLabel[2] = audioHandler.VoiceActorHandler.audioSource.clip.length.ToString("N2");
			}
			catch (NullReferenceException) { }
			catch (MissingReferenceException) { }
		}
		EditorGUI.ProgressBar(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 20), percentage, string.Join(" ", bottomLengthLabel));
		EditorGUI.LabelField(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 20), startingLine, EditorStyles.whiteMiniLabel);
	}
}