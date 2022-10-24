namespace B1NARY.Editor.Debugger
{
	using System;
	using System.Linq;
	using UnityEngine;
	using UnityEditor;
	using B1NARY.Audio;
	using B1NARY.UI;
	using B1NARY.Scripting;
	using B1NARY.DesignPatterns;
	using System.Collections.Generic;

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
			if (TryGetter<ScriptHandler>.TryGetObject(out var scriptHandler) && scriptHandler.IsActive)
				EditorGUILayout.LabelField(startingLine + scriptHandler.CurrentLine.Index, EditorStyles.boldLabel);
			else
				EditorGUILayout.LabelField(startingLine + "NaN", EditorStyles.boldLabel);
		}
		private void SpeakerShow()
		{
			const string emptySlot = "Empty";
			string startingLine = "On Character: " + emptySlot;
			string[] bottomLengthLabel = { "NaN", "/", "NaN" };
			CharacterScript currentCharacter = Multiton<CharacterScript>.AsEnumerable()
				.Where(@char => @char.name == DialogueSystem.Instance.CurrentSpeaker).Single();
			startingLine = startingLine.Replace(emptySlot, DialogueSystem.Instance.CurrentSpeaker);
			bottomLengthLabel[0] = currentCharacter.VoiceData.PlayedSeconds.TotalSeconds.ToString("N2");
			bottomLengthLabel[2] = currentCharacter.VoiceData.TotalSeconds.TotalSeconds.ToString("N2");
			EditorGUI.ProgressBar(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 20), currentCharacter.VoiceData.CompletionPercent(), string.Join(" ", bottomLengthLabel));
			EditorGUI.LabelField(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 20), startingLine, EditorStyles.whiteMiniLabel);
		}
	}
}