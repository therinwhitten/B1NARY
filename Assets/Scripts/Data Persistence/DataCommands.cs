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
		private void SaveGame(InputAction.CallbackContext context)
		{
			StartCoroutine(ScreenshotDelay(Objects));
		}
		private IEnumerator ScreenshotDelay(List<GameObject> objects)
		{
			objects.ForEach(obj => obj.SetActive(!obj.activeSelf));
			yield return new WaitForEndOfFrame();
			PersistentData.SaveGame();
			objects.ForEach(obj => obj.SetActive(!obj.activeSelf));
		}
		private void LoadGame(InputAction.CallbackContext context)
		{
			PersistentData.LoadGame();
		}
	}
}