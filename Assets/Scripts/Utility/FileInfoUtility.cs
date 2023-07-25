﻿namespace B1NARY
{
	using System;
	using System.Reflection;
	using System.IO;
	using UnityEngine;
	using System.Linq;
	using System.Collections.Generic;

	public static class DirectoryInfoUtility
	{
		/// <summary>
		/// Creates a new file, increases number incrementally if there are any
		/// files that are existant in the name.
		/// </summary>
		/// <param name="source"> The source of the new file. </param>
		/// <param name="fileName"> The file name with the extension. </param>
		/// <param name="alwaysIncludeNumber"> If the filename should always include the number at the end. </param>
		/// <returns> A fileinfo that doesn't have any files to it. </returns>
		public static FileInfo GetFileIncremental(this DirectoryInfo source, string fileName, bool alwaysIncludeNumber = false)
		{
			HashSet<string> otherNames = new(source.EnumerateFiles().Select(fileInfo => fileInfo.Name));
			if (alwaysIncludeNumber || otherNames.Contains(fileName))
				for (int i = alwaysIncludeNumber ? 0 : 1; true; i++)
				{
					string incrementalName = fileName.Insert(fileName.LastIndexOf('.'), $"_{i}");
					if (otherNames.Contains(incrementalName))
						continue;
					return GetFile(source, incrementalName);
				}
			return GetFile(source, fileName);
		}

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



		public static DirectoryInfo GetSubdirectory(this DirectoryInfo directoryInfo, string directoryName)
		{
			if (!directoryInfo.Exists)
				throw new IOException($"Directory '{directoryInfo.FullName}' does not exist!");
			var newPath = new DirectoryInfo($"{directoryInfo.FullName}\\{directoryName}");
			if (!newPath.Exists)
			{
				newPath.Create();
				newPath.Refresh();
			}
			return newPath;
		}
		public static DirectoryInfo GetPathing(this DirectoryInfo directoryInfo, params string[] subDirectories)
		{
			for (int i = 0; i < subDirectories.Length; i++)
				directoryInfo = GetSubdirectory(directoryInfo, subDirectories[i]);
			return directoryInfo;
		}
	}
	/// <summary>
	/// Manages <see cref="FileInfo"/> and <see cref="DirectoryInfo"/> as well.
	/// </summary>
	public static class FileInfoUtility
	{
		public static FileStream OpenStream(this FileInfo info, FileMode mode, FileAccess fileAccess)
		{
			if (Environment.OSVersion.Platform != PlatformID.Unix)
				return info.Open(mode, fileAccess);
			string path = info.FullName;
			path = path.Replace('/', '\\');
			return File.Open(path, mode, fileAccess);
		}
		/// <summary>
		/// Gets the name without the extension.
		/// </summary>
		public static string NameWithoutExtension(this FileInfo fileInfo) 
			=> fileInfo.Name.Remove(fileInfo.Name.LastIndexOf(fileInfo.Extension));
		

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