namespace B1NARY
{
	using System;
	using System.IO;
	using System.Text;

	public static class DirectoryInfoUtility
	{
		public static DirectoryInfo GetOrCreateSubDirectory(this DirectoryInfo directoryInfo, string directoryName)
		{
			if (!directoryInfo.Exists)
				throw new IOException($"Directory '{directoryInfo.FullName}' does not exist!");
			var newPath = new DirectoryInfo($"{directoryInfo.FullName}/{directoryName}");
			if (!newPath.Exists)
			{
				newPath.Create();
				newPath.Refresh();
			}
			return newPath;
		}
	}
}