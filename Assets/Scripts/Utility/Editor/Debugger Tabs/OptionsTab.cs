using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class OptionsTab : DebuggerTab
{
	public override string Name => "Options";
	private bool showHideScripts = true;
	private Dictionary<string, bool> showScriptSettings = new Dictionary<string, bool>();
	public override void DisplayTab()
	{
		var deleteAllKeysNames = new List<string>() { "B1NARY Slots Debugger" };

		// Slider Thingy
		int oldBarCountValue = EditorPrefs.GetInt("B1NARY Slots Debugger", 3);
		int newBarCountValue = EditorGUILayout.IntSlider("B1NARY Slots", oldBarCountValue, 1, 20);
		if (oldBarCountValue != newBarCountValue)
			EditorPrefs.SetInt("B1NARY Slots Debugger", newBarCountValue);
		// Show Tabs
		showHideScripts = EditorGUILayout.Foldout(showHideScripts, "Hide Tabs");
		if (showHideScripts)
		{
			EditorGUI.indentLevel++;
			foreach (DebuggerTab tab in Tabs)
			{
				if (tab.GetType() == typeof(OptionsTab))
					continue;
				string name = $"B1NARY Tab: {tab.Name}";
				deleteAllKeysNames.Add(name);
				bool setValue = EditorPrefs.GetBool(name, tab.ShowInDebugger);
				bool result = EditorGUILayout.Toggle(tab.Name, setValue);
				if (result != setValue)
					EditorPrefs.SetBool(name, result);
			}
			EditorGUI.indentLevel--;
		};

		foreach (var (scriptName, dataObjects) in Data)
		{
			EditorGUILayout.Space(4);
			if (!showScriptSettings.ContainsKey(scriptName))
				showScriptSettings.Add(scriptName, false);
			showScriptSettings[scriptName] = EditorGUILayout.Foldout(showScriptSettings[scriptName], scriptName);
			if (!showScriptSettings[scriptName])
				continue;
			EditorGUI.indentLevel++;
			foreach (var data in dataObjects)
			{
				switch (data.Key)
				{
					case DebuggerPreferences.DataType.Bool:
						foreach (var (paramName, defaultValue) in data.Value)
							BoolTypeObject(paramName, (bool)defaultValue);
						break;
					case DebuggerPreferences.DataType.Int:
						foreach (var (paramName, defaultValue) in data.Value)
							IntTypeObject(paramName, (int)defaultValue);
						break;
					case DebuggerPreferences.DataType.Float:
						foreach (var (paramName, defaultValue) in data.Value)
							FloatTypeObject(paramName, (float)defaultValue);
						break;
					case DebuggerPreferences.DataType.String:
						foreach (var (paramName, defaultValue) in data.Value)
							StringTypeObject(paramName, (string)defaultValue);
						break;
					case DebuggerPreferences.DataType.StringPopup:
						foreach (var (paramName, @params) in data.Value)
							StringPopupTypeObject(paramName, 
								((ValueTuple<string[], int>)@params).Item1, 
								((ValueTuple<string[], int>)@params).Item2);
						break;
					default: throw new NotImplementedException
							($"{data.Key} is not in the switch statement!");
				}
			}
			EditorGUI.indentLevel--;
		}

		EditorGUILayout.Space(20);
		bool pressedDeleteAllButton = GUILayout.Button("Reset Settings");
		if (pressedDeleteAllButton)
			foreach (string name in deleteAllKeysNames)
				EditorPrefs.DeleteKey(name);

		void BoolTypeObject(string name, bool @default)
		{
			deleteAllKeysNames.Add(name);
			bool setValue = EditorPrefs.GetBool(name, @default);
			bool result = EditorGUILayout.Toggle(name, setValue);
			if (result != setValue)
				EditorPrefs.SetBool(name, result);
		}
		void IntTypeObject(string name, int @default)
		{
			deleteAllKeysNames.Add(name);
			int setValue = EditorPrefs.GetInt(name, @default);
			int result = EditorGUILayout.IntField(name, setValue);
			if (result != setValue)
				EditorPrefs.SetInt(name, result);
		}
		void FloatTypeObject(string name, float @default)
		{
			deleteAllKeysNames.Add(name);
			float setValue = EditorPrefs.GetFloat(name, @default);
			float result = EditorGUILayout.FloatField(name, setValue);
			if (result != setValue)
				EditorPrefs.SetFloat(name, result);
		}
		void StringTypeObject(string name, string @default)
		{
			deleteAllKeysNames.Add(name);
			string setValue = EditorPrefs.GetString(name, @default);
			string result = EditorGUILayout.TextField(name, setValue);
			if (result != setValue)
				EditorPrefs.SetString(name, result);
		}
		void StringPopupTypeObject(string name, string[] @params, int @default)
		{
			deleteAllKeysNames.Add(name);
			int oldSelection = EditorPrefs.GetInt(name, @default);
			int result = EditorGUILayout.Popup(name, oldSelection, @params);
			if (result != oldSelection)
				EditorPrefs.SetInt(name, result);
		}
	}
	// Order is at min value due to how ordering shown layers when changing it
	public override int Order => int.MinValue;
}