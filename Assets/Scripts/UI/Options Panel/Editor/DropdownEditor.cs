namespace B1NARY.UI.Editor
{
	using B1NARY.Editor;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;

	public abstract class DropDownEditor<T> : Editor
	{
		private DropdownPanel<T> panel;
		public override void OnInspectorGUI()
		{
			panel = (DropdownPanel<T>)target;
			for (int i = 0; i < panel.Values.Count; i++)
			{
				Rect fullRect = GUILayoutUtility.GetRect(Screen.width, 20f);
				EditorGUI.LabelField(new Rect(fullRect) { width = (fullRect.width / 2f) - 1f }, panel.Visuals[i]);
				EditorGUI.LabelField(new Rect(fullRect) { xMin = (fullRect.width / 2f) + 1f }, panel.Values[i].ToString());
			}
		}
	}
}