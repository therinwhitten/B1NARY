namespace B1NARY.Editor
{
	using UnityEditor;
	using UnityEngine;
	using System;
	using System.Linq;

	[CustomEditor(typeof(TabPanel))]
	public sealed class TabPanelEditor : Editor
	{
		private TabPanel tabPanel;
		private void Awake() => tabPanel = (TabPanel)target;
		public override void OnInspectorGUI()
		{
			if (tabPanel.tabButtons.Any())
			{
				string value = "Null";
				if (tabPanel.tabButtons.Length > tabPanel.CurrentTabIndex)
					if (tabPanel.tabButtons[tabPanel.CurrentTabIndex] != null)
						value = tabPanel.tabButtons[tabPanel.CurrentTabIndex].name;
				EditorGUILayout.LabelField($"Current Tab: {value}", EditorStyles.foldout);
				if (GUI.Button(GUILayoutUtility.GetLastRect(), "", GUIStyle.none))
				{
					EditorUtility.SetDirty(tabPanel);
					var selectionMenu = new GenericMenu();
					for (int i = 0; i < tabPanel.tabButtons.Length; i++)
					{
						if (tabPanel.tabButtons[i] == null)
							continue;
						selectionMenu.AddItem(new GUIContent($"{i}. {tabPanel.tabButtons[i].name}"), false,
							() => 
							{ 
								tabPanel.GetType().GetProperty(nameof(TabPanel.CurrentTabIndex)).SetValue(tabPanel, i);
								EditorUtility.ClearDirty(tabPanel);
							});
					}
					selectionMenu.ShowAsContext();
				}
			}
			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TabPanel.tabButtons)));
			serializedObject.ApplyModifiedProperties();
			//var genericMenu = new GenericMenu();
		}
	}
}