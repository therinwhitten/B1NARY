namespace HDConsole.IO
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Linq;
	using System.Reflection;
	using SysPath = System.IO.Path;
	using UnityEngine;
	using System.Threading.Tasks;

	public sealed class OSDirectory : OSSystemInfo, IEquatable<OSDirectory>, IEquatable<string>
	{
		public static explicit operator DirectoryInfo(OSDirectory file) => new DirectoryInfo(file.FullPath);
		public static explicit operator OSDirectory(DirectoryInfo file) => new OSDirectory(file);
		/// <summary>
		/// Gets the local appdata folder for saving settings, configs, etc.
		/// </summary>
		public static OSDirectory PersistentData
		{
			get
			{
				m_persist ??= new OSDirectory(Application.persistentDataPath);
				return m_persist;
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
				m_streaming ??= new OSDirectory(Application.streamingAssetsPath);
				return m_streaming;
			}
		}
		private static OSDirectory m_streaming;

		public OSDirectory(DirectoryInfo info) : this(info.FullName) { }
		public OSDirectory(OSDirectory source) : base(source.FullPath) { }
		public OSDirectory(OSPath fullPath) : base(fullPath) { }

		public override string Name
		{
			get
			{
				string fullPath = this.FullPath.ToString();
				fullPath = fullPath.Remove(fullPath.Length - 1);
				fullPath = fullPath.Substring(fullPath.LastIndexOf(SysPath.DirectorySeparatorChar) + 1);
				return fullPath;
			}
			set
			{
				string fullPath = this.FullPath.ToString();
				string oldName = Name;
				fullPath = fullPath.Remove(fullPath.Length - 1 - oldName.Length, oldName.Length);
				this.FullPath = fullPath.Insert(fullPath.Length - 1, value);
			}
		}


		public override bool Exists
		{
			get => Directory.Exists(FullPath);
			set
			{
				if (Exists == value)
					return;
				if (value)
					Create();
				else
					Delete();
			}
		}

		public void Create() => Directory.CreateDirectory(FullPath);
		public OSFile File(OSPath path) => new(FullPath + path);

		public OSDirectory GetSubdirectories(params string[] subDirectories) 
			=> GetSubdirectories(true, subDirectories);
		public OSDirectory GetSubdirectories(bool createDirectories, params string[] subDirectories)
		{
			OSDirectory output = this;
			for (int i = 0; i < subDirectories.Length; i++)
			{
				output = new OSDirectory(SysPath.Combine(FullPath, subDirectories[i]));
				if (createDirectories && !output.Exists)
					output.Create();
			}
			return output;
		}

		public IEnumerable<OSDirectory> EnumerateDirectories()
		{
			return	from fullPath in Directory.EnumerateDirectories(FullPath)
					select new OSDirectory(fullPath);
		}
		public OSDirectory[] GetDirectories()
		{
			string[] directories = Directory.GetDirectories(FullPath);
			OSDirectory[] result = new OSDirectory[directories.Length];
			for (int i = 0; i < directories.Length; i++)
				result[i] = new OSDirectory(directories[i]);
			return result;
		}

		public IEnumerable<OSFile> EnumerateFiles()
		{
			return	from fullPath in Directory.EnumerateFiles(FullPath)
					select new OSFile(fullPath);
		}
		public OSFile[] GetFiles()
		{
			string[] files = Directory.GetFiles(FullPath);
			OSFile[] result = new OSFile[files.Length];
			for (int i = 0; i < files.Length; i++)
				result[i] = new OSFile(files[i]);
			return result;
		}
		public List<OSFile> GetFiles(Func<OSFile, bool> filter)
		{
			string[] files = Directory.GetFiles(FullPath);
			List<OSFile> result = new(files.Length);
			for (int i = 0; i < files.Length; i++)
			{
				OSFile file = new(files[i]);
				if (filter.Invoke(file))
					result.Add(file);
			}
			return result;
		}
		public OSFile[] GetFiles(params string[] fileNamesAndExtensions)
		{
			OSFile[] result = new OSFile[fileNamesAndExtensions.Length];
			for (int i = 0; i < fileNamesAndExtensions.Length; i++)
				result[i] = GetFile(fileNamesAndExtensions[i]);
			return result;
		}
		public OSFile GetFile(string fileNameAndExtension)
		{
			return new OSFile(SysPath.Combine(FullPath, fileNameAndExtension));
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
		public override bool Delete()
		{
			bool output = Exists;
			Directory.Delete(FullPath, true);
			return output;
		}

		public override void MoveTo(OSDirectory intoDirectory)
		{
			OSDirectory newDir = (OSDirectory)CopyTo(intoDirectory);
			Delete();
			FullPath = newDir.FullPath;
		}
		/// <summary>
		/// Copies the directory and its contents into another directory, by 
		/// creating and destroying after.
		/// </summary>
		/// <param name="intoDirectory">Copy the directory and its contents to.</param>
		/// <returns>The new directory that hopefully contains the same contents.</returns>
		public override OSSystemInfo CopyTo(OSDirectory intoDirectory)
		{
			OSDirectory output = intoDirectory.GetSubdirectories(Name);
			if (Exists)
			{
				OSDirectory[] allDirectories = GetDirectories();
				OSFile[] allFiles = GetFiles();
				for (int i = 0; i < allDirectories.Length; i++)
					allDirectories[i].CopyTo(output);
				for (int i = 0; i < allFiles.Length; i++)
					allDirectories[i].CopyTo(output);
			}
			return output;
		}

		public bool Equals(OSDirectory other)
		{
			return Equals(other.FullPath);
		}

		public bool Equals(string other)
		{
			string left = FullPath.ToString();
			string right = new OSPath(other).ToString();
			return left == right;
		}
	}
}
