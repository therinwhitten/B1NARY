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
			switch (currentHandler.CurrentTarget)
			{
				case UIThemeHandler.Target.Image:
					currentHandler.imageThemeName = PopupOrCustom("Color Option", "Custom Color Name", currentHandler.imageThemeName);
					break;
				case UIThemeHandler.Target.Button:
					currentHandler.buttonNormalName = PopupOrCustom("Normal Button Option", "Normal Button Name", currentHandler.buttonNormalName);
					currentHandler.buttonPressedName = PopupOrCustom("Pressed Button Option", "Pressed Button Name", currentHandler.buttonPressedName);
					break;
				default:
					throw new IndexOutOfRangeException(currentHandler.CurrentTarget.ToString());
			}
			EditorUtility.ClearDirty(currentHandler);
			string PopupOrCustom(string popupLabel, string textFieldLabel, string current)
			{
				// Popup box for defaults
				int currentIndex = Array.IndexOf(optionStrings, current);
				if (currentIndex == -1)
					currentIndex = Array.IndexOf(allOptions, UIThemeHandler.Option.Custom);
				int newIndex = EditorGUILayout.Popup(popupLabel, currentIndex, optionStrings);
				if (newIndex != currentIndex)
					current = optionStrings[newIndex];
				if (Array.IndexOf(allOptions, UIThemeHandler.Option.Custom) == currentIndex)
					current = EditorGUILayout.TextField(textFieldLabel, current);
				return current;
			}
		}
	}
}