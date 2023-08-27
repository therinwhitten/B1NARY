namespace HDConsole.IO
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	/// <summary>
	/// Typically turns ordinary separate file references where changing names and such into a solid change.
	/// </summary>
	public static class OSChanger
	{
		/// <summary>
		/// Renames an existing file, or if it doesn't exist, just changes the name
		/// normally.
		/// </summary>
		/// <param name="fromFile">The file to rename to.</param>
		/// <param name="name">The new name of the file.</param>
		/// <returns>
		/// A new <see cref="OSFile"/> that reflects on the new name; old file is untouched.
		/// </returns>
		public static OSFile RenameExtension(this OSFile fromFile, string extension)
		{
			OSFile newFile = new(fromFile) { Extension = extension };
			if (fromFile.Exists)
			{
				using (FileStream from = fromFile.OpenRead())
				{
					using FileStream to = newFile.Create();
					from.CopyTo(to);
				}
				fromFile.Delete();
			}
			return newFile;
		}
		/// <summary>
		/// Renames an existing file, or if it doesn't exist, just changes the name
		/// normally.
		/// </summary>
		/// <param name="fromFile">The file to rename to.</param>
		/// <param name="name">The new name of the file.</param>
		/// <returns>
		/// A new <see cref="OSFile"/> that reflects on the new name; old file is untouched.
		/// </returns>
		public static OSFile Rename(this OSFile fromFile, string name)
		{
			OSFile newFile = new(fromFile) { Name = name };
			if (fromFile.Exists)
			{
				using (FileStream from = fromFile.OpenRead())
				{
					using FileStream to = newFile.Create();
					from.CopyTo(to);
				}
				fromFile.Delete();
			}
			return newFile;
		}
		/// <summary>
		/// Renames an existing file, or if it doesn't exist, just changes the name
		/// normally. This particular command allows to keep the same extension.
		/// </summary>
		/// <param name="fromFile">The file to rename to.</param>
		/// <param name="name">The new name of the file, excluding the extension as they are inherited.</param>
		/// <returns>
		/// A new <see cref="OSFile"/> that reflects on the new name; old file is untouched.
		/// </returns>
		public static OSFile RenameWithoutExtension(this OSFile fromFile, string name)
		{
			OSFile newFile = new(fromFile) { NameWithoutExtension = name };
			if (fromFile.Exists)
			{
				using FileStream from = fromFile.OpenRead();
				using FileStream to = newFile.Create();
				from.CopyTo(to);
				fromFile.Delete();
			}
			return newFile;
		}
		/// <summary>
		/// Renames an existing file, or if it doesn't exist, just changes the name
		/// normally.
		/// </summary>
		/// <param name="fromDirectory">The directory to rename to.</param>
		/// <param name="name">The new name of the file.</param>
		/// <returns>
		/// A new <see cref="OSFile"/> that reflects on the new name; old file is untouched.
		/// </returns>
		public static OSDirectory Rename(this OSDirectory fromDirectory, string name)
		{
			OSDirectory output = fromDirectory.Parent.GetSubdirectories(name);
			if (fromDirectory.Exists)
			{
				OSDirectory[] allDirectories = fromDirectory.GetDirectories();
				OSFile[] allFiles = fromDirectory.GetFiles();
				for (int i = 0; i < allDirectories.Length; i++)
					allDirectories[i].CopyTo(output);
				for (int i = 0; i < allFiles.Length; i++)
					allFiles[i].CopyTo(output);
				fromDirectory.Delete();
			}
			return output;
		}
		public static void StartProcess(this OSSystemInfo info) => Process.Start(info.FullPath);
	}
}