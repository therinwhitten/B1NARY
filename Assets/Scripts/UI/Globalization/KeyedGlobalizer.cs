namespace B1NARY.UI.Globalization
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using TMPro;
	using UnityEngine;
	using LanguageValuePair = System.Collections.Generic.KeyValuePair<string, string>;

	public class KeyedGlobalizer : MonoBehaviour
	{
		private const char SPLIT_CHAR = '\r';

		public TMP_Text text;

		// Headers are split based on the keys, but the actual entries will
		// - act as a key value pair of language and the value of itself.
		public List<string> languages = new();
		
		[SerializeField]
		public List<string> languageKeys = new();
		public int ExpectedLines
		{
			get => languageKeys.Count;
			set
			{
				if (value == ExpectedLines)
					return;
				while (languageKeys.Count < value)
				{
					languageKeys.Add("");
					languageValues.Add(string.Join(SPLIT_CHAR, Enumerable.Repeat("", languages.Count)));
				}
				while (languageKeys.Count > value)
				{
					languageKeys.RemoveAt(languageKeys.Count - 1);
					languageValues.RemoveAt(languageValues.Count - 1);
				}
			}
		}

		[SerializeField]
		public List<string> languageValues = new();

		/// <summary>
		/// Gets or sets the list provided where the keys are the languages, and
		/// the values are the actual text.
		/// </summary>
		/// <param name="key"> The key to get the list of key-languages and value-texts. </param>
		/// <exception cref="IndexOutOfRangeException"/>
		public List<LanguageValuePair> this[string key]
		{
			get
			{
				for (int i = 0; i < languageKeys.Count; i++)
				{
					if (languageKeys[i] != key)
						continue;
					string[] values = languageValues[i].Split(SPLIT_CHAR);
					List<LanguageValuePair> output = values
						.Zip(languages, (value, language) => new LanguageValuePair(language, value)).ToList();
					return output;
				}
				throw new IndexOutOfRangeException(key);
			}
			set
			{
				// Making sure the languages itself is accounted for
				LinkedList<string> existingLanguages = new(languages);
				for (int i = 0; i < value.Count; i++)
				{
					string currentLanguage = value[i].Key;
					if (existingLanguages.Contains(currentLanguage))
					{
						existingLanguages.Remove(currentLanguage);
						continue;
					}
					// Removing the extra language
					value.RemoveAt(i);
					i--;
				}
				// Add leftover languages
				for (var node = existingLanguages.First; node != null; node = node.Next)
					value.Add(new LanguageValuePair(node.Value, ""));
				// Finally, sort the list on order by the languages list
				List<LanguageValuePair> sortedList = new(value.Count);
				for (int i = 0; i < languages.Count; i++)
				{
					string language = languages[i];
					int index = value.FindIndex(pair => pair.Key == language);
					LanguageValuePair currentPair = value[index];
					value.RemoveAt(index);
					sortedList.Add(currentPair);
				}
				sortedList.AddRange(value);
				//Debug.Log($"OLD: {string.Join("\n", value.Select(pair => $"{pair.Key}/{pair.Value}"))}, \nNEW: {string.Join("\n", sortedList.Select(pair => $"{pair.Key}/{pair.Value}"))}");
				value = sortedList;

				// Actually assign the values now lol
				for (int i = 0; i < languageKeys.Count; i++)
				{
					if (languageKeys[i] != key)
						continue;
					// ignoring the keys in the pairs
					IEnumerable<string> unmerged = value.Select(pair => pair.Value);
					languageValues[i] = string.Join(SPLIT_CHAR, unmerged);
					return;
				}
				throw new IndexOutOfRangeException(key);
			}
		}
		public string this[string language, string key]
		{
			get
			{
				List<KeyValuePair<string, string>> pairs = this[key];
				for (int i = 0; i < pairs.Count; i++)
				{
					if (pairs[i].Key != language)
						continue;
					return pairs[i].Value;
				}
				throw new IndexOutOfRangeException(language);
			}
		}

		private void Reset()
		{
			text = GetComponent<TMP_Text>();
			UpdateLanguageList();
		}

		private LanguageValuePair oldValue;
		private void Update()
		{
			if (text.text == oldValue.Value && Languages.CurrentLanguage == oldValue.Key)
				return;
			string newValue = this[Languages.CurrentLanguage, text.text];
			text.text = newValue;
			oldValue = new LanguageValuePair(Languages.CurrentLanguage, newValue);
		}
		private void OnDisable()
		{
			text.text = "";
		}


		internal void UpdateLanguageList()
		{
			HashSet<string> existingLanguage = new(languages);

			// Adding newly added languages
			for (int i = 0; i < Languages.Instance.Count; i++)
				if (existingLanguage.Contains(Languages.Instance[i]) == false)
				{
					languages.Add(Languages.Instance[i]);
					for (int ii = 0; ii < languageKeys.Count; ii++)
						this[languageKeys[ii]] = this[languageKeys[ii]];
				}

			// Removing older languages
			HashSet<string> preExistingLanguages = new(Languages.Instance);
			for (int i = 0; i < languages.Count; i++)
				if (preExistingLanguages.Contains(languages[i]) == false)
				{
					languages.RemoveAt(i);
					i--;
					for (int ii = 0; ii < languageKeys.Count; ii++)
						this[languageKeys[ii]] = this[languageKeys[ii]];
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

	[CustomEditor(typeof(KeyedGlobalizer))]
	public class KeyedGlobalizerrEditor : Editor
	{
		private List<bool> enabled;
		// Headers are split based on the keys, but the actual entries will
		// - act as a key value pair of language and the value of itself.
		public override void OnInspectorGUI()
		{
			KeyedGlobalizer globalizer = (KeyedGlobalizer)target;
			globalizer.UpdateLanguageList();
			globalizer.text = DirtyAuto.Field(target, new("Text"), globalizer.text, true);
			globalizer.ExpectedLines = DirtyAuto.Field(target, new("Expected Lines"), globalizer.ExpectedLines);
			if (enabled == null || enabled.Count != globalizer.languageKeys.Count)
				enabled = new List<bool>(Enumerable.Repeat(true, globalizer.languageKeys.Count));
			EditorGUILayout.Space();
			for (int i = 0; i < globalizer.languageKeys.Count; i++)
			{
				string currentKey = globalizer.languageKeys[i];
				string newKey = EditorGUILayout.DelayedTextField(currentKey);
				if (newKey != currentKey)
				{
					DirtyAuto.SetDirty(globalizer);
					currentKey = newKey;
					globalizer.languageKeys[i] = currentKey;
				}
				Rect copiedRect = GUILayoutUtility.GetLastRect();
				enabled[i] = EditorGUI.Foldout(copiedRect, enabled[i], "");
				if (!enabled[i])
					continue;
				EditorGUI.indentLevel++;
				List<KeyValuePair<string, string>> list = globalizer[currentKey];
				for (int ii = 0; ii < list.Count; ii++)
				{
					string output = EditorGUILayout.DelayedTextField(list[ii].Key, list[ii].Value);
					if (output != list[ii].Value)
					{
						DirtyAuto.SetDirty(globalizer);
						list[ii] = new KeyValuePair<string, string>(list[ii].Key, output);
						globalizer[currentKey] = list;
					}
				}
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.Space();
			Languages.Instance.Editor_OnGUI();
		}
	}
}
#endif
