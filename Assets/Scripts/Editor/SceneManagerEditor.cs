namespace B1NARY.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using UnityEditor.UI;
	using System;

	[CustomEditor(typeof(SceneManager))]
	public class SceneManagerEditor : Editor
	{
		private SceneManager sceneManager;
		private void Awake() => sceneManager = (SceneManager)target;
		
		public override void OnInspectorGUI()
		{
			EditorGUILayout.LabelField($"Active Scene: {SceneManager.ActiveScene.name}, Index: {SceneManager.ActiveScene.buildIndex}");
			EditorGUILayout.LabelField($"Currently Switching Scenes: {sceneManager.IsSwitchingScenes}");
			EditorGUILayout.Separator();
			DisplayTab("Before Switching Scenes", sceneManager.SwitchingScenes);
			DisplayTab("After Switching Scenes", sceneManager.SwitchedScenes);
		}

		/// <summary>
		/// Displays a tab's persistent and non-persistent listeners.
		/// </summary>
		/// <param name="header"></param>
		/// <param name="group"></param>
		/// <returns> The taken space in the rect. </returns>
		private static void DisplayTab(string header, ListenerGroup group)
		{
			Rect masterHeaderRect = GUILayoutUtility.GetRect(Screen.width / 4 * 3, 22f);
			masterHeaderRect = EditorGUI.IndentedRect(masterHeaderRect);
			EditorGUI.LabelField(masterHeaderRect, header, EditorStyles.largeLabel);
			EditorGUI.indentLevel++;
			DisplaySubCategory("Persistent Listeners", group.PersistentListeners);
			DisplaySubCategory("Non-Persistent Listeners", group.NonPersistentListeners);
			EditorGUI.indentLevel--;
			void DisplaySubCategory(string subHeader, IReadOnlyList<Action> delegates)
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