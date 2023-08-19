namespace B1NARY.IO
{
	using SixLabors.ImageSharp.PixelFormats;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using static System.IO.Path;

	///// <summary>
	///// Combines the usage of both <see cref="FileInfo"/> and <see cref="OSPath"/>s
	///// </summary>
	//public static class GeneralOSPathUtility
	//{
	//	/// <summary>
	//	/// Returns a <see cref="FileInfo"/> to an <see cref="OSFile"/> to make sure
	//	/// it syncs with both windows and unix systems.
	//	/// </summary>
	//	public static OSFile ToOSFile(this FileInfo fileInfo)
	//	{
	//		string path = fileInfo.FullName;
	//		return new OSFile(path);
	//	}
	//	/// <summary>
	//	/// Returns a <see cref="FileInfo"/> to an <see cref="OSPath"/> (not a file!)
	//	/// to make sure it syncs with both windows and unix systems.
	//	/// </summary>
	//	public static OSPath ToOSPath(this FileInfo fileInfo)
	//	{
	//		string path = fileInfo.FullName;
	//		return new OSPath(path);
	//	}
	//	/// <summary>
	//	/// Opens a stream while considering unix and windows systems.
	//	/// </summary>
	//	public static FileStream OpenStream(this FileInfo info, FileMode mode, FileAccess access)
	//	{
	//		return info.ToOSPath().Open(mode, access);
	//	}
	//	public static bool ExistsInOSFile(this FileInfo fileInfo)
	//	{
	//		return fileInfo.ToOSFile().Exists;
	//	}
	//	public static DirectoryInfo OpenSubdirectory(this DirectoryInfo info, params string[] folderNames)
	//	{
	//		DirectoryInfo output = info;
	//		for (int i = 0; i < folderNames.Length; i++)
	//			output = new(output.ToOSDirectory().GetSubdirectories(folderNames[i]).FullPath);
	//		return output;
	//	}
	//	public static OSDirectory ToOSDirectory(this DirectoryInfo directoryInfo)
	//	{
	//		string path = directoryInfo.FullName;
	//		return new OSDirectory(path);
	//	}
	//}
}
