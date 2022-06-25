using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public sealed class PersistentDataTab : DebuggerTab
{
	public override string Name => "Persistent Data";

	/*
	private static readonly Type[] persistentDataTypes =
	{
		typeof(GamePreferences),
		typeof(PersistentData),
	};
	*/

	private Vector2 scroll = Vector2.zero;
	public override void DisplayTab()
	{
		Lookup<string, string> persistentData = GameStateData();
		scroll = EditorGUILayout.BeginScrollView(scroll);
		foreach (var group in persistentData)
		{
			DisplayLine(group.Key);
			foreach (var item in group)
				DisplayLine('\t' + item);
		}
		EditorGUILayout.EndScrollView();
	}
	private Lookup<string, string> GameStateData()
	{
		var output = new List<(string type, string data)>();
		output.AddRange(PersistentData.Instance.state.strings.Keys.Select(key => ("strings", $"{key}: {PersistentData.Instance.state.strings[key]}")));
		output.AddRange(PersistentData.Instance.state.bools.Keys.Select(key => ("bools", $"{key}: {PersistentData.Instance.state.bools[key]}")));
		output.AddRange(PersistentData.Instance.state.ints.Keys.Select(key => ("ints", $"{key}: {PersistentData.Instance.state.ints[key]}")));
		output.AddRange(PersistentData.Instance.state.floats.Keys.Select(key => ("floats", $"{key}: {PersistentData.Instance.state.floats[key]}")));
		return (Lookup<string, string>)output.ToLookup(line => line.type, line => line.data);
	}



	/*
	private void DisplayClassData(Type type)
	{
		BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
			BindingFlags.Static;
		object instance = null;
		if (type.IsSubclassOf(typeof(MonoBehaviour)))
		{
			MonoBehaviour persistentDataClass = UnityEngine.Object.FindObjectOfType(type) as MonoBehaviour;
			if (persistentDataClass == null)
			{
				Debug.LogError($"{type.Name} as Component doesn't currently exist!");
				return;
			}
			instance = persistentDataClass;
		}
		var fieldData = type.GetFields(flags).Where(info => info.FieldType == typeof(Dictionary<string, string>) 
			|| info.FieldType == typeof(Dictionary<string, int>) 
			|| info.FieldType == typeof(Dictionary<string, float>) 
			|| info.FieldType == typeof(Dictionary<string, bool>)) // Get the key of dictionary from string with unknown values
			.ToLookup(info => (info.GetType().GetGenericArguments()[0].);
	}
	*/




	private void DisplayLine(object input)
	{
		var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 15);
		rect.x += 5;
		rect.width -= 10;
		EditorGUI.LabelField(rect, input.ToString());
	}
}