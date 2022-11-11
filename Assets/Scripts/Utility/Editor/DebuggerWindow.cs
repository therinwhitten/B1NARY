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
	using System.Text;

	public sealed class DebuggerWindow : EditorWindow
	{
		private const string @null = "Null", notPlaying = "Not Playing!";

		public static int SlotsLength 
		{ 
			get => EditorPrefs.GetInt(slotsLengthKey, 3); 
			set => EditorPrefs.SetInt(slotsLengthKey, value); 
		}
		public const string slotsLengthKey = "B1NARY Slots Debugger";

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



		private DebuggerTab CurrentTab
		{
			get
			{
				if (m_currentTab < 0)
					return null;
				return DebuggerTab.AllTabs[m_currentTab];
			}
		}
		private int m_currentTab = 0;


		Vector2 scrollPos = Vector2.zero;
		private void OnGUI()
		{
			EditorGUILayout.LabelField(CurrentSpeaker());
			EditorGUILayout.LabelField(CurrentLine());
			DisplayTabs();


			string CurrentSpeaker()
			{
				var onSpeaker = new StringBuilder("On Speaker: ");
				if (!Application.isPlaying)
					return onSpeaker.Append(notPlaying).ToString();
				if (!DialogueSystem.HasInstance || string.IsNullOrEmpty(DialogueSystem.Instance.CurrentSpeaker))
					return onSpeaker.Append(@null).ToString();
				return onSpeaker.Append(DialogueSystem.Instance.CurrentSpeaker).ToString();
			}
			string CurrentLine()
			{
				var onLine = new StringBuilder("On Line: ");
				if (!Application.isPlaying)
					return onLine.Append(notPlaying).ToString();
				if (!ScriptHandler.HasInstance || ScriptHandler.Instance.CurrentLine.Equals(default))
					return onLine.Append(@null).ToString();
				return onLine.Append(ScriptHandler.Instance.CurrentLine).ToString();
			}
			void DisplayTabs()
			{
				const int slotHeight = 20;
				
				int tabHeight = (int)Math.Ceiling((double)DebuggerTab.AllTabs.Count / SlotsLength);
				tabHeight *= slotHeight;
				GUIContent[] content = DebuggerTab.AllTabs.Select(tab => tab.Name).ToArray();
				Rect rect = GUILayoutUtility.GetRect(Screen.width, tabHeight);
				rect.xMax -= 2;
				rect.xMin += 2;
				m_currentTab = GUI.SelectionGrid(rect, m_currentTab, content, SlotsLength);

				if (CurrentTab == null)
					return;
				scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
				CurrentTab.DisplayTab();
				EditorGUILayout.EndScrollView();
			}
		}

		private void OnInspectorUpdate()
		{
			if (CurrentTab.ConstantlyRepaint)
				Repaint();
		}
	}
}