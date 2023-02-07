namespace B1NARY
{
	using System;
	using System.Reflection;
	using System.IO;
	using UnityEngine;

	/// <summary>
	/// Manages <see cref="FileInfo"/> and <see cref="DirectoryInfo"/> as well.
	/// </summary>
	public static class FileInfoUtility
	{
		/// <summary>
		/// Gets the name without the extension.
		/// </summary>
		public static string NameWithoutExtension(this FileInfo fileInfo) 
			=> fileInfo.Name.Remove(fileInfo.Name.LastIndexOf(fileInfo.Extension));
		
		/// <summary>
		/// Gets the file based on name and extension from <paramref name="fileName"/>.
		/// </summary>
		/// <param name="directoryInfo"> The directory of the file. </param>
		/// <param name="fileName"> 
		/// The file name, expected to have the name itself, and the extension. 
		/// </param>
		/// <returns> The file info directed to the <paramref name="fileName"/>. </returns>
		public static FileInfo GetFile(this DirectoryInfo directoryInfo, string fileName)
		{
			return new FileInfo(directoryInfo.FullName + $"\\{fileName}");
		}

		/// <summary>
		/// Renames <see cref="FileInfo.Name"/> without considering the extension.
		/// </summary>
		public static void Rename(this FileInfo fileInfo, string newName)
		{
			string oldName = fileInfo.NameWithoutExtension();
			if (oldName == newName)
				return;
			string newFullPath = fileInfo.FullName.Replace(oldName, newName);
			fileInfo.GetType().GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(fileInfo, new object[] { newFullPath, true });
			fileInfo.Refresh();
		}
	}
}