namespace HDConsole.IO
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;



	/// <summary>
	/// A highly complicated 'string' that purely only cares about the path.
	/// Do not use this as an actual class that can do stuff!
	/// </summary>
	/// <remarks> 
	/// Prints the path that can be used and is the same as <see cref="Normalized"/>. 
	/// </remarks>
	public struct OSPath
	{
		public static string RemoveExtension(in string input)
		{
			string trimmed = input.Trim();
			// its a directory
			if (trimmed.EndsWith(DirectorySeparatorChar))
				return trimmed;

			int fileIndex = trimmed.LastIndexOf('.');
			if (fileIndex == -1)
				return trimmed;
			string removed = trimmed.Remove(fileIndex);
			return removed;
		}
		public static string AddExtension(string input, string extension)
		{
			string trimmed = input.Trim();
			// its a directory
			if (trimmed.EndsWith(DirectorySeparatorChar))
				throw new InvalidCastException($"'{input}' is not a file path!");
			
			int fileIndex = trimmed.LastIndexOf('.');
			if (fileIndex != -1)
				trimmed = trimmed.Remove(fileIndex);

			if (!extension.StartsWith('.'))
				extension = '.' + extension;

			string output = trimmed + extension;
			return output;
		}
		public static OSPath Combine(string left, params string[] extras) => Combine(new OSPath(left), extras);
		public static OSPath Combine(OSPath result, params string[] extras)
		{
			for (int i = 0; i < extras.Length; i++)
				result += extras[i];
			return result;
		}
		public static readonly OSPath Empty = new("");
		public static char DirectorySeparatorChar => Path.DirectorySeparatorChar;

		public static bool IsWindows => DirectorySeparatorChar == '\\';

		public OSPath(string text)
		{
			Text = text.Trim();
		}

		public static implicit operator OSPath(string text) => text == null ? Empty : new OSPath(text);
		public static implicit operator string(OSPath path) => path.Normalized;
		public override string ToString() => Normalized;

		private string Text { get; }

		public string Normalized => IsWindows ? Windows : Unix;
		public string Windows => Text.Replace('/', '\\');
		public string Unix => Simplified.Text.Replace('\\', '/');

		public OSPath Relative => Simplified.Text.TrimStart('/', '\\');
		public OSPath Absolute => IsAbsolute ? this : "/" + Relative;

		public bool IsAbsolute => IsRooted || HasVolume;
		public bool IsRooted => Text.Length >= 1 && (Text[0] == '/' || Text[0] == '\\');
		public bool HasVolume => Text.Length >= 2 && Text[1] == ':';
		public OSPath Simplified => HasVolume ? Text.Substring(2) : Text;

		public string[] Split => Normalized.Split(DirectorySeparatorChar);
		public OSPath Parent => Path.GetDirectoryName(Text);

		public bool Contains(OSPath path) =>
			Normalized.StartsWith(path);

		public static OSPath operator +(OSPath left, OSPath right) =>
			new OSPath(Path.Combine(left, right.Relative));

		public static OSPath operator -(OSPath left, OSPath right) =>
			left.Contains(right)
			? new OSPath(left.Normalized.Substring(right.Normalized.Length)).Relative
			: left;

		// Extras relating to strings
		public int Length => ToString().Length;
	}
}
