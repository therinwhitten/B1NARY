namespace B1NARY.Scripting
{
	using B1NARY.DataPersistence;
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
		public static DirectoryInfo DocumentFolder => SaveSlot.StreamingAssets.GetSubdirectory("Docs");
		/// <summary>
		/// All the documents that are divided into languages, located within the
		/// original <see cref="DocumentFolder"/>.
		/// </summary>
		public static DirectoryInfo LanguagePacks => DocumentFolder.GetSubdirectory("Language Packs");
		

		public List<Document> CoreDocuments { get; }
		public Dictionary<string, List<Document>> LanguagedDocuments { get; }

		private readonly StringBuilder concerns = new();

		public DocumentExplorer()
		{
			CoreDocuments = new List<Document>();
			LanguagedDocuments = new Dictionary<string, List<Document>>();

			List<Document> allDocuments = new();
			RecursivelyGetFiles(ref allDocuments, DocumentFolder);
			for (int i = 0; i < allDocuments.Count; i++)
			{
				Document document = allDocuments[i];
				string language = document.Language;
				if (!string.IsNullOrEmpty(language))
				{
					if (!LanguagedDocuments.TryGetValue(language, out List<Document> languagedDocuments))
						languagedDocuments = LanguagedDocuments[language] = new List<Document>();
					languagedDocuments.Add(document);
				}
				else
					CoreDocuments.Add(document);
			}
		}
		public DocumentExplorer(string defaultLanguage) : this()
		{
			if (!LanguagedDocuments.TryGetValue(defaultLanguage, out List<Document> sex))
			{
				concerns.AppendLine($"Although the selected language is '{defaultLanguage}', it couldn't find the folder containing it!");
				return;
			}
			List<Document> comparingTo = new(sex);
			List<Document> newDocuments = new(CoreDocuments.Count);
			for (int i = 0; i < CoreDocuments.Count; i++)
			{
				Document newDoc = CoreDocuments[i].GetWithoutLanguage();
				for (int ii = 0; ii < comparingTo.Count; ii++)
					if (newDoc.VisualPath == comparingTo[ii].VisualPath)
					{
						newDocuments.Add(newDoc);
						continue;
					}
				newDocuments.Add(CoreDocuments[i]);
				concerns.AppendLine($"Document '{CoreDocuments[i].VisualPath}' doesn't have a {defaultLanguage} variation.");
			}
		}
		/// <summary>
		/// Uses recursion to get all the documents as a full path, including
		/// the drive and such.
		/// </summary>
		/// <param name="currentPath"> The directory to interact with. </param>
		/// <returns> 
		/// All the file paths that start with .txt within the directory.
		/// </returns>
		private void RecursivelyGetFiles(ref List<Document> documents, DirectoryInfo currentPath)
		{
			FileInfo[] files = currentPath.GetFiles();
			for (int i = 0; i < files.Length; i++)
			{
				if (!files[i].Extension.Contains("txt"))
				{
					concerns.AppendLine($"Although the system filters out non-text files, it is recommended to remove '{files[i].FullName}' from the folders to slightly increase performance.");
					continue;
				}
				documents.Add(new Document(files[i]));
			}
			DirectoryInfo[] directories = currentPath.GetDirectories();
			if (directories.Length <= 0)
				return;
			for (int i = 0; i < directories.Length; i++)
				RecursivelyGetFiles(ref documents, directories[i]);
		}


		public void PrintConcerns()
		{
			if (concerns.Length <= 0)
				return;
			Debug.LogWarning($"Here are some concerns: \n{concerns}");
			concerns.Clear();
			concerns.Capacity = 0;
		}

		public Document GetFromVisual(in string visualPath)
		{
			Document comparingTo = new(visualPath);
			string currentLanguage = PlayerConfig.Instance.language.Value;
			// First try to get the language version
			Document target = comparingTo.GetWithLanguage(currentLanguage);
			if (target.FullPath.Exists)
				return target;
			// trying to get the core version instead.
			target = comparingTo.GetWithoutLanguage();
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