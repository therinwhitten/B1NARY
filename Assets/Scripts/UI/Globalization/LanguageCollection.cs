namespace B1NARY.UI.Globalization
{
	using HideousDestructor.DataPersistence;
	using OVSXmlSerializer;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using UnityEngine;

	public class Languages : List<string>
	{
		private static FileInfo LanguagesInfo { get; } = SerializableSlot.StreamingAssets.GetFile("languages.xml");
		private static Languages m_instance;
		public static Languages Instance
		{
			get
			{
				if (m_instance is null)
				{
					if (LanguagesInfo.Exists)
						m_instance = XmlSerializer<Languages>.Default.Deserialize(LanguagesInfo);
					else
						m_instance = new Languages();
				}
				return m_instance;
			}
		}
		public void Save()
		{
			using (var stream = LanguagesInfo.Open(FileMode.Create))
				XmlSerializer<Languages>.Default.Serialize(stream, this, "Languages");
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.UI.Globalization.Editor
{
	using B1NARY.UI.Colors.Editor;
	using UnityEditor;
	using UnityEngine;

	// Too lazy, maybe later
	public class LanguageEditor : EditorWindow
	{


		private static readonly Vector2Int defaultMinSize = new Vector2Int(300, 350);
		//[MenuItem("B1NARY/Language Selection", priority = 1)]
		public static void ShowWindow()
		{
			// Get existing open window or if none, make a new one:
			ColorFormatWindow window = GetWindow<ColorFormatWindow>();
			window.titleContent = new GUIContent("Language Selection Editor");
			window.minSize = defaultMinSize;
			window.Show();
		}

		public void OnGUI()
		{

		}
	}
}
#endif
/*
using System;
using System.Collections.Generic;
using System.Collections;

[Serializable]
public class LanguageCollection : IEnumerable<KeyValuePair<string, string>>
{
	/// <summary>
	/// Messing with these values are not recommend and will cause synchronisation
	/// issues! These are only exposed to please the unity gods.
	/// </summary>
	public List<string> keys, values;

	public LanguageCollection()
	{
		keys = new List<string>();
		values = new List<string>();
	}
	public string this[string key]
	{
		get => values[keys.IndexOf(key)];
		set => values[keys.IndexOf(key)] = value;
	}
	public string this[int index]
	{
		get => values[index];
		set => values[index] = value;
	}
	internal void AddLanguage(string key)
	{
		keys.Add(key.Trim().ToLower());
		values.Add(string.Empty);
	}
	internal void RemoveLanguage(string key)
	{
		key = key.Trim().ToLower();
		int index = key.IndexOf(key);
		keys.RemoveAt(index);
		values.RemoveAt(index);
	}
	public bool Contains(string key) => keys.Contains(key);

	IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
	{
		for (int i = 0; i < keys.Count; i++)
			yield return new KeyValuePair<string, string>(keys[i], values[i]);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		for (int i = 0; i < keys.Count; i++)
			yield return new KeyValuePair<string, string>(keys[i], values[i]);
	}
}
*/