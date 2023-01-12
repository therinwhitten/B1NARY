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
		public static FileInfo Rename(this FileInfo fileInfo, string newName)
		{
			if (fileInfo.Exists)
			{
				fileInfo.MoveTo(fileInfo.FullName.Replace(fileInfo.Name.Remove(fileInfo.Name.LastIndexOf(fileInfo.Extension)), newName));
				return fileInfo;
			}
			return new FileInfo(fileInfo.FullName.Replace(fileInfo.Name.Remove(fileInfo.Name.LastIndexOf(fileInfo.Extension)), newName));
		}
	}
}