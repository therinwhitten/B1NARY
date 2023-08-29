namespace B1NARY.Scripting
{
	using B1NARY.DataPersistence;
	using HDConsole.IO;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using UnityEngine;

	// TODO: Play some starting scripts or limit them via language!
	public class DocumentExplorer
	{
		/// <summary>
		/// The documents folder that has all the playable scripts the system
		/// can access.
		/// </summary>
		public static OSDirectory DocumentFolder { get; } = SaveSlot.StreamingAssets.GetSubdirectories("Docs");
		/// <summary>
		/// All the documents that are divided into languages, located within the
		/// original <see cref="DocumentFolder"/>.
		/// </summary>
		public static OSDirectory LanguagePacks { get; } = DocumentFolder.GetSubdirectories("Language Packs");

		public List<Document> CoreDocuments { get; }
		public Dictionary<string, List<Document>> LanguagedDocuments { get; }

		public DocumentExplorer()
		{
			CoreDocuments = new List<Document>();
			LanguagedDocuments = new Dictionary<string, List<Document>>();

			List<Document> allDocuments = new();
			RecursivelyGetFiles(DocumentFolder);
			for (int i = 0; i < allDocuments.Count; i++)
			{
				Document document = allDocuments[i];
				string language = document.Language;
				if (string.IsNullOrEmpty(language))
				{
					CoreDocuments.Add(document);
					continue;
				}
				if (!LanguagedDocuments.TryGetValue(language, out List<Document> languagedDocuments))
					languagedDocuments = LanguagedDocuments[language] = new List<Document>();
				languagedDocuments.Add(document);
			}

			void RecursivelyGetFiles(OSDirectory currentPath)
			{
				OSFile[] files = currentPath.GetFiles();
				for (int i = 0; i < files.Length; i++)
				{
					if (files[i].Extension != ".txt")
						continue;
					allDocuments.Add(new Document(files[i]));
				}
				OSDirectory[] directories = currentPath.GetDirectories();
				for (int i = 0; i < directories.Length; i++)
					RecursivelyGetFiles(directories[i]);
			}
		}

		public Document GetDocumentFromVisual(string visualPath)
		{
			visualPath = OSExtension.AddExtension(visualPath, ".txt");
			string currentLanguage = PlayerConfig.Instance.language.Value;
			// First try to get the language version
			Document target = new(visualPath, currentLanguage);
			if (target.FullPath.Exists)
				return target;
			// trying to get the core version instead.
			target = new Document(visualPath);
			if (target.FullPath.Exists)
			{
				Debug.LogWarning($"Document '{visualPath}' doesn't lead to the specified document; using the core version instead.");
				return target;
			}
			// ripperonis
			throw new IndexOutOfRangeException($"'{visualPath}' does not lead to a valid '{currentLanguage}' language or core document!");
		}
	}
}