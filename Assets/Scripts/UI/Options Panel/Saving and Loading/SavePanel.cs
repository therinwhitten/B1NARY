﻿namespace B1NARY.UI.Saving
{
	using B1NARY.DataPersistence;
	using OVSSerializer.IO;
	using B1NARY.Scripting;
	using OVSSerializer;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;
	using UnityEngine;

	public class SavePanel : AutoPagePopulator
	{
		public GameObject slotTemplate;
		public GameObject newSaveSlotTemplate;

		public InterfaceHandler deletionActionPanel;
		public SavePanelActionPanel modifyActionPanel;

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
				KeyValuePair<OSFile, Lazy<SaveSlot>> slotPair = SaveSlot.AllSaves[i];
				try
				{
					HandleSave(slotPair.Key, slotPair.Value);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			m_allObjects = newList;

			void HandleSave(OSFile fileInfo, Lazy<SaveSlot> saveSlot)
			{
				GameObject instance = AddEntry(slotTemplate);
				var pair = new SaveSlotInstance()
				{
					source = saveSlot.Value,
					target = instance.GetComponent<SavePanelBehaviour>(),
				};
				// adding deletion button
				if (pair.target.deleteButton != null)
					pair.target.deleteButton.onClick.AddListener(() =>
					{
						deletionActionPanel.gameObject.SetActive(true);
						deletionActionPanel.text.text = "$DELETE";
						deletionActionPanel.OnPress += (delete) => { if (delete) Delete(pair.source); };
					});
				// Adding override function
				pair.target.button.onClick.AddListener(() =>
				{
					modifyActionPanel.gameObject.SetActive(true);
					modifyActionPanel.text.text = "$OVERRIDE";
					modifyActionPanel.inputField.text = pair.source.SaveName;
					modifyActionPanel.OnPress += (@override) => { if (@override) Override(pair.source, SaveSlot.ActiveSlot, modifyActionPanel.inputField.text); };
				});
				if (saveSlot.Value.metadata.thumbnail != null)
					pair.target.foregroundImage.sprite = saveSlot.Value.metadata.thumbnail.Sprite;
				pair.target.tmpText.text = saveSlot.Value.DisplaySaveContents;
				newList.Add(pair);
			}
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
			LoadPanelBehaviour behaviour = addNewInstance.GetComponent<LoadPanelBehaviour>();
			behaviour.button.onClick.AddListener(() =>
			{
				modifyActionPanel.gameObject.SetActive(true);
				modifyActionPanel.text.text = "$NEW";
				modifyActionPanel.inputField.text = "Quicksave";
				modifyActionPanel.OnPress += (@override) => { if (@override) CreateNew(SaveSlot.ActiveSlot, modifyActionPanel.inputField.text); };
			});
			SaveSlot.EmptiedSaveCache += UpdateSaves;
		}
		private void UpdateSaves()
		{
			OnDisable();
			OnEnable();
		}
		public void CreateNew(SaveSlot slot, string saveName)
		{
			SaveSlot newSlot;
			using (var stream = new MemoryStream())
			{
				OVSXmlSerializer<SaveSlot> serializer = new();
				serializer.Serialize(stream, slot);
				stream.Position = 0;
				newSlot = serializer.Deserialize(stream);
			}
			newSlot.metadata.ChangeFileTo(null);
			newSlot.SaveName = saveName;
			newSlot.Save();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="source">The source file to override to. </param>
		/// <param name="active">The file to override with as a replacement. </param>
		public void Override(SaveSlot source, SaveSlot active, string saveName)
		{
			OSFile fileInfo = source.metadata.DirectoryInfo;
			source.metadata.ChangeFileTo(null, true);
			active.metadata.ChangeFileTo(fileInfo);
			active.SaveName = saveName;
			active.Save();
		}
		public void Delete(SaveSlot slot)
		{
			slot.metadata.ChangeFileTo(null, true);
			SaveSlot.EmptySaveCache();
		}
		protected virtual void OnDisable()
		{
			SaveSlot.EmptiedSaveCache -= UpdateSaves;
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
			panel.deletionActionPanel = DirtyAuto.Field(target, new GUIContent("Action Panel"), panel.deletionActionPanel, true);
			panel.modifyActionPanel = DirtyAuto.Field(target, new GUIContent("Overwrite Panel"), panel.modifyActionPanel, true);
			panel.objectsPerRow = DirtyAuto.Slider(target, new GUIContent("Columns"), panel.objectsPerRow, 1, 6);
		}
	}
}
#endif