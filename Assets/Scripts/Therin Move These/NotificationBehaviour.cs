namespace B1NARY
{
	using B1NARY.DataPersistence;
	using B1NARY.Globalization;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;

	public class NotificationBehaviour : TextboxGlobalizer
	{
		[SerializeField]
		public Button closeButton;

		public void SetText(CollectibleCollection.NewFlag flag)
		{
			SetText(flag.FlagName, flag.FormalName);
		}
	}
}

#if UNITY_EDITOR
namespace B1NARY.Globalization.Editor
{
	using B1NARY.Editor;
	using B1NARY.UI.Globalization;
	using System;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.UI;

	[CustomEditor(typeof(NotificationBehaviour))]
	public class NotificationGlobalizerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			NotificationBehaviour globalizer = (NotificationBehaviour)target;
			globalizer.UpdateLanguageList();
			globalizer.closeButton = DirtyAuto.Field<Button>(target, new GUIContent("Button"), globalizer.closeButton, true);
			globalizer.text = DirtyAuto.Field(target, new GUIContent("Text"), globalizer.text, true);
			EditorGUILayout.Space();
			for (int i = 0; i < globalizer.languageKeys.Count; i++)
			{
				globalizer.languageValues[i] = EditorGUILayout.TextArea(globalizer.languageValues[i], GUILayout.Height(50));
			}
			Languages.Instance.Editor_OnGUI();
		}
	}
}
#endif
