namespace B1NARY.UI.Globalization
{
	using B1NARY.DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using TMPro;
	using UnityEngine;

	public class DropdownGlobalizer : MonoBehaviour
	{
		private const char SPLIT_CHAR = '\n';
		public TMP_Dropdown dropdown;

		// Both lists are equal length, and the values act as a 2-dimensional
		// - array, using the split char to separate them to fit the unity's 
		// - specific way to prevent it to whine like a little baby.
		[SerializeField]
		public List<string> languageKeys = new List<string>();
		[SerializeField]
		public List<string> languageValues = new List<string>();

		public string[] this[string language]
		{
			get
			{
				for (int i = 0; i < languageKeys.Count; i++)
				{
					if (languageKeys[i] == language)
						return languageValues[i].Split(SPLIT_CHAR);
				}
				throw new IndexOutOfRangeException(language);
			}
			set
			{
				// Modifying the input directly
				if (value.Length != ExpectedLines)
				{
					var modified = new List<string>(value);
					// Adding new
					while (modified.Count < ExpectedLines)
						modified.Add(" ");
					// Removing old
					while (modified.Count > ExpectedLines)
						modified.RemoveAt(modified.Count - 1);
					value = modified.ToArray();
				}
				for (int i = 0; i < languageKeys.Count; i++)
				{
					if (languageKeys[i] != language)
						continue;
					// selected language
					languageValues[i] = string.Join(SPLIT_CHAR.ToString(), value);
					return;
				}
				throw new IndexOutOfRangeException(language);
			}
		}

		public int ExpectedLines
		{
			get => expectedLines;
			set
			{
				if (expectedLines == value)
					return;
				expectedLines = value;
				// Update existing values
				for (int i = 0; i < languageKeys.Count; i++)
					this[languageKeys[i]] = this[languageKeys[i]];
			}
		}
		[SerializeField]
		public int expectedLines = 1;

		private void Reset()
		{
			dropdown = GetComponent<TMP_Dropdown>();
			UpdateLanguageList();
			int dropdownLength = 1;
			if (dropdown != null)
				dropdownLength = Math.Max(1, dropdown.options.Count);
			ExpectedLines = dropdownLength;
			if (dropdown != null && languageValues.Count > 0)
				this[languageKeys[0]] = dropdown.options.Select(option => option.text).ToArray();
		}

		private void OnEnable()
		{
			if (dropdown == null)
				throw new MissingFieldException(nameof(TMP_Dropdown), nameof(dropdown));
			PlayerConfig.Instance.language.AttachValue(UpdateLanguage);
		}
		private void OnDisable()
		{
			PlayerConfig.Instance.language.ValueChanged -= UpdateLanguage;
		}

		private void UpdateLanguage(string newLanguage)
		{
			List<TMP_Dropdown.OptionData> list = dropdown.options;
			int count = Math.Min(list.Count, expectedLines);
			string[] data = this[newLanguage];
			for (int i = 0; i < count; i++)
				list[i].text = data[i];
			dropdown.options = list;
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
					languageValues.RemoveAt(i);
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
	using System.Collections.Generic;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.UI;

	[CustomEditor(typeof(DropdownGlobalizer))]
	public class DropdownGlobalizerEditor : Editor
	{
		private List<bool> enabled;
		public override void OnInspectorGUI()
		{
			DropdownGlobalizer globalizer = (DropdownGlobalizer)target;
			globalizer.UpdateLanguageList();
			if (enabled == null || enabled.Count != globalizer.languageKeys.Count)
				enabled = new List<bool>(Enumerable.Repeat(true, globalizer.languageKeys.Count));
			globalizer.dropdown = DirtyAuto.Field(target, new GUIContent("Dropdown"), globalizer.dropdown, true);
			globalizer.ExpectedLines = DirtyAuto.Field(target, new GUIContent("Expected Lines"), globalizer.ExpectedLines);
			EditorGUILayout.Space();
			for (int i = 0; i < globalizer.languageKeys.Count; i++)
			{
				enabled[i] = EditorGUILayout.Foldout(enabled[i], globalizer.languageKeys[i]);
				if (!enabled[i])
					continue;
				EditorGUI.indentLevel++;
				string[] subValues = globalizer[globalizer.languageKeys[i]];
				for (int ii = 0; ii < subValues.Length; ii++)
				{
					string newValue = EditorGUILayout.TextField(subValues[ii]);
					if (subValues[ii] != newValue)
					{
						globalizer.SetDirty();
						subValues[ii] = newValue;
					}
				}
				globalizer[globalizer.languageKeys[i]] = subValues;
				EditorGUI.indentLevel--;
			}
			Languages.Instance.Editor_OnGUI();
		}
	}
}
#endif
