namespace B1NARY.DataPersistence.Editor
{
	using B1NARY.Scripting;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(PersistentData), true)]
	public class PersistentDataEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var target = (PersistentData)this.target;
			if (Application.isPlaying || !ScriptHandler.HasInstance || !ScriptHandler.Instance.IsActive)
			{
				EditorGUILayout.HelpBox("no\nError code 420", MessageType.Error);
				return;
			}
			UpdateTab("Strings", target.Strings.AsEnumerable());
			UpdateTab("Integers", target.Integers.AsEnumerable());
			UpdateTab("Booleans", target.Booleans.AsEnumerable());
			UpdateTab("Singles", target.Singles.AsEnumerable());

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