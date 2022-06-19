using System.Linq;
using UnityEngine;
using UnityEditor;

public class DebuggerWindow : EditorWindow
{
	private static ScriptParser _parser;
	public static bool TryGetScriptParser(out ScriptParser scriptParser)
	{
		if (_parser == null)
			_parser = FindObjectOfType<ScriptParser>();
		if (_parser == null)
		{
			scriptParser = null;
			return false;
		}
		scriptParser = _parser;
		return true;
	}

	private static DialogueSystem _charScript;
	public static bool TryGetDialogueSystem(out DialogueSystem dialogueSys)
	{
		if (_charScript == null)
			_charScript = FindObjectOfType<DialogueSystem>();
		if (_charScript == null)
		{
			dialogueSys = null;
			return false;
		}
		dialogueSys = _charScript;
		return true;
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

	

	int selected = 0;
	private void ShowTabs()
	{
		const int slotHeight = 20;
		int slotsEach = EditorPrefs.GetInt("B1NARY Slots Debugger", 3);
		string[] tabNames = DebuggerTab.Tabs.Select(tab => tab.Name).ToArray();
		int RectHeight = 0;
		for (int i = 0; i < tabNames.Length; i += slotsEach)
			RectHeight += slotHeight;
		Rect guiRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, RectHeight);
		guiRect.width -= 10;
		guiRect.x += 5;
		selected = GUI.SelectionGrid(guiRect, selected, tabNames, slotsEach);
		DebuggerTab.Tabs[selected].DisplayTab();
	}

	
	private void CurrentLineShow()
	{
		const string startingLine = "On line: ";
		if (TryGetScriptParser(out var scriptParser) && scriptParser.currentNode != null)
			EditorGUILayout.LabelField(startingLine + (scriptParser.currentNode.index + 1), EditorStyles.boldLabel);
		else
			EditorGUILayout.LabelField(startingLine + "NaN", EditorStyles.boldLabel);
	}
	private void SpeakerShow()
	{
		const string startingLine = "On Character: ";
		if (TryGetDialogueSystem(out var charScript) && charScript.currentSpeaker.Length != 0)
			EditorGUILayout.LabelField(startingLine + charScript.currentSpeaker);
		else
			EditorGUILayout.LabelField(startingLine + "NaN");
	}

	
	
}