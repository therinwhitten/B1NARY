namespace B1NARY.Editor
{
	using System.Linq;
	using UnityEngine;
	using UnityEditor;
	using B1NARY.UI;
	using B1NARY.DataPersistence;
	using System;
	using System.Collections.Generic;

	[CustomEditor(typeof(ColorFormat))]
	public class ColorFormatEditor : Editor
	{
		ColorFormat _colorFormat;
		private ColorFormat ColorFormat
		{
			get
			{
				if (_colorFormat == null)
				{
					_colorFormat = (ColorFormat)target;
				}
				return _colorFormat;
			}
		}
		private bool openedHeader = true;
		public override void OnInspectorGUI()
		{
			EditorUtility.SetDirty(ColorFormat);
			ColorFormat.primaryUI = DirtyAuto.Field(ColorFormat, new GUIContent("Primary Color"), ColorFormat.primaryUI);
			ColorFormat.SecondaryUI = DirtyAuto.Field(ColorFormat, new GUIContent("Secondary Color"), ColorFormat.SecondaryUI);
			if (openedHeader = EditorGUILayout.BeginFoldoutHeaderGroup(openedHeader, new GUIContent("Extra UI Color Data")))
			{
				EditorGUI.indentLevel++;
				var editable = new DictionaryEditor<string, Color>(ColorFormat.ExtraUIValues)
				{
					defaultValue = Color.black,
					defaultKey = ""
				};
				if (editable.Repaint())
				{
					ColorFormat.ExtraUIValues = editable.Flush();
					EditorUtility.SetDirty(ColorFormat);
				}
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}
	}
}