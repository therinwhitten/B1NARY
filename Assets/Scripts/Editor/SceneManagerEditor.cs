namespace B1NARY.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;

	[CustomEditor(typeof(SceneManager))]
	public class SceneManagerEditor : Editor
	{
		private SceneManager sceneManager;
		private void Awake() => sceneManager = (SceneManager)target;

		private Vector2 scroll = Vector2.zero;
		public override void OnInspectorGUI()
		{
			EditorGUILayout.LabelField($"Active Scene: {SceneManager.ActiveScene.name}, Index: {SceneManager.ActiveScene.buildIndex}");
			EditorGUILayout.LabelField($"Currently Switching Scenes: {sceneManager.IsSwitchingScenes}");
			EditorGUILayout.Separator();
			scroll = EditorGUILayout.BeginScrollView(scroll);
			DisplayTab("Before Switching Scenes", sceneManager.SwitchingScenes);
			DisplayTab("After Switching Scenes", sceneManager.SwitchedScenes);
			EditorGUILayout.EndScrollView();
		}

		private static void DisplayTab(string header, PersistentListenerGroup group)
		{
			Rect masterHeaderRect = GUILayoutUtility.GetRect(Screen.width / 4 * 3, 22f);
			masterHeaderRect = EditorGUI.IndentedRect(masterHeaderRect);
			EditorGUI.LabelField(masterHeaderRect, header, EditorStyles.largeLabel);
			EditorGUI.indentLevel++;
			DisplaySubCategory("Persistent Listeners", group.PersistentListeners);
			DisplaySubCategory("Non-Persistent Listeners", group.NonPersistentListeners);
			EditorGUI.indentLevel--;
			void DisplaySubCategory(string subHeader, IReadOnlyList<SwitchScenesDelegate> delegates)
			{
				EditorGUILayout.LabelField(subHeader, EditorStyles.boldLabel);
				EditorGUI.indentLevel++;
				for (int i = 0; i < delegates.Count; i++)
					EditorGUILayout.LabelField($"{i + 1}. '{delegates[i].Method.Name}' from class '{delegates[i].Method.DeclaringType}'");
				EditorGUI.indentLevel--;
			}
		}
	}
}