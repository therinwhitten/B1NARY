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
		public override void OnInspectorGUI()
		{
			bool changedValue = false;
			if (ColorUtility.TryParseHtmlString(ColorFormat.primary, out var primaryColor))
			{
				Color newColor = EditorGUILayout.ColorField("Primary Color", primaryColor);
				if (changedValue |= newColor != primaryColor)
					ColorFormat.primary = ColorUtility.ToHtmlStringRGBA(newColor);
			}
			else
				ColorFormat.primary = ColorUtility.ToHtmlStringRGBA(Color.white);
			if (ColorUtility.TryParseHtmlString(ColorFormat.secondary, out var secondaryColor))
			{
				Color newColor = EditorGUILayout.ColorField("Secondary Color", secondaryColor);
				if (changedValue |= newColor != secondaryColor)
					ColorFormat.primary = ColorUtility.ToHtmlStringRGBA(newColor);
			}
			else
				ColorFormat.primary = ColorUtility.ToHtmlStringRGBA(Color.white);
			EditorGUILayout.Space();
			for (int i = 0; i < ColorFormat.colorHex.Count; i++)
			{
				Rect fullRect = GUILayoutUtility.GetRect(Screen.width, 20),
					colorRect = fullRect, 
					removeRect = fullRect;
				colorRect.width /= 1.2f;
				colorRect.xMin = colorRect.width / 2;
				removeRect.xMin = colorRect.xMax + 2;
				removeRect.xMax = fullRect.xMax;
				Rect stringRect = GUILayoutUtility.GetLastRect();
				stringRect.xMax = colorRect.xMin + 12;


				if (changedValue |= GUI.Button(removeRect, "Remove"))
				{
					ColorFormat.Remove(ColorFormat.keys[i]);
					i--;
				}

				ColorUtility.TryParseHtmlString(ColorFormat.colorHex[i], out var currentColor);
				Color newColor = EditorGUI.ColorField(colorRect, currentColor);
				if (changedValue |= newColor != currentColor)
					ColorFormat.colorHex[i] = ColorUtility.ToHtmlStringRGBA(newColor);

				string newName = EditorGUI.DelayedTextField(stringRect, ColorFormat.keys[i]);
				if (changedValue |= newName != ColorFormat.keys[i])
					ColorFormat.keys[i] = newName;
			}
			if (changedValue |= GUI.Button(EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 20)), "Add New Color Entry"))
				ColorFormat.Append("New Color Entry", Color.white);
			if (changedValue)
				EditorUtility.SetDirty(ColorFormat);
		}
	}
}