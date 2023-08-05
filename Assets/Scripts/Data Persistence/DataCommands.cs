namespace B1NARY.DataPersistence
{
	using B1NARY.DesignPatterns;
	using UnityEngine;
	using UnityEngine.InputSystem;

	public sealed class DataCommands : Singleton<DataCommands>
	{
		public PlayerInput input;
		public string saveButton = "QuickSave";
		public string loadButton = "LoadSave";

		private void Start()
		{
			input.actions.FindAction(saveButton, true).started += SaveGame;
			input.actions.FindAction(loadButton, true).started += LoadGame;
		}
		public void SaveGame(InputAction.CallbackContext context)
		{
			Debug.Log("gersgersrdhtdrytjdthrhrdtr6utrdytj5hrdtyt");
			SaveSlot.ActiveSlot.Save();
		}
		public void LoadGame(InputAction.CallbackContext context)
		{
			SaveSlot.ActiveSlot.Slot.Load();
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.DataPersistence.Editor
{
	using B1NARY.Editor;
	using B1NARY.Scripting;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(DataCommands), true)]
	public class DataCommandsEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DataCommands.input)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DataCommands.loadButton)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DataCommands.saveButton)));
			serializedObject.ApplyModifiedProperties();
			EditorGUILayout.Separator();
			if (!Application.isPlaying)
			{
				EditorGUILayout.HelpBox("Enter play mode to inspect data values!", MessageType.Error);
				return;
			}
			if (SaveSlot.ActiveSlot is null)
			{
				EditorGUILayout.HelpBox("Save slot not created yet!", MessageType.Error);
				return;
			}
			UpdateTab("Strings", SaveSlot.ActiveSlot.Slot.strings);
			UpdateTab("Booleans", SaveSlot.ActiveSlot.Slot.booleans);

			static void UpdateTab<T>(string label, DataPersistence.Collection<T> data)
			{
				EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
				EditorGUI.indentLevel++;
				using (var keys = data.Keys.GetEnumerator())
					while (keys.MoveNext())
					{
						Rect fullRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 20f)),
							keyRect = new(fullRect) { width = (fullRect.width / 2f) - 1f },
							valueRect = new(fullRect) { xMin = (fullRect.width / 2f) + 1f };
						EditorGUI.LabelField(keyRect, keys.Current);
						object value = data[keys.Current];
						if (data.IsPointer(keys.Current) == true)
						{
							EditorGUI.LabelField(valueRect, value.ToString());
						}
						else if (value is bool @bool)
						{
							bool newBool = EditorGUI.Toggle(valueRect, @bool);
							if (newBool != @bool)
								data[keys.Current] = (T)(object)newBool;
						}
						else if (value is string @string)
						{
							string newString = EditorGUI.TextField(valueRect, @string);
							if (newString != @string)
								data[keys.Current] = (T)(object)newString;
						}
						else if (value is int @int)
						{
							int newInt = EditorGUI.IntField(valueRect, @int);
							if (newInt != @int)
								data[keys.Current] = (T)(object)newInt;
						}
						else if (value is float @float)
						{
							float newFloat = EditorGUI.FloatField(valueRect, @float);
							if (newFloat != @float)
								data[keys.Current] = (T)(object)newFloat;
						}
						else
						{
							EditorGUI.LabelField(valueRect, value.ToString());
						}
					}
				EditorGUI.indentLevel--;
			}
		}
	}
}
#endif