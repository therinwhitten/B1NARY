namespace B1NARY.IO
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;


	public class OSFile : OSSystemInfo
	{
		public OSFile(OSPath fullPath) : this(fullPath, OSPath.Empty) { }
		public OSFile(OSPath fullPath, OSPath root) : base(fullPath, root) { }
		public string NameWithoutExtension
		{
			get
			{
				string extension = Extension;
				string name = Name;
				if (string.IsNullOrEmpty(extension))
					return name;
				return name.Remove(name.LastIndexOf(extension));
			}
			set
			{
				string oldName = NameWithoutExtension;
				string fullPath = (string)FullName;
				if (oldName.Length == 0)
				{
					FullName = fullPath + value;
					return;
				}
				int startIndex = fullPath.LastIndexOf(oldName);
				FullName = fullPath.Remove(startIndex, oldName.Length)
					.Insert(startIndex, value);
			}
		}
		public string Extension
		{
			get
			{
				string name = Name;
				int index = name.LastIndexOf(".");
				if (index == -1)
					return string.Empty;
				return name.Substring(index);
			}
			set
			{
				string oldExtension = Extension;
				if (string.IsNullOrEmpty(oldExtension))
				{
					if (!value.StartsWith('.'))
						value = '.' + value;
					FullName += value;
					return;
				}
				if (!value.StartsWith('.'))
					value = '.' + value;
				string fullPath = (string)FullName;
				int startIndex = fullPath.LastIndexOf(oldExtension);
				FullName = fullPath.Remove(startIndex, oldExtension.Length)
					.Insert(startIndex, value);
			}
		}

		public bool Exists => File.Exists(FullName);

		public FileStream Open(FileMode fileMode, FileAccess fileAccess) => 
			File.Open(FullName.Normalized, fileMode, fileAccess);
		public FileStream OpenRead() => Open(FileMode.Open, FileAccess.Read);
		public FileStream Create() => Open(FileMode.Create, FileAccess.Write);
		public string ReadAllText()
		{
			using FileStream stream = OpenRead();
			return new StreamReader(stream).ReadToEnd();
		}
		public string[] ReadAllLines() => ReadAllText().Split('\n');
	}
}
