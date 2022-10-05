namespace B1NARY.Editor
{
	using System;
	using UnityEngine;
	using UnityEditor;
	using B1NARY.UI;

	[CustomEditor(typeof(QuitButton))]
	public class QuitButtonEditor : Editor
	{
		private QuitButton quitButton;
		private void Awake()
		{
			quitButton = (QuitButton)target;
		}

		public override void OnInspectorGUI()
		{
			bool hasChanges = false;
			bool newQuit = EditorGUILayout.Toggle(new GUIContent("Ask Before Committing", "If it should use a small menu to conform the action"), quitButton.askBeforeCommitting);
			if (newQuit != quitButton.askBeforeCommitting)
			{
				quitButton.askBeforeCommitting = newQuit;
				hasChanges = true;
			}

			if (quitButton.askBeforeCommitting)
			{
				serializedObject.Update();
				EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(QuitButton.askPanel)));
				if (serializedObject.hasModifiedProperties)
				{
					GameObject panelObject = (GameObject)serializedObject.FindProperty(nameof(QuitButton.askPanel)).objectReferenceValue;
					if (!QuitButton.VerifyGameObject(panelObject, out _))
						throw new InvalidCastException($"Panel does not contain a child game object named {QuitButton.quitButtonKey} that contains a button!");
					else
						serializedObject.ApplyModifiedProperties();
				}
			}

			newQuit = EditorGUILayout.Toggle(new GUIContent("Quit To Scene", "If the object should quit to another scene, otherwise just quits the game."), quitButton.quitToScene);
			if (newQuit != quitButton.quitToScene)
			{
				quitButton.quitToScene = newQuit;
				hasChanges = true;
			}
			if (quitButton.quitToScene)
			{
				string newString = EditorGUILayout.TextField(new GUIContent("Scene Name", "The scene to switch to."), quitButton.sceneName);
				if (newString != quitButton.sceneName)
				{
					quitButton.sceneName = newString;
					hasChanges = true;
				}
			}
			if (hasChanges)
				EditorUtility.SetDirty(quitButton);
		}
	}
}