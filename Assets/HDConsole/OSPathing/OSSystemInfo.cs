﻿namespace HDConsole.IO
{
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using SystemPath = System.IO.Path;

	/// <summary>
	/// Information about a folder or file.
	/// </summary>
	public abstract class OSSystemInfo
	{
		protected OSSystemInfo(OSPath fullPath)
		{
			FullPath = fullPath;
		}

		/// <summary>
		/// The full path, regards Unix-based and windows systems.
		/// </summary>
		public virtual OSPath FullPath { get; set; }
		/// <summary>
		/// The name of the ending path. Setting only changes <see cref="FullPath"/>;
		/// use <see cref="Rename"/> for such.
		/// </summary>
		public abstract string Name { get; set; }
		/// <summary>
		/// If the path exists. Setting impacts the files.
		/// </summary>
		public abstract bool Exists { get; set; }
		/// <summary>
		/// To delete the path.
		/// </summary>
		/// <returns>If the path used to exist.</returns>
		public abstract bool Delete();
		/// <summary>
		/// Moves the path and its contents to another directory.
		/// </summary>
		public abstract void MoveTo(OSDirectory directory);
		/// <summary>
		/// Moves the path and its contents to another directory.
		/// </summary>
		public abstract OSSystemInfo CopyTo(OSDirectory directory);
		public OSDirectory Parent => new(FullPath.Parent);
	}
}
