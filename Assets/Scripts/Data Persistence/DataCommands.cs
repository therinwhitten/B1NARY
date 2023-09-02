namespace B1NARY.DataPersistence
{
	using B1NARY.DesignPatterns;
	using B1NARY.Scripting;
	using B1NARY.UI;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.InputSystem;
	using UnityEngine.UI;

	public sealed class DataCommands : Singleton<DataCommands>
	{
		public PlayerInput input;
		public string save;
		public string load;
		public string escape;

		[Space]
		public string exitPopupTag;

		protected override void SingletonAwake()
		{
			input.actions.FindAction(save, true).performed += SaveGame;
			input.actions.FindAction(load, true).performed += LoadGame;
			input.actions.FindAction(escape, true).performed += EscapeGame;
		}
		public void SaveGame(InputAction.CallbackContext context)
		{
			SaveSlot.ActiveSlot.Save();
		}
		public void LoadGame(InputAction.CallbackContext context)
		{
			SaveSlot.ActiveSlot.Load();
		}
		
		public void EscapeGame(InputAction.CallbackContext context)
		{
			if (ScriptHandler.Instance.documentWatcher == null)
			{
				GameObject obj = GameObject.FindWithTag(exitPopupTag);
				obj.SetActive(!obj.activeSelf);
				return;
			}
			if (!OptionsMenuPauser.HasInstance)
				OptionsMenuPauser.Instance = OptionsMenuPauser.ForceFind();
			GameObject options = OptionsMenuPauser.Instance.gameObject;
			options.SetActive(!options.activeSelf);
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
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DataCommands.save)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DataCommands.load)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DataCommands.escape)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DataCommands.exitPopupTag)));
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
			UpdateTab("Strings", SaveSlot.ActiveSlot.strings);
			UpdateTab("Booleans", SaveSlot.ActiveSlot.booleans);

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