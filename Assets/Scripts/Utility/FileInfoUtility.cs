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
		public static void Rename(this FileInfo fileInfo, string newName, bool copy = false)
		{
			fileInfo.Refresh();
			if (fileInfo.Exists)
			{
				string newFullName = fileInfo.FullName.Replace(fileInfo.NameWithoutExtension(), newName);
				string oldName = fileInfo.FullName;
				if (oldName == newFullName)
					return;
				fileInfo.CopyTo(newFullName, true);
				if (!copy && File.Exists(oldName))
					File.Delete(oldName);
				return;
			}
			fileInfo.GetType().GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance)
				.SetValue(fileInfo, fileInfo.Name.Replace(fileInfo.NameWithoutExtension(), newName));
		}
	}
}