﻿namespace B1NARY.IO
{
	using SixLabors.ImageSharp.PixelFormats;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using static System.IO.Path;

	public static class GeneralOSPathUtility
	{
		public static OSFile ToOSFile(this FileInfo fileInfo)
		{
			string path = fileInfo.FullName;
			return new OSFile(path);
		}
		public static OSPath ToOSPath(this FileInfo fileInfo)
		{
			string path = fileInfo.FullName;
			return new OSPath(path);
		}
		public static FileStream OpenStream(this FileInfo info, FileMode mode, FileAccess access)
		{
			return info.ToOSPath().Open(mode, access);
		}
		public static bool ExistsInOSFile(this FileInfo fileInfo)
		{
			return fileInfo.ToOSFile().Exists;
		}
		public static DirectoryInfo OpenSubdirectory(this DirectoryInfo info, params string[] folderNames)
		{
			DirectoryInfo output = info;
			for (int i = 0; i < folderNames.Length; i++)
				output = new(output.ToOSDirectory().GetSubdirectory(folderNames[i]).FullPath);
			return output;
		}
		public static OSDirectory ToOSDirectory(this DirectoryInfo directoryInfo)
		{
			string path = directoryInfo.FullName;
			return new OSDirectory(path);
		}
	}
}
