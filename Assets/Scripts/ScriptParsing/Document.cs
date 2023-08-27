namespace B1NARY.Scripting
{
	using HDConsole.IO;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using UnityEngine;

	public struct Document
	{
		public static OSDirectory CoreFolder => DocumentExplorer.DocumentFolder;
		public static OSDirectory LanguageFolders => DocumentExplorer.LanguagePacks;
		/// <summary>
		/// The ending path
		/// </summary>
		public string VisualPath { get; }
		public OSFile FullPath { get; }
		public string Language { get; }

		public Document(OSFile fullFile)
		{
			this.FullPath = fullFile;

			bool isLanguaged = fullFile.FullPath.Contains(LanguageFolders.FullPath);
			if (isLanguaged)
			{
				Language = fullFile.FullPath.ToString().Substring(LanguageFolders.FullPath.Length + 1);
				Language = Language.Remove(Language.IndexOf(OSPath.DirectorySeparatorChar));
				OSPath removal = OSPath.Combine(LanguageFolders.FullPath, Language);
				VisualPath = fullFile.FullPath - removal;
				return;
			}
			Language = null;
			VisualPath = fullFile.FullPath - CoreFolder.FullPath;
		}
		public Document(string visualPath, string language = null)
		{
			this.VisualPath = visualPath;
			this.Language = language;

			bool isLanguaged = !string.IsNullOrEmpty(language);
			if (isLanguaged)
				FullPath = new OSFile(OSPath.Combine(LanguageFolders.FullPath, Language, VisualPath));
			else
				FullPath = new OSFile(OSPath.Combine(CoreFolder.FullPath, Language, VisualPath));
		}

		public Document GetWithoutLanguage() => new(VisualPath, null);
		public Document GetWithLanguage(string newLanguage) => new(VisualPath, newLanguage);
	}
}