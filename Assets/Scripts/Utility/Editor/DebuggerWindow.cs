using UnityEngine;
using UnityEditor;


public class DebuggerWindow : EditorWindow
{
	[MenuItem("B1NARY/Debugger")]
	public static void ShowWindow()
	{
		// Get existing open window or if none, make a new one:
		DebuggerWindow window = (DebuggerWindow)GetWindow(typeof(DebuggerWindow));
		window.titleContent = new GUIContent("B1NARY Debugger");
		window.Show();
	}

	private void OnGUI()
	{
		EditorGUILayout.LabelField("GUI Built by AnOddDoorKnight, in beta", EditorStyles.miniLabel);
		LoadLines();

	}


	private ScriptParser parser;
	private void LoadLines()
	{
		const string startingLine = "On line: ";
		if (parser == null)
			parser = FindObjectOfType<ScriptParser>();
		if (parser == null || parser.currentNode == null)
			EditorGUILayout.LabelField(startingLine + "NaN", EditorStyles.boldLabel);
		else
			EditorGUILayout.LabelField(startingLine + parser.currentNode.index, EditorStyles.boldLabel);
	}
}
