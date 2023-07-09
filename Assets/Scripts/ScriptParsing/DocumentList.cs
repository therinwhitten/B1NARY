namespace B1NARY.Scripting
{
	using HideousDestructor.DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using UnityEngine;

	public sealed class DocumentList : List<FileInfo>
	{
		public static string ToVisual(string fullPath) =>
			fullPath.Replace(BasePath, "").Replace(".txt", "");
		public static FileInfo FromVisualLanguage(string visualPath)
		{
			string fullPath = $"{SerializableSlot.StreamingAssets.FullName}/Docs/Language Packs/{PlayerConfig.Instance.language.Value}/{visualPath}.txt";
			FileInfo output = new FileInfo(fullPath);
			if (!output.Exists)
			{
				output = FromVisual(visualPath);
				Debug.Log($"Language of '{PlayerConfig.Instance.language.Value}' does not lead to a document '{fullPath}'. \nusing '{output.FullName}' instead.");
			}
			return output;
		}
		public static FileInfo FromVisual(string visualPath) =>
			new FileInfo($"{SerializableSlot.StreamingAssets.FullName}/Docs/{visualPath}.txt");

		public static DirectoryInfo DocumentFolder { get; } = SerializableSlot.StreamingAssets.GetOrCreateSubDirectory("Docs");
		public static DirectoryInfo LanguagePacks { get; } = DocumentFolder.GetOrCreateSubDirectory("Language Packs");
		public static string BasePath => $"{Application.streamingAssetsPath.Replace('/', '\\')}\\Docs\\";
		private WeakReference visualNames;
		public DocumentList() : base()
		{
			RecursivelyGetFiles(DocumentFolder);
		}
		public DocumentList(string language) : base()
		{
			DirectoryInfo documentFolderTarget = DocumentFolder;
			if (LanguagePacks.Exists)
			{
				DirectoryInfo languageFolder = LanguagePacks.GetOrCreateSubDirectory(language);
				if (languageFolder.Exists)
					documentFolderTarget = languageFolder;
			}
			RecursivelyGetFiles(documentFolderTarget);
		}
		/// <summary>
		/// Uses recursion to get all the documents as a full path, including
		/// the drive and such.
		/// </summary>
		/// <param name="currentPath"> The directory to interact with. </param>
		/// <returns> 
		/// All the file paths that start with .txt within the directory.
		/// </returns>
		private void RecursivelyGetFiles(DirectoryInfo currentPath)
		{
			AddRange(currentPath.EnumerateFiles().Where(path => path.Extension == ".txt"));
			IEnumerable<DirectoryInfo> directories = currentPath.EnumerateDirectories();
			if (directories.Any())
			{
				using (IEnumerator<DirectoryInfo> enumerator = directories.GetEnumerator())
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.Name.Contains("Language Packs"))
							continue;
						RecursivelyGetFiles(enumerator.Current);
					}
			}
		}
		public string[] AsVisual()
		{
			if (visualNames != null && visualNames.IsAlive)
				return (string[])visualNames.Target;
			string[] output = new string[Count];
			for (int i = 0; i < output.Length; i++)
			{
				output[i] = ToVisual(this[i].FullName);
			}
			visualNames = new WeakReference(output);
			return output;
		}
	}
}