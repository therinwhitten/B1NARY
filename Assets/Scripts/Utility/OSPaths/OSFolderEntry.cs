namespace B1NARY.IO
{
	using System;
	using static System.IO.Path;

	public abstract class OSFolderEntry
	{
		protected OSFolderEntry(OSPath fullPath, OSPath root)
		{
			FullPath = fullPath;
			Root = root;
		}

		public OSPath FullPath { get; }
		public string Name
		{
			get
			{
				string fullName = FullPath.ToString();
				string[] splitName = fullName.Split(PathSeparator);
				if (string.IsNullOrEmpty(splitName[^1]))
					return splitName[^2];
				return splitName[^1];
			}
		}
		public OSPath Path => FullPath - Root;
		protected OSPath Root { get; }
	}
}
