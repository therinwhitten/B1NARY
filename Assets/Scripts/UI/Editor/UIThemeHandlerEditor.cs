namespace B1NARY.Editor
{
	using B1NARY.UI;
	using System;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Reflection;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.UI;
	using Object = UnityEngine.Object;

	[CustomEditor(typeof(UIThemeHandler))]
	public class UIThemeHandlerEditor : Editor
	{
		internal static UIThemeHandler.Option[] options = (UIThemeHandler.Option[])Enum.GetValues(typeof(UIThemeHandler.Option));
		internal static string[] optionNames = Enum.GetNames(typeof(UIThemeHandler.Option));



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
				//EditorGUILayout.ObjectField(new GUIContent("Color Format [readonly]", "The currently used format across all Theme Handlers"), , typeof(ColorFormat), false);
				EditorGUILayout.Space();
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
			bool hasChanges = false;
			if (currentHandler.ColorEdit.Value is Color)
				hasChanges = ModifyColor("Color Option", new Ref<string>(() => currentHandler.imageThemeName, str => currentHandler.imageThemeName = str));
			else if (currentHandler.ColorEdit.Value is ColorBlock)
				hasChanges = ModifyColor("Normal Color", new Ref<string>(() => currentHandler.imageThemeName, str => currentHandler.imageThemeName = str))
				 | ModifyColor("Highlighted Color", new Ref<string>(() => currentHandler.buttonHighlightedName, str => currentHandler.buttonHighlightedName = str))
				 | ModifyColor("Pressed Color", new Ref<string>(() => currentHandler.buttonPressedName, str => currentHandler.buttonPressedName = str))
				 | ModifyColor("Selected Color", new Ref<string>(() => currentHandler.buttonSelectedName, str => currentHandler.buttonSelectedName = str))
				 | ModifyColor("Disabled Color", new Ref<string>(() => currentHandler.buttonDisabledName, str => currentHandler.buttonDisabledName = str));
			else
				throw new IndexOutOfRangeException(currentHandler.CurrentTarget.ToString());
			if (hasChanges)
			{
				if (Application.isPlaying)
					currentHandler.UpdateColors();
				else
					EditorUtility.SetDirty(currentHandler);
			}
		}

		public bool ModifyColor(string label, Ref<string> colorName)
		{
			string old = colorName;
			// Popup box for defaults
			int currentIndex = Array.IndexOf(optionNames, colorName);
			if (currentIndex == -1)
				currentIndex = Array.IndexOf(options, UIThemeHandler.Option.Custom);
			int newIndex = EditorGUILayout.Popup(label, currentIndex, optionNames);
			if (newIndex != currentIndex)
				colorName.Value = optionNames[newIndex];
			if (Array.IndexOf(options, UIThemeHandler.Option.Custom) == currentIndex)
			{
				CustomMenu(colorName);
			}
			return colorName != old;
		}

		public void CustomMenu(Ref<string> newValueAction)
		{
			Rect fullRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 20f)),
						popupRect = new Rect(fullRect) { width = 20f };
			fullRect.xMin += 22f;
			newValueAction.Value = EditorGUI.TextField(fullRect, newValueAction);

			if (!GUI.Button(popupRect, "", EditorStyles.foldout))
				return;
			// Context Menu
			//var menu = new GenericMenu();
			////menu.AddDisabledItem(new GUIContent($"Current Selection: {ColorFormat..name}"));
			//menu.AddSeparator("");
			//for (int i = 0; i < UIThemeHandler.CurrentlyEquippedFormat.SavedPairs.Count; i++)
			//{
			//	int currentIterative = i;
			//	string capturedKey = UIThemeHandler.CurrentlyEquippedFormat.SavedPairs[i].Key;
			//	menu.AddItem(new GUIContent(capturedKey), newValueAction.Value == capturedKey, () =>
			//	{
			//		newValueAction.Value = capturedKey;
			//		EditorUtility.SetDirty(target);
			//	});
			//}
			//menu.ShowAsContext();
		}
	}
}