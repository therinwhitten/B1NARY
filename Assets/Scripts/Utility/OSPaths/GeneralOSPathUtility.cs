namespace B1NARY.IO
{
	using SixLabors.ImageSharp.PixelFormats;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using static System.IO.Path;

	public static class GeneralOSPathUtility
	{
		public static OSPath ToOSPath(this FileInfo fileInfo)
		{
			string path = fileInfo.FullName;
			return new OSPath(path);
		}
		public static FileStream OpenStream(this FileInfo info, FileMode mode, FileAccess access)
		{
			return info.ToOSPath().Open(mode, access);
		}
		public static OSDirectory ToOSFolder(this DirectoryInfo directoryInfo)
		{
			string path = directoryInfo.FullName;
			return new OSDirectory(path);
		}
	}
}
