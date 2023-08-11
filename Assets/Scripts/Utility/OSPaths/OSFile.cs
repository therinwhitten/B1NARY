namespace B1NARY.IO
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;


	public class OSFile : OSFolderEntry
	{
		public OSFile(OSPath fullPath) : this(fullPath, OSPath.Empty) { }
		public OSFile(OSPath fullPath, OSPath root) : base(fullPath, root) { }
		public string NameWithoutExtension
		{
			get
			{
				return Name.Remove(Name.LastIndexOf(Extension));
			}
		}
		public string Extension
		{
			get
			{
				string name = Name;
				int index = name.LastIndexOf(".");
				if (index == -1)
					return null;
				return name.Substring(index);
			}
		}

		public bool Exists => File.Exists(FullPath);
	}
}
