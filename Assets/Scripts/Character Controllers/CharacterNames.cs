namespace B1NARY
{
	using B1NARY.CharacterManagement;
	using B1NARY.Scripting;
	using B1NARY.UI.Globalization;
	using OVSXmlSerializer;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;
	using UnityEngine;

#warning TODO: With it using lazy classes, it may be helpful in the future to simply convert them to normal variables in .NET 5, around there.
	public class CharacterNames : IXmlSerializable
	{
		public static CharacterNames ChangingNameOf { get; private set; } = null;
		public static bool ChangingNames => ChangingNameOf != null;

		private Dictionary<string, Lazy<string>> _characterNames;
		public CharacterNames()
		{
			_characterNames = new Dictionary<string, Lazy<string>>();
			for (int i = 0; i < Languages.Instance.Count; i++)
				_characterNames.Add(Languages.Instance[i], new Lazy<string>(() => ""));
		}

		public string this[string language]
		{
			get => _characterNames[language].Value;
			set
			{
				_characterNames[language] = new Lazy<string>(() => value);
				// On value change, also reflect on the others
				if (ChangingNames)
					return;
				for (int i = 0; i < Languages.Instance.Count; i++)
				{
					string targetLanguage = Languages.Instance[i];
					if (targetLanguage == language)
						continue;
					int index = ScriptHandler.Instance.documentWatcher.CurrentNode.GlobalIndex;
					_characterNames[targetLanguage] = new Lazy<string>(() => GetAlternateName(targetLanguage, index));
				}
			}
		}

		public string CurrentName
		{
			get => this[Languages.CurrentLanguage];
			set => this[Languages.CurrentLanguage] = value;
		}

		private string GetAlternateName(string targetLanguage, int documentIndex)
		{
			ChangingNameOf = this;
			Document newDocument = new Document(ScriptHandler.Instance.document.ReadFile);
			newDocument = newDocument.GetWithLanguage(targetLanguage);
			FileInfo info = newDocument.FullPath;
			if (!info.Exists)
			{
				Debug.Log($"{info.FullName} doesn't exist, using core path instead");
				info = newDocument.GetWithoutLanguage().FullPath;
			}

			ScriptHandler.config.stopOnAllLines = true;
			var document = new ScriptDocument(ScriptHandler.config, info);
			IDocumentWatcher watcher = document.StartAtLine(documentIndex);
			watcher.NextNode(out _); // changes line here
			ScriptHandler.config.stopOnAllLines = false;
			ChangingNameOf = null;
			return _characterNames[targetLanguage].Value;
		}

		bool IXmlSerializable.ShouldWrite => true;
		void IXmlSerializable.Read(XmlNode value)
		{
			_characterNames.Clear();
			for (int i = 0; i < value.ChildNodes.Count; i++)
				_characterNames[value.ChildNodes[i].Name] = new Lazy<string>(() => value.ChildNodes[i].InnerText);
		}

		void IXmlSerializable.Write(XmlDocument sourceDocument, XmlNode currentNode)
		{
			using (var enumerator = _characterNames.GetEnumerator())
				while (enumerator.MoveNext())
				{
					var element = sourceDocument.CreateElement(enumerator.Current.Key);
					element.InnerText = enumerator.Current.Value.Value;
					currentNode.AppendChild(element);
				}
		}
	}
}