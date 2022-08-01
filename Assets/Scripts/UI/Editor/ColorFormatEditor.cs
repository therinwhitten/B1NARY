namespace B1NARY.Editor
{
	using System.Linq;
	using UnityEngine;
	using UnityEditor;
	using B1NARY.UI;

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
		private void Awake()
		{
			EditorUtility.SetDirty(ColorFormat);
		}
		private bool openedHeader = false;
		public override void OnInspectorGUI()
		{
			ColorFormat.primaryUI = EditorGUILayout.ColorField("Primary Color", ColorFormat.primaryUI);
			ColorFormat.SecondaryUI = EditorGUILayout.ColorField("Secondary Color", ColorFormat.SecondaryUI);
			if (openedHeader = EditorGUILayout.BeginFoldoutHeaderGroup(openedHeader, new GUIContent("Extra UI Color Data")))
			{
				EditorGUI.indentLevel++;
				(string @new, string old)? setNewDictionaryValue = null;
				string remove = string.Empty;
				(Color color, string key)? modifyColor = null;
				foreach (var pair in ColorFormat.Pairs)
				{
					Rect fullRect = GUILayoutUtility.GetRect(Screen.width, 20),
						colorRect = fullRect, removeRect = fullRect;
					colorRect.width /= 1.2f;
					colorRect.xMin = colorRect.width / 2;
					removeRect.xMin = colorRect.xMax + 2;
					removeRect.xMax = fullRect.xMax;
					Color newColor = EditorGUI.ColorField(colorRect, pair.Value);
					if (newColor != pair.Value)
						modifyColor = (newColor, pair.Key);
					Rect stringRect = GUILayoutUtility.GetLastRect();
					stringRect.xMax = colorRect.xMin + 12;
					string newString = EditorGUI.DelayedTextField(stringRect, pair.Key);
					if (newString != pair.Key)
						setNewDictionaryValue = (newString, pair.Key);
					if (GUI.Button(removeRect, "Remove"))
						remove = pair.Key;
				}
				if (setNewDictionaryValue.HasValue)
					ColorFormat.InsertKey(setNewDictionaryValue.Value.old, setNewDictionaryValue.Value.@new);
				if (modifyColor.HasValue)
					ColorFormat.Modify(modifyColor.Value.key, modifyColor.Value.color);
				if (!string.IsNullOrEmpty(remove))
					ColorFormat.Remove(remove);
				if (GUI.Button(EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 20)), "Add New Color Entry"))
					ColorFormat.Append("New Color Entry", Color.white);
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
			EditorUtility.SetDirty(ColorFormat);
			serializedObject.ApplyModifiedProperties(); // Just in case
		}
	}
}