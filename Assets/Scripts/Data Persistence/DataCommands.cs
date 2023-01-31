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
			SaveSlot.SaveGame();
			objects.ForEach(obj => obj.SetActive(!obj.activeSelf));
		}
		public void LoadGame(InputAction.CallbackContext context)
		{
			SaveSlot.QuickLoad();
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.DataPersistence.Editor
{
	using B1NARY.Scripting;
	using System;
	using System.Collections.Generic;
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
			if (SaveSlot.Instance.scriptDocumentInterface.strings is null)
				return;
			UpdateTab("Strings", SaveSlot.Instance.scriptDocumentInterface.strings.AsEnumerable());
			UpdateTab("Integers", SaveSlot.Instance.scriptDocumentInterface.ints.AsEnumerable());
			UpdateTab("Booleans", SaveSlot.Instance.scriptDocumentInterface.bools.AsEnumerable());
			UpdateTab("Singles", SaveSlot.Instance.scriptDocumentInterface.floats.AsEnumerable());

			void UpdateTab<TKey, TValue>(string label, IEnumerable<KeyValuePair<TKey, TValue>> data)
			{
				EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
				EditorGUI.indentLevel++;
				using (var enumerator = data.GetEnumerator())
					while (enumerator.MoveNext())
					{
						Rect fullRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(Screen.width, 20f));
						EditorGUI.LabelField(new Rect(fullRect) { width = (fullRect.width / 2f) - 1f }, enumerator.Current.Key.ToString());
						string value = enumerator.Current.Value is Delegate del 
							? del.DynamicInvoke().ToString() 
							: enumerator.Current.Value.ToString();
						EditorGUI.LabelField(new Rect(fullRect) { xMin = (fullRect.width / 2f) + 1f }, value);
					}
				EditorGUI.indentLevel--;
			}
		}
	}
}
#endif