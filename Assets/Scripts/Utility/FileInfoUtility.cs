namespace B1NARY
{
	using System;
	using System.IO;

	public static class FileInfoUtility
	{
		public static FileInfo GetFile(this DirectoryInfo directoryInfo, string fileName)
		{
			return new FileInfo(directoryInfo.FullName + $"\\{fileName}");
		}
		public static void Rename(this FileInfo fileInfo, string newName, bool copy = false)
		{
			if (fileInfo.Exists && !copy)
				fileInfo.MoveTo(fileInfo.FullName.Replace(fileInfo.Name.Remove(fileInfo.Name.LastIndexOf(fileInfo.Extension)), newName));
			else
				fileInfo.CopyTo(fileInfo.FullName.Replace(fileInfo.Name.Remove(fileInfo.Name.LastIndexOf(fileInfo.Extension)), newName));
		}
	}
}