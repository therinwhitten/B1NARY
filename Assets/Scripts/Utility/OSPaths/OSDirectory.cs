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

	public class OSDirectory : OSSystemInfo
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

		public OSDirectory AsRoot() => new OSDirectory(FullName, FullName);

		public OSDirectory Up() => new(FullName.Parent, Root);

		public OSDirectory Down(string folder) => new(FullName + folder, Root);

		public bool Exists => Directory.Exists(FullName);
		public void Create() => Directory.CreateDirectory(FullName);
		public OSFile File(OSPath path) => new OSFile(FullName + path, Root);

		public OSDirectory GetSubdirectories(params string[] subDirectories)
		{
			OSDirectory output = this;
			for (int i = 0; i < subDirectories.Length; i++)
			{
				output = new OSDirectory(Combine(FullName, subDirectories[i]));
				if (!output.Exists)
					output.Create();
			}
			return output;
		}

		public IEnumerable<OSDirectory> EnumerateDirectories()
		{
			return	from fullPath in Directory.EnumerateDirectories(FullName)
					select new OSDirectory(fullPath, Root);
		}
		public OSDirectory[] GetDirectories()
		{
			string[] directories = Directory.GetDirectories(FullName);
			OSDirectory[] result = new OSDirectory[directories.Length];
			for (int i = 0; i < directories.Length; i++)
				result[i] = new OSDirectory(directories[i], FullName);
			return result;
		}

		public IEnumerable<OSFile> EnumerateFiles()
		{
			return	from fullPath in Directory.EnumerateFiles(FullName)
					select new OSFile(fullPath, Root);
		}
		public OSFile[] GetFiles()
		{
			string[] files = Directory.GetFiles(FullName);
			OSFile[] result = new OSFile[files.Length];
			for (int i = 0; i < files.Length; i++)
				result[i] = new OSFile(files[i], FullName);
			return result;
		}
		public OSFile GetFile(string fileNameAndExtension)
		{
			return new OSFile(Combine(FullName, fileNameAndExtension));
		}

		/// <summary>
		/// Creates a new file, increases number incrementally if there are any
		/// files that are existant in the name.
		/// </summary>
		/// <param name="source"> The source of the new file. </param>
		/// <param name="fileName"> The file name with the extension. </param>
		/// <param name="alwaysIncludeNumber"> If the filename should always include the number at the end. </param>
		/// <returns> A fileinfo that doesn't have any files to it. </returns>
		public OSFile GetFileIncremental(string fileName, bool alwaysIncludeNumber = false)
		{
			HashSet<string> otherNames = new(EnumerateFiles().Select(fileInfo => fileInfo.Name));
			if (alwaysIncludeNumber || otherNames.Contains(fileName))
				for (int i = alwaysIncludeNumber ? 0 : 1; true; i++)
				{
					string incrementalName = fileName.Insert(fileName.LastIndexOf('.'), $"_{i}");
					if (otherNames.Contains(incrementalName))
						continue;
					return GetFile(incrementalName);
				}
			return GetFile(fileName);
		}
	}
}
