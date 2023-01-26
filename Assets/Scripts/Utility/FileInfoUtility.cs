namespace B1NARY
{
	using System;
	using System.Reflection;
	using System.IO;
	using UnityEngine;

	public static class FileInfoUtility
	{
		
		public static string NameWithoutExtension(this FileInfo fileInfo) 
			=> fileInfo.Name.Remove(fileInfo.Name.LastIndexOf(fileInfo.Extension));
		
		public static FileInfo GetFile(this DirectoryInfo directoryInfo, string fileName)
		{
			return new FileInfo(directoryInfo.FullName + $"\\{fileName}");
		}
		/// <summary>
		/// Renames <see cref="FileInfo.Name"/> without considering the extension.
		/// </summary>
		public static void Rename(this FileInfo fileInfo, string newName)
		{
			if (fileInfo.NameWithoutExtension() == newName)
				return;
			fileInfo.GetType().GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance)
				.SetValue(fileInfo, fileInfo.Name.Replace(fileInfo.NameWithoutExtension(), newName));
			fileInfo.Refresh();
		}
	}
}