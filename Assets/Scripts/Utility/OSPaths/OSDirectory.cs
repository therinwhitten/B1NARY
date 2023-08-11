namespace B1NARY.IO
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Linq;
	using System.Reflection;
	using static System.IO.Path;
	using UnityEngine;

	public class OSDirectory : OSFolderEntry
	{
		/// <summary>
		/// Gets the local appdata folder for saving settings, configs, etc.
		/// </summary>
		public static OSDirectory PersistentData
		{
			get
			{
				try { return m_persist = new OSDirectory(Application.persistentDataPath); }
				catch { return m_persist; }
			}
		}
		private static OSDirectory m_persist;


		/// <summary>
		/// Gets the streaming assets folder with a <see cref="DirectoryInfo"/>.
		/// </summary>
		public static OSDirectory StreamingAssets
		{
			get
			{
				try { return m_streaming = new OSDirectory(Application.streamingAssetsPath); }
				catch { return m_streaming; }
			}
		}
		private static OSDirectory m_streaming;

		public static OSDirectory OfAssembly<T>() =>
			OfAssembly(typeof(T).Assembly);

		public static OSDirectory OfAssembly(Assembly assembly) =>
			new OSDirectory(GetDirectoryName(assembly.Location));

		public OSDirectory(OSPath fullPath) : this(fullPath, OSPath.Empty) { }
		public OSDirectory(DirectoryInfo info) : this(info.FullName, OSPath.Empty) { }

		public OSDirectory(OSPath fullPath, OSPath root) 
			: base(fullPath, root)
		{
		}

		public OSDirectory AsRoot() => new OSDirectory(FullPath, FullPath);

		public OSDirectory Up() => new(FullPath.Parent, Root);

		public OSDirectory Down(string folder) => new(FullPath + folder, Root);

		public bool Exists => Directory.Exists(FullPath);
		public void Create() => Directory.CreateDirectory(FullPath);
		public OSFile File(OSPath path) => new OSFile(FullPath + path, Root);

		public OSDirectory GetSubdirectory(string subDirectory)
		{
			OSDirectory output = new(Combine(Path, subDirectory));
			if (!output.Exists)
				output.Create();
			return output;
		}

		public IEnumerable<OSDirectory> EnumerateDirectories()
		{
			return	from fullPath in Directory.EnumerateDirectories(FullPath)
					select new OSDirectory(fullPath, Root);
		}
		public OSDirectory[] GetDirectories()
		{
			string[] directories = Directory.GetDirectories(FullPath);
			OSDirectory[] result = new OSDirectory[directories.Length];
			for (int i = 0; i < directories.Length; i++)
				result[i] = new OSDirectory(FullPath, directories[i]);
			return result;
		}

		public IEnumerable<OSFile> EnumerateFiles()
		{
			return	from fullPath in Directory.EnumerateFiles(FullPath)
					select new OSFile(fullPath, Root);
		}
		public OSFile[] GetFiles()
		{
			string[] files = Directory.GetFiles(FullPath);
			OSFile[] result = new OSFile[files.Length];
			for (int i = 0; i < files.Length; i++)
				result[i] = new OSFile(FullPath, files[i]);
			return result;
		}
	}
}
