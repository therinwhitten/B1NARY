namespace B1NARY.Scripting
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using UnityEngine;

	[Serializable]
	public readonly struct ScriptLine : IEquatable<ScriptLine>, IEquatable<string>
	{
		//public static bool operator ==(ScriptLine left, ScriptLine right)
		//	=> left.lineData == right.lineData && left.Index == right.Index;
		//public static bool operator !=(ScriptLine left, ScriptLine right)
		//	=> !(left == right);


		/// <summary> Regex to determine if it is an expression. </summary>
		public static readonly Regex emoteRegex = new Regex("\\[(.*?)\\]");
		/// <summary> Regex to determine if it is a command. </summary>
		public static readonly Regex commandRegex = new Regex("\\{(.*?)\\}");
		/// <summary> 
		/// The type of <see cref="ScriptLine"/>, for basic overview of commands 
		/// </summary>
		public enum LineType : sbyte
		{
			/// <summary> An empty block. Contains nothing. </summary>
			Empty = -1,
			/// <summary> 
			/// Normal speaking bubble, just plain text. May or may not contain
			/// rich tags. 
			/// </summary>
			Normal,
			/// <summary> A command to run and parse. </summary>
			Command,
			/// <summary> 
			/// Anything that ends with '::'
			/// </summary>
			/// <remarks>
			/// For B1NARY, this is used to change speakers.
			/// </remarks>
			Entry,
			/// <summary> 
			/// A command that has square brackets, invokes all commands that uses
			/// this.
			/// </summary>
			/// <remarks>
			/// For B1NARY, this is used to express emotions for the current speaker.
			/// </remarks>
			Attribute,
			/// <summary> A starting indent for blocks. </summary>
			BeginIndent,
			/// <summary> A ending indent for blocks. </summary>
			EndIndent,
			/// <summary> 
			/// A flag to determine if its at the end or start. There is no
			/// reason the script should hit these tags!
			/// </summary>
			DocumentFlag,
		}
		public static readonly HashSet<char> startIndents = new HashSet<char>()
		{
			'{',
			'[',
		};
		public static readonly HashSet<char> endIndents = new HashSet<char>()
		{
			'}',
			']',
		};

		/// <summary>
		/// Determines what type of line it is at a glance.
		/// </summary>
		/// <param name="lineData">The input line, comments must be removed before-hand.</param>
		/// <returns> The type of line it is. </returns>
		public static LineType ParseLineAsType(string lineData)
		{
			lineData = lineData.Trim();
			if (string.IsNullOrWhiteSpace(lineData))
			{
				return LineType.Empty;
			}

			if (lineData.Length == 1)
			{
				if (startIndents.Contains(lineData[0]))
					return LineType.BeginIndent;
				if (endIndents.Contains(lineData[0]))
					return LineType.EndIndent;
			}
			else
			{
				if (emoteRegex.IsMatch(lineData))
					return LineType.Attribute;
				if (commandRegex.IsMatch(lineData))
					return LineType.Command;
				if (lineData.EndsWith("::"))
					return LineType.Entry;
				if (lineData.StartsWith("::"))
				{
					lineData = lineData.ToLower();
					if (lineData.Contains("start") || lineData.Contains("end"))
						return LineType.DocumentFlag;
					throw new ArgumentException($"{lineData} has the marking of a " +
						"document flag, but does not possess any of the traits!");
				}
			}
			return LineType.Normal;
		}

		/// <summary>
		/// Casts a <see cref="ScriptLine"/> into a command with arguments.
		/// </summary>
		/// <param name="line">The line to parse.</param>
		/// <returns>Command with arguments.</returns>
		/// <exception cref="InvalidCastException">'{line}' is not a command!</exception>
		public static (string command, string[] arguments) CastCommand(ScriptLine line)
		{
			if (line.Type != LineType.Command)
				throw new InvalidCastException($"'{line.Type}' is not a command!");
			string outputLine = line.RawLine;
			outputLine = outputLine.Substring(outputLine.IndexOf('{') + 1);
			outputLine = outputLine.Remove(outputLine.LastIndexOf('}'));
			string[] dataArray = outputLine.Split(':', ',');
			string command = dataArray[0].Trim().ToLower();
			string[] arguments = dataArray.Skip(1).Select(str => str.Trim()).ToArray();
			return (command, arguments);
		}
		/// <summary>
		/// Casts the current <see cref="ScriptLine"/> as an emotion value, trimmed.
		/// Must have <see cref="Type.Emotion"/>.
		/// </summary>
		/// <param name="line">The line input.</param>
		/// <returns></returns>
		/// <exception cref="InvalidCastException">'{line}' is not a emotion!</exception>
		public static string CastAttribute(ScriptLine line)
		{
			if (line.Type != LineType.Attribute)
				throw new InvalidCastException($"'{line}' is not a emotion!");
			string outputLine = line.RawLine;
			outputLine = outputLine.Substring(outputLine.IndexOf('[') + 1);
			outputLine = outputLine.Remove(outputLine.LastIndexOf(']'));
			return outputLine.Trim();
		}

		/// <summary>
		/// Casts the current <see cref="ScriptLine"/> as an speaker value, trimmed.
		/// Must have <see cref="Type.Speaker"/>.
		/// </summary>
		/// <param name="line">The line input.</param>
		/// <returns></returns>
		/// <exception cref="InvalidCastException">'{line}' is not a speaker!</exception>
		public static string CastEntry(ScriptLine line)
		{
			if (line.Type != LineType.Entry)
				throw new InvalidCastException($"'{line}' is not a speaker command!");
			return line.RawLine.Remove(line.RawLine.IndexOf("::")).Trim();
		}


		/// <summary> The raw string contents. </summary>
		public string RawLine { get; }
		/// <summary> The index where it appears in for the document. </summary>
		public int Index { get; }
		/// <summary> The type of line it detects which. </summary>
		public LineType Type { get; }
		/// <summary>
		/// The index that is meant to be used for arrays and indexing.
		/// </summary>
		public int ArrayIndex => Index - 1;

		public ScriptLine(string rawLine, int index)
		{
			int commentIndex = rawLine.IndexOf("//");
			if (commentIndex != -1)
				rawLine = rawLine.Remove(commentIndex);
			RawLine = string.IsNullOrWhiteSpace(rawLine) ? "" : rawLine.Trim();
			Index = index;
			Type = ParseLineAsType(RawLine);
		}

		public bool Equals(ScriptLine other)
		{
			return RawLine == other.RawLine
				&& Index == other.Index;
		}

		public bool Equals(string other)
		{
			return RawLine == other;
		}
		public override string ToString()
		{
			return $"{Index}:\t{RawLine} ({Type})";
		}
	}
}
