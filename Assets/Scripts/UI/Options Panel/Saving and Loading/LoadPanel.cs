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

		private List<LoadSlotInstance> m_allObjects;
		protected List<LoadSlotInstance> AllObjects
		{
			get
			{
				if (m_allObjects is null)
				{
					var newList = new List<LoadSlotInstance>();
					for (int i = 0; i < SaveSlot.AllSaves.Count; i++)
					{
						KeyValuePair<FileInfo, Lazy<SaveSlot>> slotPair = SaveSlot.AllSaves[i];
						GameObject instance = AddEntry(slotTemplate);
						var pair = new LoadSlotInstance()
						{
							source = slotPair.Value.Value,
							target = instance.GetComponent<LoadPanelBehaviour>(),
						};
						pair.target.button.onClick.AddListener(() =>
						{
							actionPanel.gameObject.SetActive(true);
							actionPanel.OnPress += shouldLoad => { if (shouldLoad) pair.source.Load(); };
						});
						newList.Add(pair);
					}
					m_allObjects = newList;
				}
				return m_allObjects;
			}
			set => m_allObjects = value;
		}

		protected virtual void OnEnable()
		{
			_ = AllObjects;
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
			panel.slotTemplate = DirtyAuto.Field(target, new GUIContent("Slot"), panel.slotTemplate, true);
			panel.row = DirtyAuto.Field(target, new GUIContent("Row"), panel.row, true);
			panel.actionPanel = DirtyAuto.Field(target, new GUIContent("Action Panel"), panel.actionPanel, true);
			panel.objectsPerRow = DirtyAuto.Slider(target, new GUIContent("Columns"), panel.objectsPerRow, 1, 6);
		}
	}
}
#endif