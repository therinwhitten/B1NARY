namespace B1NARY.DataPersistence
{
	using B1NARY.DesignPatterns;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;
	using UnityEngine;
	using UnityEngine.InputSystem;

	public sealed class DataCommands : Singleton<DataCommands>
	{
		public PlayerInput input;
		public string saveButton = "QuickSave";
		public string loadButton = "LoadSave";
		public List<string> toggledGameObjectNames;
		private List<GameObject> Objects
		{
			get
			{
				var objects = new List<GameObject>(toggledGameObjectNames.Count);
				for (int i = 0; i < toggledGameObjectNames.Count; i++)
				{
					GameObject obj = GameObject.Find(name);
					if (obj != null)
						objects.Add(obj);
				}
				return objects;
			}
		}

		private void OnEnable()
		{
			input.actions.FindAction(saveButton, true).performed += SaveGame;
			input.actions.FindAction(loadButton, true).performed += LoadGame;
		}
		private void OnDisable()
		{
			input.actions.FindAction(saveButton, true).performed -= SaveGame;
			input.actions.FindAction(loadButton, true).performed -= LoadGame;
		}
		public void SaveGame(InputAction.CallbackContext context)
		{
			StartCoroutine(ScreenshotDelay(Objects));
		}
		private IEnumerator ScreenshotDelay(List<GameObject> objects)
		{
			objects.ForEach(obj => obj.SetActive(!obj.activeSelf));
			yield return new WaitForEndOfFrame();
			SaveSlot.ActiveSlot.Save();
			objects.ForEach(obj => obj.SetActive(!obj.activeSelf));
		}
		public void LoadGame(InputAction.CallbackContext context)
		{
			SaveSlot.ActiveSlot.Load();
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
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DataCommands.toggledGameObjectNames)));
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
			UpdateTab("Strings", SaveSlot.ActiveSlot.ScriptDocumentInterface.strings);
			UpdateTab("Integers", SaveSlot.ActiveSlot.ScriptDocumentInterface.ints);
			UpdateTab("Booleans", SaveSlot.ActiveSlot.ScriptDocumentInterface.bools);
			UpdateTab("Singles", SaveSlot.ActiveSlot.ScriptDocumentInterface.floats);

			void UpdateTab<T>(string label, ScriptDocumentInterface.Collection<T> data)
			{
				EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
				EditorGUI.indentLevel++;
				
				using (var keys = data.Keys.GetEnumerator())
					while (keys.MoveNext())
					{
						Rect fullRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 20f)),
							keyRect = new Rect(fullRect) { width = (fullRect.width / 2f) - 1f },
							valueRect = new Rect(fullRect) { xMin = (fullRect.width / 2f) + 1f };
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