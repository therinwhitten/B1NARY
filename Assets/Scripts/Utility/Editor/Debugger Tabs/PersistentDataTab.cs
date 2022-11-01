namespace B1NARY.Editor.Debugger
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using B1NARY.DataPersistence;
	using UnityEditor;
	using UnityEngine;
	/*
	public sealed class PersistentDataTab : DebuggerTab
	{
		public override string Name => "Persistent Data";
		public override bool ConstantlyRepaint => true;

		
		private static readonly Type[] persistentDataTypes =
		{
			typeof(GamePreferences),
			typeof(PersistentData),
		};
		

		public override void DisplayTab()
		{
			ShowGameStateBlock();
		}

		It doesn't exist here
		private Vector2 gamePreferencesScroll = Vector2.zero;
		private void ShowGamePreferences()
		{
			EditorGUILayout.LabelField("Game State", EditorStyles.boldLabel);
			Lookup<string, string> gamePrefs = GamePreferencesData();
			gamePreferencesScroll = EditorGUILayout.BeginScrollView(gamePreferencesScroll);
			foreach (var group in gamePrefs)
			{
				DisplayLine(group.Key);
				foreach (var item in group)
					DisplayLine('\t' + item);
			}
			EditorGUILayout.EndScrollView();
		}
		private Lookup<string, string> GamePreferencesData()
		{
			var output = new List<(string type, string data)>();
		}
		

		private Vector2 gameStateScroll = Vector2.zero;
		private void ShowGameStateBlock()
		{
			EditorGUILayout.LabelField("Game State", EditorStyles.boldLabel);
			Lookup<string, string> persistentData = GameStateData();
			gameStateScroll = EditorGUILayout.BeginScrollView(gameStateScroll);
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
			output.AddRange(PersistentData.strings.Keys.Select(key => ("strings", $"{key}: {PersistentData.strings[key]}")));
			output.AddRange(PersistentData.bools.Keys.Select(key => ("bools", $"{key}: {PersistentData.bools[key]}")));
			output.AddRange(PersistentData.ints.Keys.Select(key => ("ints", $"{key}: {PersistentData.ints[key]}")));
			output.AddRange(PersistentData.floats.Keys.Select(key => ("floats", $"{key}: {PersistentData.floats[key]}")));
			return (Lookup<string, string>)output.ToLookup(line => line.type, line => line.data);
		}



		
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
		




		private void DisplayLine(object input)
		{
			var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 15);
			rect.x += 5;
			rect.width -= 10;
			EditorGUI.LabelField(rect, input.ToString());
		}
	}
	*/
}