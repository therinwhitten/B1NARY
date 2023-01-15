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
			if (fileInfo.Exists)
			{
				string newFullName = fileInfo.FullName.Replace(fileInfo.Name.Remove(fileInfo.Name.LastIndexOf(fileInfo.Extension)), newName);
				if (!copy || File.Exists(newFullName))
					fileInfo.MoveTo(newFullName);
				else
					fileInfo.CopyTo(newFullName);
				return;
			}
			using (var stream = fileInfo.OpenWrite())
			{
				stream.Write(new byte[0], 0, 0);
				stream.Flush();
			}
			Rename(fileInfo, newName, copy);
		}
	}
}