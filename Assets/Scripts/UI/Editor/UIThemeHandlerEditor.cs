namespace B1NARY.Editor
{
	using ICSharpCode.NRefactory.Ast;
	using System;
	using System.Linq;
	using System.Reflection;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.UI;

	[CustomEditor(typeof(UIThemeHandler))]
	public class UIThemeHandlerEditor : Editor
	{
		UIThemeHandler currentHandler;
		private void Awake() => currentHandler = (UIThemeHandler)target;

		public override void OnInspectorGUI()
		{
			if (!Application.isPlaying)
			{
				int count = 0;
				Transform transform = currentHandler.transform;
				for (int i = 0; i < transform.childCount; i++)
				{
					if (transform.GetChild(i).GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
						.Any(info => info.PropertyType == typeof(Color) || info.PropertyType == typeof(ColorBlock)))
						count++;
				}
				if (count > 1)
					EditorGUILayout.HelpBox("There is more than one " +
						"color-related blocks detected in the components, " +
						"make sure that the one you want to modify is at the top!", 
						MessageType.Info);
			}
			try
			{
				string componentName = currentHandler.CurrentTarget.GetType().ToString();
				EditorGUILayout.LabelField($"Current Configuration: {componentName.Substring(componentName.LastIndexOf('.') + 1).Replace('_', ' ')}", EditorStyles.boldLabel);
			}
			catch (MissingComponentException ex)
			{
				EditorGUILayout.HelpBox(ex.Message, MessageType.Error);
				throw;
			}
			UIThemeHandler.Option[] allOptions = (UIThemeHandler.Option[])Enum.GetValues(typeof(UIThemeHandler.Option));
			string[] optionStrings = allOptions.Select(option => option.ToString()).ToArray();
			bool hasChanges = false;
			if (currentHandler.ColorEdit.Value is Color)
				hasChanges = PopupOrCustom("Color Option", ref currentHandler.imageThemeName);
			else if (currentHandler.ColorEdit.Value is ColorBlock)
				hasChanges = PopupOrCustom("Normal Color", ref currentHandler.imageThemeName)
				 | PopupOrCustom("Highlighted Color", ref currentHandler.buttonHighlightedName)
				 | PopupOrCustom("Pressed Color", ref currentHandler.buttonPressedName)
				 | PopupOrCustom("Selected Color", ref currentHandler.buttonSelectedName)
				 | PopupOrCustom("Disabled Color", ref currentHandler.buttonDisabledName);
			else
				throw new IndexOutOfRangeException(currentHandler.CurrentTarget.ToString());
			if (hasChanges)
			{
				if (Application.isPlaying)
					currentHandler.UpdateColors();
				else
					EditorUtility.SetDirty(currentHandler);
			}

			bool PopupOrCustom(string popupLabel, ref string current)
			{
				string old = current;
				// Popup box for defaults
				int currentIndex = Array.IndexOf(optionStrings, current);
				if (currentIndex == -1)
					currentIndex = Array.IndexOf(allOptions, UIThemeHandler.Option.Custom);
				int newIndex = EditorGUILayout.Popup(popupLabel, currentIndex, optionStrings);
				if (newIndex != currentIndex)
					current = optionStrings[newIndex];
				if (Array.IndexOf(allOptions, UIThemeHandler.Option.Custom) == currentIndex)
					current = EditorGUILayout.TextField(current);
				return current != old;
			}
		}
	}
}