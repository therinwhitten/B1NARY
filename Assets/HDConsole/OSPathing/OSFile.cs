namespace HDConsole.IO
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using SysPath = System.IO.Path;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Contains metadata about the file
	/// </summary>
	public sealed class OSFile : OSSystemInfo
	{
		public static explicit operator FileInfo(OSFile file) => new FileInfo(file.FullPath);
		public static explicit operator OSFile(FileInfo file) => new OSFile(file);
		public OSFile(OSPath fullPath) : base(fullPath) { }
		public OSFile(OSFile source) : base(source.FullPath) { }
		public OSFile(FileInfo info) : base(info.FullName) { }

		/// <summary>
		/// The name of the file; Extension included. Use <see cref="Rename"/>
		/// to also change the file itself located on the disk.
		/// </summary>
		public override string Name
		{
			get => SysPath.GetFileName(FullPath);
			set
			{
				string nameToReplace = Name;
				string fullPath = FullPath.ToString();
				FullPath = fullPath.Remove(fullPath.IndexOf(nameToReplace)) + value;
			}
		}
		public string Extension
		{
			get => SysPath.GetExtension(FullPath);
			set
			{
				string extensionToReplace = Extension;
				string fullPath = FullPath.ToString();
				if (string.IsNullOrEmpty(extensionToReplace))
					FullPath = fullPath + value;
				else
					FullPath = fullPath.Remove(fullPath.IndexOf(extensionToReplace)) + value;
			}
		}
		public string NameWithoutExtension
		{
			get => SysPath.GetFileNameWithoutExtension(FullPath);
			set
			{
				string nameToReplace = NameWithoutExtension;
				string fullPath = FullPath.ToString();
				int index = fullPath.IndexOf(nameToReplace);
				FullPath = fullPath.Remove(index, nameToReplace.Length).Insert(index, value);
			}
		}
		public override bool Exists
		{
			get => File.Exists(FullPath);
			set
			{
				if (Exists == value)
					return;
				if (value)
					File.WriteAllText(FullPath, "");
				else
					Delete();
			}
		}

		#region Reading/Writing
		public FileStream Open(FileMode fileMode, FileAccess fileAccess) => 
			File.Open(FullPath.Normalized, fileMode, fileAccess);
		public FileStream Append() => Open(FileMode.Append, FileAccess.Write);
		public FileStream OpenRead() => Open(FileMode.Open, FileAccess.Read);
		public FileStream Create() => Open(FileMode.Create, FileAccess.Write);
		public string ReadAllText()
		{
			using FileStream stream = OpenRead();
			return new StreamReader(stream).ReadToEnd();
		}
		public string[] ReadAllLines() => ReadAllText().Split('\n');

		public void WriteAllText(string text)
		{
			using StreamWriter writer = new(Create());
			writer.Write(text);
		}
		public void WriteAllLines(string[] lines) => WriteAllText(string.Join('\n', lines));
		public void WriteAllLines(IEnumerable<string> lines) => WriteAllText(string.Join('\n', lines));

		public void AppendAllText(string text)
		{
			using StreamWriter writer = new(Append());
			writer.Write(text);
		}
		public void AppendAllLines(string[] lines) => AppendAllText(string.Join('\n', lines));
		public void AppendAllLines(IEnumerable<string> lines) => AppendAllText(string.Join('\n', lines));
		#endregion
		public override bool Delete()
		{
			bool output = Exists;
			File.Delete(FullPath);
			return output;
		}

		#region Extras
		public override void MoveTo(OSDirectory directory)
		{
			if (!directory.Exists)
				throw new DirectoryNotFoundException(directory.FullPath);
			OSFile copy = directory.GetFile(Name);
			if (Exists)
			{
				using (FileStream from = OpenRead())
				{
					using FileStream to = copy.Create();
					from.CopyTo(to);
				}
				Delete();
			}
			FullPath = copy.FullPath;
		}
		public void MoveIntoFile(OSFile file)
		{
			if (!file.Parent.Exists)
				throw new DirectoryNotFoundException(file.Parent.FullPath);
			if (Exists)
			{
				using (FileStream from = OpenRead())
				{
					using FileStream to = file.Create();
					from.CopyTo(to);
				}
				Delete();
			}
			FullPath = file.FullPath;
		}
		public async Task MoveToAsync(OSDirectory directory)
		{
			if (!directory.Exists)
				throw new DirectoryNotFoundException(directory.FullPath);
			OSFile copy = directory.GetFile(Name);
			if (Exists)
			{
				using (FileStream from = OpenRead())
				{
					using FileStream to = copy.Create();
					await from.CopyToAsync(to);
				}
				Delete();
			}
			FullPath = copy.FullPath;
		}
		public override OSSystemInfo CopyTo(OSDirectory directory)
		{
			if (!directory.Exists)
				throw new DirectoryNotFoundException(directory.FullPath);
			OSFile copy = directory.GetFile(Name);
			if (Exists)
			{
				using FileStream from = OpenRead();
				using FileStream to = copy.Create();
				from.CopyTo(to);
			}
			FullPath = copy.FullPath;
			return copy;
		}
		public void CopyIntoFile(OSFile file)
		{
			if (!file.Parent.Exists)
				throw new DirectoryNotFoundException(file.Parent.FullPath);
			if (Exists)
			{
				using FileStream from = OpenRead();
				using FileStream to = file.Create();
				from.CopyTo(to);
			}
			FullPath = file.FullPath;
		}
		public async Task<OSSystemInfo> CopyToAsync(OSDirectory directory)
		{
			if (!directory.Exists)
				throw new DirectoryNotFoundException(directory.FullPath);
			OSFile copy = directory.GetFile(Name);
			if (Exists)
			{
				using FileStream from = OpenRead();
				using FileStream to = copy.Create();
				await from.CopyToAsync(to);
			}
			FullPath = copy.FullPath;
			return copy;
		}
		#endregion
	}
}