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
				string oldName = fileInfo.FullName;
				fileInfo.CopyTo(newFullName);
				if (!copy && File.Exists(oldName))
					File.Delete(oldName);
				return;
			}
			using (var stream = new StreamWriter(fileInfo.Open(FileMode.CreateNew)))
			{
				stream.Write("sex");
				stream.Flush();
			}
			Rename(fileInfo, newName, copy);
		}
	}
}