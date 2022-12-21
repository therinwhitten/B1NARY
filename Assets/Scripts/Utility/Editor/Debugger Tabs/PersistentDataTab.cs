﻿namespace B1NARY.Editor.Debugger
{
	using System;
	using System.Reflection;
	using System.Collections.Generic;
	using System.Linq;
	using B1NARY.DataPersistence;
	using UnityEditor;
	using UnityEngine;
	public sealed class PersistentDataTab : DebuggerTab
	{
		public override GUIContent Name => new GUIContent("Persistent Data");
		public override bool ConstantlyRepaint => true;

		private Vector2 scroll = Vector2.zero;
		public override void DisplayTab()
		{
			if (!Application.isPlaying)
			{
				EditorGUILayout.HelpBox("Cannot show data when it is not in " +
					"play mode.", MessageType.Info);
				return;
			}
			scroll = EditorGUILayout.BeginScrollView(scroll);
			DisplayBlock("Booleans", PersistentData.Booleans);
			DisplayBlock("Integers", PersistentData.Integers);
			DisplayBlock("Singles/Single", PersistentData.Singles);
			DisplayBlock("Strings", PersistentData.Strings);
			EditorGUILayout.EndScrollView();
		}

		private static void DisplayBlock<TValue>(string label, IEnumerable<KeyValuePair<string, TValue>> pairs)
		{
			Rect largeLabel = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 22f));
			EditorGUI.LabelField(largeLabel, label, EditorStyles.largeLabel);
			EditorGUI.indentLevel++;
			using (var enumerator = pairs.GetEnumerator())
				while (enumerator.MoveNext())
					DisplayKeyValuePair(enumerator.Current.Key, enumerator.Current.Value);
			EditorGUI.indentLevel--;
		}
		private static void DisplayKeyValuePair(string key, object value)
		{
			Rect fullRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 15f)),
				keyRect = new Rect(fullRect) { width = fullRect.width / 2 },
				valueRect = new Rect(fullRect) { xMin = keyRect.xMax + 2 };
			EditorGUI.LabelField(keyRect, key, EditorStyles.boldLabel);
			EditorGUI.LabelField(valueRect, value.ToString());
		}
	}
}