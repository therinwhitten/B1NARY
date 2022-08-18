namespace B1NARY.Editor
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using B1NARY.UI.Globalization;
	using static GameCommands;
	using System.Linq;

	public class GameOptionsWindow : EditorWindow
	{
		private static GameOptionsWindow instance;
		[MenuItem("B1NARY/Game Options", priority = 2)]
		public static void ShowWindow()
		{
			instance = GetWindow<GameOptionsWindow>();
			instance.titleContent = new GUIContent("B1NARY Game Options");
			instance.Show();
		}

		private void OnGUI()
		{
			if (GUILayout.Button("Delete all keys"))
				GamePreferences.DeleteAllKeys();
			Print(("Frame Limiter", FrameLimitBlock),
				("Exception Handling Settings", ExceptionChoiceBlock),
				("Languages", LanguagesBlock));
		}
		private void Print(params (string name, Action tab)[] tabs)
		{
			for (int i = 0; i < tabs.Length; i++)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField(tabs[i].name, EditorStyles.boldLabel);
				tabs[i].tab.Invoke();
			}
		}

		private void FrameLimitBlock()
		{
			int frameLimitTypeOut = GUILayout.SelectionGrid(FrameLimiter.VSyncCount, FrameLimiter.frameLimitSettings, FrameLimiter.frameLimitSettings.Length);
			if (frameLimitTypeOut != FrameLimiter.VSyncCount)
			{
				FrameLimiter.VSyncCount = frameLimitTypeOut;
				FrameLimiter.ApplySettings();
			}
			if (frameLimitTypeOut == 0)
			{
				int frameLimitTargetOut = EditorGUILayout.IntSlider("FPS Limit", FrameLimiter.Target, 1, 300);
				// Setting a value every time the GUI is loaded is expensive.
				if (frameLimitTargetOut != FrameLimiter.Target)
					FrameLimiter.Target = frameLimitTargetOut;
			}
		}
		private void ExceptionChoiceBlock()
		{
			EditorGUILayout.LabelField("This turns some options that can be treated as warnings into errors: ");
			bool exceptionType = GUILayout.SelectionGrid(GamePreferences.GetBool(exceptionLoadName, true) ? 0 : 1, new string[] { "Exception", "Warning" }, 2) == 0;
			if (GamePreferences.GetBool(exceptionLoadName, true) != exceptionType)
				GamePreferences.SetBool(exceptionLoadName, exceptionType);
		}

		string inputLanguages = string.Empty;
		private void LanguagesBlock()
		{
			inputLanguages = EditorGUILayout.TextField("New Language", inputLanguages);
			if (GUILayout.Button("Add New Language"))
				FontLanguageChanger.AddLanguage(inputLanguages);
			EditorGUILayout.Space();
			// Display Languages
			for (int i = 0; i < FontLanguageChanger.Languages.Count; i++)
			{
				Rect fullRect = GUILayoutUtility.GetRect(Screen.width, 20);
				fullRect.xMin += 2;
				fullRect.xMax -= 2;
				Rect labelRect = new Rect(fullRect) { width = fullRect.width / 3 * 2, xMin = fullRect.xMin + 16 },
					buttonRect = new Rect(fullRect) { xMin = labelRect.xMax + 2 },
					toggleRect = new Rect(fullRect) { xMax = labelRect.xMin };
				EditorGUI.LabelField(labelRect, 
					new GUIContent(FontLanguageChanger.Languages[i].First().ToString().ToUpper() + FontLanguageChanger.Languages[i].Substring(1), 
					$"The capitalization is purely cosmetic, it's actual value is '{FontLanguageChanger.Languages[i]}'"));
				bool toggle = EditorGUI.Toggle(toggleRect, i == FontLanguageChanger.LanguageIndex);
				if (toggle && i != FontLanguageChanger.LanguageIndex)
					FontLanguageChanger.LanguageIndex = i;
				if (GUI.Button(buttonRect, new GUIContent("Remove", $"Remove the language '{FontLanguageChanger.Languages[i]}' from the database.")))
				{
					FontLanguageChanger.RemoveLanguage(FontLanguageChanger.Languages[i]);
					i--;
				}
			}
		}
	}
}