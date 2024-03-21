#if UNITY_EDITOR
namespace B1NARY.Editor
{
	using UnityEditor;
	using UnityEngine;

	public class ReferencerWindow : EditorWindow
	{
		public static Texture texture;
		[MenuItem("B1NARY/Referencer" /*,priority = 0*/)]
		public static void ShowWindow()
		{
			// Get existing open window or if none, make a new one:
			var window = (ReferencerWindow)GetWindow(typeof(ReferencerWindow));
			window.titleContent = new GUIContent("B1NARY Referencer");
		}

		private SerializedObject SerializedThis;
		public GameObject GameObject;
		public Texture Texture;
		private SerializedProperty GameObjectProperty;
		private SerializedProperty textureProperty;

		private void OnEnable()
		{
			SerializedThis = new SerializedObject(this);
			GameObjectProperty = SerializedThis.FindProperty(nameof(GameObject));
			textureProperty = SerializedThis.FindProperty(nameof(Texture));
		}

		private void OnGUI()
		{
			EditorGUILayout.PropertyField(GameObjectProperty);
			EditorGUILayout.PropertyField(textureProperty);
			SerializedThis.ApplyModifiedProperties();
			texture = Texture;
		}

	}
}
#endif