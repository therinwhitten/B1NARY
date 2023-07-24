namespace B1NARY
{
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
		public static bool ChangingNames { get; private set; } = false;

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
					string currentLanguage = Languages.Instance[i];
					if (currentLanguage == language)
						continue;
					int index = ScriptHandler.Instance.documentWatcher.CurrentNode.GlobalIndex;
					_characterNames[currentLanguage] = new Lazy<string>(() => GetAlternateName(language, index));
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
			if (Languages.CurrentLanguage == targetLanguage)
				return _characterNames[targetLanguage].Value;
			ChangingNames = true;
			Document newDocument = new Document(ScriptHandler.Instance.document.ReadFile);
			newDocument = newDocument.GetWithLanguage(targetLanguage);
			FileInfo info = newDocument.FullPath;
			if (!info.Exists)
				info = newDocument.GetWithoutLanguage().FullPath;
			ScriptHandler.config.stopOnAllLines = true;
			var document = new ScriptDocument(ScriptHandler.config, info);
			IDocumentWatcher watcher = document.StartAtLine(documentIndex);
			watcher.NextNode(out var line); // changes line here
			Debug.Log(line);
			ChangingNames = false;
			ScriptHandler.config.stopOnAllLines = false;
			Debug.Log(_characterNames[targetLanguage].Value);
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