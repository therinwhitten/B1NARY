#if UNITY_EDITOR
namespace B1NARY.Editor.Debugger
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;
	using B1NARY.UI;
	using System.Security.Cryptography;

	public sealed class OptionsTab : DebuggerTab
	{
		//public static void BoolTypeObject(string name, bool @default)
		//{
		//	deleteAllKeysNames.Add(name);
		//	bool setValue = EditorPrefs.GetBool(name, @default);
		//	bool result = EditorGUILayout.Toggle(name, setValue);
		//	if (result != setValue)
		//		EditorPrefs.SetBool(name, result);
		//}
		//public static void IntTypeObject(string name, int @default)
		//{
		//	deleteAllKeysNames.Add(name);
		//	int setValue = EditorPrefs.GetInt(name, @default);
		//	int result = EditorGUILayout.IntField(name, setValue);
		//	if (result != setValue)
		//		EditorPrefs.SetInt(name, result);
		//}
		//public static void FloatTypeObject(string name, float @default)
		//{
		//	deleteAllKeysNames.Add(name);
		//	float setValue = EditorPrefs.GetFloat(name, @default);
		//	float result = EditorGUILayout.FloatField(name, setValue);
		//	if (result != setValue)
		//		EditorPrefs.SetFloat(name, result);
		//}
		//public static void StringTypeObject(string name, string @default)
		//{
		//	deleteAllKeysNames.Add(name);
		//	string setValue = EditorPrefs.GetString(name, @default);
		//	string result = EditorGUILayout.TextField(name, setValue);
		//	if (result != setValue)
		//		EditorPrefs.SetString(name, result);
		//}
		//public static void StringPopupTypeObject(string name, string[] @params, int @default)
		//{
		//	deleteAllKeysNames.Add(name);
		//	int oldSelection = EditorPrefs.GetInt(name, @default);
		//	int result = EditorGUILayout.Popup(name, oldSelection, @params);
		//	if (result != oldSelection)
		//		EditorPrefs.SetInt(name, result);
		//}


		public override GUIContent Name => new GUIContent("Options");
		public override bool ConstantlyRepaint => false;

		private List<string> tabKeys;
		private HashSet<string> tabValues;
		private void TryAdd(string value)
		{
			if (tabValues.Contains(value))
				return;
			tabKeys.Add(value);
			tabValues.Add(value);
		}

		private bool showHideTabs = false;
		public override void DisplayTab()
		{
			if (tabKeys == null)
			{
				tabKeys = new List<string>(Preferences.Count + AllTabs.Count) { DebuggerWindow.slotsLengthKey };
				tabValues = new HashSet<string>() { DebuggerWindow.slotsLengthKey };
			}

			// Top bar slider thingy
			DebuggerWindow.SlotsLength = EditorGUILayout.IntSlider("B1NARY Slots", DebuggerWindow.SlotsLength, 1, 20);

			// Show Tabs
			showHideTabs = EditorGUILayout.Foldout(showHideTabs, "Hide Tabs");
			if (showHideTabs)
			{
				EditorGUI.indentLevel++;
				var enumeratorTabs = AllTabs.GetEnumerator();
				while (enumeratorTabs.MoveNext())
				{
					if (enumeratorTabs.Current.GetType() == typeof(OptionsTab))
						continue;
					string name = enumeratorTabs.Current.Name.text;
					TryAdd(name);
					bool setValue = EditorPrefs.GetBool(name, enumeratorTabs.Current.ShowInDebugger);
					bool result = EditorGUILayout.Toggle(enumeratorTabs.Current.Name, setValue);
					if (result != setValue)
						EditorPrefs.SetBool(name, result);
				}
				EditorGUI.indentLevel--;
			}

			// Debugger

		}
		//	var dataEnumerator = DebuggerPreferences.GetEnumerator();
		//	while (dataEnumerator.MoveNext())
		//	{
		//		
		//	}
		//	foreach (var (scriptName, dataObjects) in Data)
		//	{
		//		EditorGUILayout.Space(4);
		//		if (!showScriptSettings.ContainsKey(scriptName))
		//			showScriptSettings.Add(scriptName, false);
		//		showScriptSettings[scriptName] = EditorGUILayout.Foldout(showScriptSettings[scriptName], scriptName);
		//		if (!showScriptSettings[scriptName])
		//			continue;
		//		EditorGUI.indentLevel++;
		//		foreach (var data in dataObjects)
		//		{
		//			switch (data.Key)
		//			{
		//				case DebuggerPreferences.DataType.Bool:
		//					foreach (var (paramName, defaultValue) in data.Value)
		//						BoolTypeObject(paramName, (bool)defaultValue);
		//					break;
		//				case DebuggerPreferences.DataType.Int:
		//					foreach (var (paramName, defaultValue) in data.Value)
		//						IntTypeObject(paramName, (int)defaultValue);
		//					break;
		//				case DebuggerPreferences.DataType.Float:
		//					foreach (var (paramName, defaultValue) in data.Value)
		//						FloatTypeObject(paramName, (float)defaultValue);
		//					break;
		//				case DebuggerPreferences.DataType.String:
		//					foreach (var (paramName, defaultValue) in data.Value)
		//						StringTypeObject(paramName, (string)defaultValue);
		//					break;
		//				case DebuggerPreferences.DataType.StringPopup:
		//					foreach (var (paramName, @params) in data.Value)
		//						StringPopupTypeObject(paramName,
		//							((ValueTuple<string[], int>)@params).Item1,
		//							((ValueTuple<string[], int>)@params).Item2);
		//					break;
		//				default:
		//					throw new NotImplementedException
		//						($"{data.Key} is not in the switch statement!");
		//			}
		//		}
		//		EditorGUI.indentLevel--;
		//	}
		//
		//	EditorGUILayout.Space(20);
		//	bool pressedDeleteAllButton = GUILayout.Button("Clear Settings");
		//	if (pressedDeleteAllButton)
		//		foreach (string name in deleteAllKeysNames)
		//			EditorPrefs.DeleteKey(name);
		//
		//	
		//}
		// Order is at min value due to how ordering shown layers when changing it
		public override int Order => int.MinValue;
	}
}
#endif