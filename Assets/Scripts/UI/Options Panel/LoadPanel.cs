namespace B1NARY.UI
{
	using B1NARY.DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using UnityEngine.Events;

	public class LoadPanel : SavePanel
	{
		protected override void OnEnable()
		{
			base.Awake();
			objects = GetSaves().ToList();
			for (int i = 0; i < objects.Count; i++)
			{
				BlockInfo block = objects[i];
				block.button.onClick.AddListener(() =>
				{
					var @interface = new BoxInterface(panels[0]);
					@interface.PressedButton += (@bool) =>
					{
						@interface.Dispose();
						if (@bool == false)
							return;
						SaveSlot.LoadSlot(block.fileData);
					};
				});
			}
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.UI.Editor
{
	using B1NARY.Editor;
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
			panel.slot = DirtyAuto.Field(target, new GUIContent("Slot"), panel.slot, true);
			panel.row = DirtyAuto.Field(target, new GUIContent("Row"), panel.row, true);
			panel.panels[0] = DirtyAuto.Field(target, new GUIContent("Confirm Panel"), panel.panels[0], true);
			panel.objectsPerRow = DirtyAuto.Slider(target, new GUIContent("Columns"), panel.objectsPerRow, 1, 6);
		}
	}
}
#endif