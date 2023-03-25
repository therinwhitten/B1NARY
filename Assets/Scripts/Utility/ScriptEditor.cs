#if UNITY_EDITOR
namespace B1NARY.Editor
{
	using System;
	using UnityEngine;
	using UnityEditor;
	using B1NARY.Scripting;
	using UnityEditor.UI;
	using System.Collections.Generic;
	using System.Linq;

	/*
	public class ScriptEditor : EditorWindow
	{
		private static readonly Vector2 defaultMinSize = new Vector2(400f, 350f);

		[MenuItem("B1NARY/Script Editor", priority = 1)]
		public static void ShowWindow()
		{
			// Get existing open window or if none, make a new one:
			ScriptEditor window = GetWindow<ScriptEditor>();
			window.titleContent = new GUIContent("Script Editor");
			window.minSize = defaultMinSize;
			window.Show();
		}

		public string fullPath = string.Empty;
		public string VisualPath { get => ScriptHandler.DocumentList.ToVisual(fullPath); }
		private void Awake()
		{
			fullPath = ScriptHandler.HasInstance
				? ScriptHandler.Instance.defaultStreamingAssetsDocumentPath
				: ScriptHandler.AllDocuments[0].FullName;
		}
		private void OnGUI()
		{
			Rect fullRect = new Rect(0f, 0f, position.width, position.height);
			//TopInfoBar(new Rect(fullRect) { yMax = 40f });

			
			void LeftShortcutTab(Rect rect)
			{

			}
			void RightDetailsBox(Rect rect)
			{

			}
		}

		/*
		private void TopInfoBar(Rect rect)
		{
			if (EditorGUI.DropdownButton(new Rect(rect) { width = (rect.width / 2) - 20f }, new GUIContent(VisualPath), FocusType.Passive))
			{
				var menu = new GenericMenu();
				List<visua> visualItems = ScriptHandler.AllDocuments,
					previousLists = visualItems[0].Split('/', '\\').ToList();
				previousLists.RemoveAt(previousLists.Count - 1);
				for (int i = 0; i < visualItems.Count; i++)
				{
					int captured = i;
					List<string> splits = visualItems[i].Split('/', '\\').ToList();
					splits.RemoveAt(splits.Count - 1);

					// separator checks
					void AddSeparator()
					{
						if (visualItems.Count != splits.Count)
						{

						}
					}

					menu.AddItem(new GUIContent(visualItems[i]), visualItems[i] == VisualPath, () => fullPath = ScriptHandler.GetFullDocumentsPaths()[captured]);
				}
				menu.DropDown(rect);

				
			}

		}
	}
		*/
}
#endif