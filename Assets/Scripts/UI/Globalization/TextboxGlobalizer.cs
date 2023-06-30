namespace B1NARY.Globalization
{
	using B1NARY.UI.Globalization;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using TMPro;
	using UnityEngine;

	public class TextboxGlobalizer : MonoBehaviour
	{
		public TMP_Text text;

		public List<string> languageKeys = new List<string>();
		public List<string> languageValues = new List<string>();

		public string this[string language]
		{
			get
			{
				for (int i = 0; i < languageKeys.Count; i++)
				{
					if (languageKeys[i] == language)
						return languageValues[i];
				}
				throw new IndexOutOfRangeException(language);
			}
		}

		private void Reset()
		{
			text = GetComponent<TMP_Text>();
			UpdateLanguageList();
			if (text != null && languageValues.Count > 0)
				languageValues[0] = text.text;
		}

		private void OnEnable()
		{
			if (text == null)
				throw new MissingFieldException(nameof(TMP_Text), nameof(text));
			PlayerConfig.Instance.language.AttachValue(UpdateLanguage);
		}
		private void OnDisable()
		{
			PlayerConfig.Instance.language.ValueChanged -= UpdateLanguage;
		}

		private void UpdateLanguage(string newLanguage)
		{
			text.text = this[newLanguage];
		}

		internal void UpdateLanguageList()
		{
			Dictionary<string, string> existingLanguage = new Dictionary<string, string>();
			for (int i = 0; i < languageKeys.Count; i++)
				existingLanguage.Add(languageKeys[i], languageValues[i]);
			HashSet<string> preExistingLanguage = new HashSet<string>(Languages.Instance);

			// Adding newly added languages
			for (int i = 0; i < Languages.Instance.Count; i++)
				if (existingLanguage.ContainsKey(Languages.Instance[i]) == false)
				{
					languageKeys.Add(Languages.Instance[i]);
					languageValues.Add("");
				}

			// Removing older languages
			for (int i = 0; i < languageKeys.Count; i++)
				if (preExistingLanguage.Contains(languageKeys[i]) == false)
				{
					languageKeys.RemoveAt(i);
					i--;
				}
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

	[CustomEditor(typeof(TextboxGlobalizer))]
	public class TextboxGlobalizerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			TextboxGlobalizer globalizer = (TextboxGlobalizer)target;
			globalizer.UpdateLanguageList();
			globalizer.text = DirtyAuto.Field(target, new GUIContent("Text"), globalizer.text, true);
			EditorGUILayout.Space();
			for (int i = 0; i < globalizer.languageKeys.Count; i++)
			{
				globalizer.languageValues[i] = DirtyAuto.Field(target, new GUIContent(globalizer.languageKeys[i]), globalizer.languageValues[i]);
			}
			Languages.Instance.Editor_OnGUI();
		}
	}
}
#endif
