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
		DebuggerWindow window = (DebuggerWindow)GetWindow(typeof(DebuggerWindow));
		window.titleContent = new GUIContent("B1NARY Debugger");
		window.minSize = defaultMinSize;
		window.Show();
	}

	private void OnGUI()
	{
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
		Rect guiRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, RectHeight - 10);
		guiRect.width -= 10;
		guiRect.x += 5;
		guiRect.y -= 10;
		guiRect.height += 10;
		selected = GUI.SelectionGrid(guiRect, selected, tabNames, slotsEach);
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		DebuggerTab.ShownTabs[selected].DisplayTab();
		EditorGUILayout.EndScrollView();
	}

	
	private void CurrentLineShow()
	{
		const string startingLine = "On line: ";
		if (TryGetter<ScriptParser>.TryGetObject(out var scriptParser) && scriptParser.currentNode != null)
			EditorGUILayout.LabelField(startingLine + (scriptParser.currentNode.index + 1), EditorStyles.boldLabel);
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
				percentage = audioHandler.VoiceActorHandler.SpeakerCoroutine.AudioSource.time / audioHandler.VoiceActorHandler.SpeakerCoroutine.AudioClip.clip.length;
				bottomLengthLabel[0] = audioHandler.VoiceActorHandler.SpeakerCoroutine.AudioSource.time.ToString("N2");
				bottomLengthLabel[2] = audioHandler.VoiceActorHandler.SpeakerCoroutine.AudioClip.clip.length.ToString("N2");
			}
			catch (NullReferenceException) { }
		}
		EditorGUI.ProgressBar(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 20), percentage, string.Join(" ", bottomLengthLabel));
		EditorGUI.LabelField(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 20), startingLine, EditorStyles.whiteMiniLabel);
	}
}