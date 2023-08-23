namespace HDConsole.IO
{
	using System;
	using static System.IO.Path;

	public abstract class OSSystemInfo
	{
		protected OSSystemInfo(OSPath fullPath, OSPath root)
		{
			FullName = fullPath;
			Root = root;
		}

		public OSPath FullName { get; protected set; }
		public string Name
		{
			get
			{
				string fullName = FullName.ToString();
				string[] splitName = fullName.Split(PathSeparator);
				if (string.IsNullOrEmpty(splitName[^1]))
					return splitName[^2];
				return splitName[^1];
			}
			set
			{
				string oldName = Name;
				string fullPath = (string)FullName;
				if (Name.Length == 0)
				{
					if (fullPath.EndsWith(PathSeparator))
						FullName = fullPath.Insert(fullPath.Length - 1, value);
					else
						FullName = fullPath + value;
					return;
				}
				int startIndex = fullPath.LastIndexOf(oldName);
				FullName = fullPath.Remove(startIndex, oldName.Length)
					.Insert(startIndex, value);
			}
		}
		public OSPath Path => FullName - Root;
		protected OSPath Root { get; }
	}
}
