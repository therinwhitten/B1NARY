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
	}
}