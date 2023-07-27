namespace B1NARY.UI.Saving
{
	using B1NARY.DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.Events;

	public class LoadPanel : AutoPagePopulator
	{
		public GameObject slotTemplate;

		public InterfaceHandler actionPanel;
		
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
				pair.source.hasSaved = true;
				pair.target.button.onClick.AddListener(() =>
				{
					actionPanel.gameObject.SetActive(true);
					actionPanel.text.text = "$LOAD";
					actionPanel.OnPress += shouldLoad => { if (shouldLoad) pair.source.Load(); };
				});
				if (pair.target.deleteButton != null)
					pair.target.deleteButton.onClick.AddListener(() =>
					{
						actionPanel.gameObject.SetActive(true);
						actionPanel.text.text = "$DELETE";
						actionPanel.OnPress += (delete) => { if (delete) Delete(pair.source); };
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
			_ = AllObjects;
			SaveSlot.EmptiedSaveCache += UpdateSaves;
		}
		private void UpdateSaves()
		{
			OnDisable();
			OnEnable();
		}
		protected virtual void OnDisable()
		{
			SaveSlot.EmptiedSaveCache -= UpdateSaves;
			Clear();
		}
		public void Delete(SaveSlot slot)
		{
			slot.metadata.ChangeFileTo(null, true);
			SaveSlot.EmptySaveCache();
			OnDisable();
			OnEnable();
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
	using B1NARY.UI.Saving;
	using System;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(LoadPanel), true)]
	public class LoadPanelEditor : SavePanelEditor
	{
		protected new LoadPanel panel;
		private void Awake() => panel = (LoadPanel)target;
		public override void OnInspectorGUI()
		{
			panel.slotTemplate = DirtyAuto.Field(target, new("Slot"), panel.slotTemplate, true);
			panel.row = DirtyAuto.Field(target, new("Row"), panel.row, true);
			panel.actionPanel = DirtyAuto.Field(target, new("Action Panel"), panel.actionPanel, true);
			panel.objectsPerRow = DirtyAuto.Slider(target, new("Columns"), panel.objectsPerRow, 1, 6);
		}
	}
}
#endif