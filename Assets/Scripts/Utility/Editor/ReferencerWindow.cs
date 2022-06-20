using UnityEditor;
using UnityEngine;

public class ReferencerWindow : EditorWindow
{
	[MenuItem("B1NARY/Referencer" /*,priority = 0*/)]
	public static void ShowWindow()
	{
		// Get existing open window or if none, make a new one:
		var window = (ReferencerWindow)GetWindow(typeof(ReferencerWindow));
		window.titleContent = new GUIContent("B1NARY Referencer");
	}

	private SerializedObject SerializedThis;
	public GameObject GameObject;
	private SerializedProperty GameObjectProperty;

	private void OnEnable()
	{
		SerializedThis = new SerializedObject(this);
		GameObjectProperty = SerializedThis.FindProperty(nameof(GameObject));
	}

	private void OnGUI()
	{
		EditorGUILayout.PropertyField(GameObjectProperty);
		SerializedThis.ApplyModifiedProperties();
	}

}