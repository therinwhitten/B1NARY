namespace B1NARY.Scripting
{
	using B1NARY.Globalization;
	using HideousDestructor.DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using UnityEngine;

	public struct Document
	{
		// nullable
		/// <summary>
		/// What language the visual path targets. <see langword="null"/> if it
		/// has no language i.e in the core folder.
		/// </summary>
		public string Language
		{
			get
			{
				string fullPath = FullPath.FullName;
				string packName = DocumentExplorer.LanguagePacks.Name;
				if (!fullPath.Contains(packName))
					return null;
				string result = fullPath.Substring(fullPath.IndexOf(packName) + packName.Length + 1);
				if (fullPath.Contains('/'))
					result = result.Remove(result.IndexOf('/'));
				return result;
			}
		}
		/// <summary>
		/// The path that is purely part of <see cref="DocumentExplorer.DocumentFolder"/>.
		/// </summary>
		public string VisualPath { get; set; }
		/// <summary>
		/// The full path that leads to the document. 
		/// </summary>
		public FileInfo FullPath
		{
			get => new FileInfo($"{DocumentExplorer.DocumentFolder.FullName}/{VisualPath}.txt");
			set => VisualPath = value.FullName.Remove(DocumentExplorer.DocumentFolder.FullName.Length).Replace(".txt", "");
		}


		public Document(string visualPath)
		{
			this.VisualPath = visualPath;
		}
		public Document(FileInfo documentPath)
		{
			VisualPath = null;
			FullPath = documentPath;
		}

		/// <summary>
		/// Creates a new document that has no language variable. Having the actual
		/// document itself existing is not guaranteed.
		/// </summary>
		/// <exception cref="InvalidCastException"/>
		public Document GetWithoutLanguage()
		{
			if (string.IsNullOrEmpty(Language))
				return this;
			string fullPath = FullPath.FullName;
			string packName = $"{DocumentExplorer.LanguagePacks.Name}/{Language}";
			if (!fullPath.Contains(packName))
				throw new InvalidCastException();
			string result = fullPath.Substring(fullPath.IndexOf(packName) + packName.Length + 1);
			return new Document(result);
		}

		/// <summary>
		/// Adds or Replaces to a new language.
		/// </summary>
		/// <param name="language"> The new language to replace to. </param>
		/// <returns></returns>
		public Document GetWithLanguage(string language)
		{
			string currentLanguage = this.Language;
			if (currentLanguage == language)
				return this;
			string visualPath = VisualPath;
			if (!string.IsNullOrEmpty(currentLanguage))
				visualPath = GetWithoutLanguage().VisualPath;
			return new Document($"{DocumentExplorer.LanguagePacks.Name}/{language}/{visualPath}");
		}
	}
}