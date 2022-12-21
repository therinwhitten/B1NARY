namespace B1NARY.DataPersistence.Editor
{
	using B1NARY.Scripting;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(DataCommands), true)]
	public class DataCommandsEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DataCommands.input)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DataCommands.loadButton)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DataCommands.saveButton)));
			serializedObject.ApplyModifiedProperties();
			EditorGUILayout.Separator();
			if (!Application.isPlaying)
			{
				EditorGUILayout.HelpBox("Enter play mode to inspect data values!", MessageType.Error);
				return;
			}
			UpdateTab("Strings", PersistentData.Strings.AsEnumerable());
			UpdateTab("Integers", PersistentData.Integers.AsEnumerable());
			UpdateTab("Booleans", PersistentData.Booleans.AsEnumerable());
			UpdateTab("Singles", PersistentData.Singles.AsEnumerable());
		
			void UpdateTab<TKey, TValue>(string label, IEnumerable<KeyValuePair<TKey, TValue>> data)
			{
				EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
				EditorGUI.indentLevel++;
				using (var enumerator = data.GetEnumerator())
					while (enumerator.MoveNext())
					{
						Rect fullRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 20f));
						EditorGUI.LabelField(new Rect(fullRect) { width = (fullRect.width / 2f) - 1f }, enumerator.Current.Key.ToString());
						EditorGUI.LabelField(new Rect(fullRect) { xMin = (fullRect.width / 2f) + 1f }, enumerator.Current.Value.ToString());
					}
				EditorGUI.indentLevel--;
			}
		}
	}
}