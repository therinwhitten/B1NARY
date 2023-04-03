namespace B1NARY.UI.Saving
{
	using B1NARY.DataPersistence;
	using B1NARY.Scripting;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using UnityEngine;
	using UnityEngine.UI;

	public class SavePanel : AutoPagePopulator
	{
		public GameObject slotTemplate;
		public GameObject newSaveSlotTemplate;

		public InterfaceHandler actionPanel;
		public SavePanelActionPanel modifyPanel;

		private List<SaveSlotInstance> m_allObjects;
		protected List<SaveSlotInstance> AllObjects
		{
			get
			{
				if (m_allObjects is null)
					UpdateGameObjectSaves();
				return m_allObjects;
			}
			set => m_allObjects = value;
		}
		public void UpdateGameObjectSaves()
		{
			if (m_allObjects != null)
				m_allObjects = null;
			var newList = new List<SaveSlotInstance>();
			for (int i = 0; i < SaveSlot.AllSaves.Count; i++)
			{
				KeyValuePair<FileInfo, Lazy<SaveSlot>> slotPair = SaveSlot.AllSaves[i];
				GameObject instance = AddEntry(slotTemplate);
				var pair = new SaveSlotInstance()
				{
					source = slotPair.Value.Value,
					target = instance.GetComponent<SavePanelBehaviour>(),
				};
				// adding deletion button
				if (pair.target.deleteButton != null)
					pair.target.deleteButton.onClick.AddListener(() =>
					{
						actionPanel.gameObject.SetActive(true);
						actionPanel.text.text = "Delete Save?";
						actionPanel.OnPress += (delete) => { if (delete) Delete(pair.source); };
					});
				// Adding override function
				pair.target.button.onClick.AddListener(() =>
				{
					modifyPanel.gameObject.SetActive(true);
					modifyPanel.text.text = "Override Save?";
					modifyPanel.inputField.text = pair.source.saveName;
					modifyPanel.OnPress += (@override) => { if (@override) Override(pair.source, SaveSlot.ActiveSlot, modifyPanel.inputField.text); };
				});
				if (slotPair.Value.Value.metadata.thumbnail != null)
					pair.target.foregroundImage.sprite = slotPair.Value.Value.metadata.thumbnail.Sprite;
				pair.target.tmpText.text = slotPair.Value.Value.DisplaySaveContents;
				newList.Add(pair);
			}
			m_allObjects = newList;
		}

		protected virtual void OnEnable()
		{
			if (SceneManager.ActiveScene.buildIndex == 0)
			{
				gameObject.SetActive(false);
				return;
			}
			UpdateGameObjectSaves();
			GameObject addNewInstance = AddEntry(newSaveSlotTemplate);
			Button button = addNewInstance.GetComponentInChildren<Button>();
			button.onClick.AddListener(() =>
			{
				modifyPanel.gameObject.SetActive(true);
				modifyPanel.text.text = "New Save?";
				modifyPanel.inputField.text = SaveSlot.ActiveSlot.saveName;
				modifyPanel.OnPress += (@override) => 
				{
					SaveSlot.ActiveSlot.metadata.DirectoryInfo = null;
					if (@override)
						CreateNew(SaveSlot.ActiveSlot, modifyPanel.inputField.text); 
				};
			});
		}
		public void CreateNew(SaveSlot slot, string newSaveName)
		{
			slot.metadata.DirectoryInfo = null;
			slot.saveName = newSaveName;
			slot.Save();
			SaveSlot.EmptySaveCache();
			OnDisable();
			OnEnable();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="source">The source file to override to. </param>
		/// <param name="active">The file to override with as a replacement. </param>
		public void Override(SaveSlot source, SaveSlot active, string newSaveName)
		{
			FileInfo fileInfo = source.metadata.DirectoryInfo;
			source.metadata.DirectoryInfo = null;
			active.metadata.DirectoryInfo = fileInfo;
			active.saveName = newSaveName;
			active.Save();
			SaveSlot.EmptySaveCache();
			OnDisable();
			OnEnable();
		}
		public void Delete(SaveSlot slot)
		{
			FileInfo fileInfo = slot.metadata.DirectoryInfo;
			if (fileInfo.Exists)
				fileInfo.Delete();
			slot.metadata.DirectoryInfo = null;
			SaveSlot.EmptySaveCache();
			OnDisable();
			OnEnable();
		}
		protected virtual void OnDisable()
		{
			Clear();
		}
		public override void Clear()
		{
			base.Clear();
			AllObjects = null;
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.UI.Editor
{
	using B1NARY.Editor;
	using B1NARY.UI;
	using B1NARY.UI.Saving;
	using UnityEditor;
	using UnityEngine;
	
	[CustomEditor(typeof(SavePanel), true)]
	public class SavePanelEditor : Editor
	{
		protected SavePanel panel;
		private void Awake() => panel = (SavePanel)target;
		public override void OnInspectorGUI()
		{
			panel.slotTemplate = DirtyAuto.Field(target, new GUIContent("Slot"), panel.slotTemplate, true);
			panel.newSaveSlotTemplate = DirtyAuto.Field(target, new GUIContent("Add Slot"), panel.newSaveSlotTemplate, true);
			panel.row = DirtyAuto.Field(target, new GUIContent("Row"), panel.row, true);
			panel.actionPanel = DirtyAuto.Field(target, new GUIContent("Action Panel"), panel.actionPanel, true);
			panel.modifyPanel = DirtyAuto.Field(target, new GUIContent("Modify Action Panel"), panel.modifyPanel, true);
			panel.objectsPerRow = DirtyAuto.Slider(target, new GUIContent("Columns"), panel.objectsPerRow, 1, 6);
		}
	}
}
#endif