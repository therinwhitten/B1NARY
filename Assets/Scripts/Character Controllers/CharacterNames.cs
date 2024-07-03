namespace B1NARY
{
	using B1NARY.CharacterManagement;
	using B1NARY.DataPersistence;
	using OVSSerializer.IO;
	using B1NARY.Scripting;
	using B1NARY.UI.Globalization;
	using OVSSerializer;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;
	using UnityEngine;

	public class CharacterNames : IOVSXmlSerializable
	{
		public static LanguagedName ChangingNameOf { get; private set; } = null;
		public static bool ChangingNames => ChangingNameOf != null;

		private Dictionary<string, LanguagedName> _characterNames = new();
		public CharacterNames()
		{
			for (int i = 0; i < Languages.Instance.Count; i++)
				_characterNames.Add(Languages.Instance[i], new LanguagedName(-1, Languages.Instance[i], ""));
		}

		public string this[string language]
		{
			get => _characterNames[language].Name;
			set
			{
				_characterNames[language] = new LanguagedName(-1, language, value);
				// On value change, also reflect on the others
				if (ChangingNames)
					return;
				if (!ScriptHandler.HasInstance || ScriptHandler.Instance.documentWatcher == null)
					return;
				int index = -1;
				try { index = ScriptHandler.Instance.documentWatcher.CurrentNode.GlobalIndex; }
				catch { /*Debug.LogError($"Failed to retrieve languaged name '{CurrentName}' for '{language}'!"); */ throw; }
				for (int i = 0; i < Languages.Instance.Count; i++)
				{
					string targetLanguage = Languages.Instance[i];
					if (targetLanguage == language)
						continue;
					_characterNames[targetLanguage] = new LanguagedName(index, targetLanguage);
				}
			}
		}

		public string CurrentName
		{
			get => this[Languages.CurrentLanguage];
			set => this[Languages.CurrentLanguage] = value;
		}

		bool IOVSXmlSerializable.ShouldWrite => true;
		void IOVSXmlSerializable.Read(XmlNode value)
		{
			_characterNames.Clear();
			for (int i = 0; i < value.ChildNodes.Count; i++)
			{
				string language = value.ChildNodes[i].Name;
				LanguagedName name = new(-1, language, value.ChildNodes[i].InnerText);
				ChangingNameOf = name;
				_characterNames[language] = name;
				ChangingNameOf = null;
			}
		}

		void IOVSXmlSerializable.Write(XmlNode currentNode)
		{
			using var enumerator = _characterNames.GetEnumerator();
			while (enumerator.MoveNext())
			{
				var element = currentNode.OwnerDocument.CreateElement(enumerator.Current.Key);
				element.InnerText = enumerator.Current.Value.Name;
				currentNode.AppendChild(element);
			}
		}

		public record LanguagedName(int DocumentIndex, string Language)
		{
			public string Name
			{
				get
				{
					if (m_name is null)
					{
						if (ChangingNames)
							throw new InvalidOperationException("Tried to define name while it was trying to define the name!");
						ChangingNameOf = this;
						Document newDocument = new(ScriptHandler.Instance.document.ReadFile);
						newDocument = newDocument.GetWithLanguage(Language);
						OSFile info = newDocument.FullPath;
						if (!info.Exists)
						{
							Debug.Log($"{info.FullPath} doesn't exist, using core path instead for replacement of name");
							info = newDocument.GetWithoutLanguage().FullPath;
						}

						ScriptHandler.config.stopOnAllLines = true;
						var document = new ScriptDocument(ScriptHandler.config, info);
						IDocumentWatcher watcher = document.StartAtLine(DocumentIndex);
						watcher.NextNode(out _); // changes line here
						ScriptHandler.config.stopOnAllLines = false;
						ChangingNameOf = null;
					}
					return m_name;
				}
				set => m_name = value;
			}
			private string m_name = null;

			public LanguagedName(int DocumentIndex, string Language, string Name) : this(DocumentIndex, Language)
			{
				this.Name = Name;
			}
		}
	}
}