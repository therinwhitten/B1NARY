﻿namespace B1NARY.UI.Globalization
{
	using B1NARY.DataPersistence;
	using OVSSerializer.IO;
	using HDConsole;
	using OVSSerializer;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;
	using UnityEngine;

	public class Languages : List<string>, IOVSXmlSerializable
	{
		[return: CommandsFromGetter]
		private static HDCommand[] GetHDCommands() => new HDCommand[]
		{
			new HDCommand("lang_current", (args) =>
			{
				if (args.Length <= 0)
				{
					HDConsole.WriteLine(PlayerConfig.Instance.language.Value);
					return;
				}
				if (!Instance.Contains(args[0]))
					throw new InvalidDataException(args[0]);
				PlayerConfig.Instance.language.Value = args[0];
			}) { description = "Gets (into console) or Sets the current language.", optionalArguments = new string[] { "language" } },
		};
		private static OSFile LanguagesInfo => SaveSlot.StreamingAssets.GetFile("languages.xml");
		public static string CurrentLanguage { get => PlayerConfig.Instance.language; set => PlayerConfig.Instance.language.Value = value; }
		private static Languages m_instance;
		public static Languages Instance
		{
			get
			{
				if (m_instance is null)
				{
					if (LanguagesInfo.Exists)
						using (FileStream stream = LanguagesInfo.OpenRead())
							m_instance = OVSXmlSerializer<Languages>.Shared.Deserialize(stream);
					m_instance ??= new Languages();
				}
				return m_instance;
			}
		}

		public void Save()
		{
			using FileStream stream = LanguagesInfo.Create();
			OVSXmlSerializer<Languages>.Shared.Serialize(stream, this);
		}

#if UNITY_EDITOR
		private bool guiOpen = false;
		public void Editor_OnGUI()
		{
			if (guiOpen = UnityEditor.EditorGUILayout.BeginFoldoutHeaderGroup(guiOpen, "Languages"))
			{
				UnityEditor.EditorGUI.indentLevel++;
				for (int i = 0; i < Count; i++)
				{
					Rect fullRect = GUILayoutUtility.GetRect(Screen.width, 20f);
					fullRect = UnityEditor.EditorGUI.IndentedRect(fullRect);

					Rect languageRect = fullRect;
					languageRect.width /= 2f;
					string newLanguage = UnityEditor.EditorGUI.DelayedTextField(languageRect, this[i]);
					if (newLanguage != this[i])
					{
						this[i] = newLanguage;
						Save();
					}

					Rect removeButtonRect = fullRect;
					removeButtonRect.xMin = languageRect.xMax + 2f;
					removeButtonRect.xMax -= 3f;
					if (GUI.Button(removeButtonRect, "Remove"))
					{
						RemoveAt(i);
						i--;
						Save();
					}
				}
				if (GUILayout.Button("Add New"))
				{
					Add("English");
					Save();
				}
				UnityEditor.EditorGUI.indentLevel--;
			}
			UnityEditor.EditorGUILayout.EndFoldoutHeaderGroup();
		}
#endif

		bool IOVSXmlSerializable.ShouldWrite => true;
		void IOVSXmlSerializable.Read(XmlNode value)
		{
			for (int i = 0; i < value.ChildNodes.Count; i++)
			{
				Add(value.ChildNodes[i].Attributes[0].Value);
			}
		}
		void IOVSXmlSerializable.Write(XmlNode currentNode)
		{
			for (int i = 0; i < Count; i++)
			{
				XmlElement element = currentNode.OwnerDocument.CreateElement("Language");
				XmlAttribute attribute = currentNode.OwnerDocument.CreateAttribute("data");
				attribute.Value = this[i];
				element.Attributes.Append(attribute);
				currentNode.AppendChild(element);
			}
		}
	}
}
#if UNITY_EDITOR
namespace B1NARY.UI.Globalization.Editor
{
	using B1NARY.UI.Colors.Editor;
	using UnityEditor;
	using UnityEngine;

	public class LanguageEditor : EditorWindow
	{


		private static readonly Vector2Int defaultMinSize = new(300, 350);
		[MenuItem("B1NARY/Language Selection", priority = 1)]
		public static void ShowWindow()
		{
			// Get existing open window or if none, make a new one:
			LanguageEditor window = GetWindow<LanguageEditor>();
			window.titleContent = new GUIContent("Language Selection Editor");
			window.minSize = defaultMinSize;
			window.Show();
		}

		public void OnGUI()
		{
			Languages.Instance.Editor_OnGUI();
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