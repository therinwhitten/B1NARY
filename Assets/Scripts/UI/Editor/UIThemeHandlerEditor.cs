namespace B1NARY.Editor
{
	using System;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(UIThemeHandler))]
	public class UIThemeHandlerEditor : Editor
	{
		UIThemeHandler currentHandler;
		private void Awake() => currentHandler = (UIThemeHandler)target;

		public override void OnInspectorGUI()
		{
			try
			{
				EditorGUILayout.LabelField($"Current Configuration: {currentHandler.CurrentTarget}", EditorStyles.boldLabel);
			}
			catch (MissingComponentException ex)
			{
				EditorGUILayout.HelpBox(ex.Message, MessageType.Error);
				throw;
			}
			EditorUtility.SetDirty(currentHandler);
			UIThemeHandler.Option[] allOptions = (UIThemeHandler.Option[])Enum.GetValues(typeof(UIThemeHandler.Option));
			string[] optionStrings = allOptions.Select(option => option.ToString()).ToArray();
			bool hasChanges = false;
			switch (currentHandler.CurrentTarget)
			{
				case UIThemeHandler.Target.Image:
					hasChanges = PopupOrCustom("Color Option", "Custom Color Name", ref currentHandler.imageThemeName);
					break;
				case UIThemeHandler.Target.Button:
					hasChanges = PopupOrCustom("Normal Button Option", "Normal Button Name", ref currentHandler.imageThemeName)
					 | PopupOrCustom("Highlighted Button Option", "Hilighted Button Name", ref currentHandler.buttonHighlightedName)
					 | PopupOrCustom("Pressed Button Option", "Pressed Button Name", ref currentHandler.buttonPressedName)
					 | PopupOrCustom("Selected Button Option", "Selected Button Name", ref currentHandler.buttonPressedName)
					 | PopupOrCustom("Disabled Button Option", "Disabled Button Name", ref currentHandler.buttonDisabledName);
					break;
				default:
					throw new IndexOutOfRangeException(currentHandler.CurrentTarget.ToString());
			}
			if (hasChanges)
				currentHandler.UpdateColors(currentHandler.CurrentTarget);
			//EditorUtility.ClearDirty(currentHandler);
			bool PopupOrCustom(string popupLabel, string textFieldLabel, ref string current)
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
					current = EditorGUILayout.TextField(textFieldLabel, current);
				return current != old;
			}
		}
	}
}