namespace B1NARY.Scripting.Experimental
{
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;

	/// <summary>
	/// A single line of a scriptName, easily parsed and used for behaviours.
	/// </summary>
	[Serializable]
	public struct ScriptLine
	{
		public static bool operator ==(ScriptLine left, ScriptLine right)
			=> left.lineData == right.lineData && left.docPointer() == right.docPointer()
			&& left.Index == right.Index;
		public static bool operator !=(ScriptLine left, ScriptLine right)
			=> !(left == right);


		/// <summary> Regex to determine if it is an expression. </summary>
		public static readonly Regex emoteRegex = new Regex("\\[(.*?)\\]");
		/// <summary> Regex to determine if it is a command. </summary>
		public static readonly Regex commandRegex = new Regex("\\{(.*?)\\}");
		/// <summary> 
		/// The type of <see cref="ScriptLine"/>, for basic overview of commands 
		/// </summary>
		public enum Type
		{
			/// <summary> Normal speaking bubble, just plain text. </summary>
			Normal,
			/// <summary> An empty block. Contains nothing. </summary>
			Empty,
			/// <summary> A command to run and parse. </summary>
			Command,
			/// <summary> The new speaker set to speak future lines. </summary>
			Speaker,
			/// <summary> A set emotion of the current speaker. Used for animations. </summary>
			Emotion,
			/// <summary> A starting indent for blocks. </summary>
			BeginIndent,
			/// <summary> A ending indent for blocks. </summary>
			EndIndent,
			/// <summary> A flag to determine if its at the end or start. </summary>
			DocumentFlag,
		}

		/// <summary>
		/// Determines what type of line it is at a glance.
		/// </summary>
		/// <param name="lineData">The input line, comments must be removed before-hand.</param>
		/// <returns> The type of line it is. </returns>
		public static Type ParseLineAsType(string lineData)
		{
			lineData = lineData.Trim();
			if (string.IsNullOrWhiteSpace(lineData))
				return Type.Empty;
			if (lineData.Length == 1)
			{
				if (lineData.IndexOfAny(new char[] { '}', ']' }) != -1)
					return Type.EndIndent;
				if (lineData.IndexOfAny(new char[] { '{', '[' }) != -1)
					return Type.BeginIndent;
				throw new ArgumentException($"{lineData} is not an indent!");
			}
			if (emoteRegex.IsMatch(lineData))
				return Type.Emotion;
			if (commandRegex.IsMatch(lineData))
				return Type.Command;
			if (lineData.EndsWith("::"))
				return Type.Speaker;
			if (lineData.StartsWith("::"))
			{
				lineData = lineData.ToLower();
				if (lineData.Contains("start") || lineData.Contains("end"))
					return Type.DocumentFlag;
				throw new ArgumentException($"{lineData} has the marking of a " +
					"document flag, but does not possess any of the traits!");
			}
			return Type.Normal;
		}
		/// <summary>
		/// Casts a <see cref="ScriptLine"/> into a command with arguments.
		/// </summary>
		/// <param name="line">The line to parse.</param>
		/// <returns>Command with arguments.</returns>
		/// <exception cref="InvalidCastException">'{line}' is not a command!</exception>
		public static (string command, string[] arguments) CastCommand(ScriptLine line)
		{
			if (line.type != Type.Command)
				throw new InvalidCastException($"'{line}' is not a command!");
			string[] dataArray = line.lineData.Trim('{', '}').Split(':', ',');
			return (dataArray.First().Trim().ToLower(), dataArray.Skip(1).Select(str => str.Trim()).ToArray());
		}
		/// <summary>
		/// Casts the current <see cref="ScriptLine"/> as an emotion value, trimmed.
		/// Must have <see cref="Type.Emotion"/>.
		/// </summary>
		/// <param name="line">The line input.</param>
		/// <returns></returns>
		/// <exception cref="InvalidCastException">'{line}' is not a emotion!</exception>
		public static string CastEmotion(ScriptLine line)
		{
			if (line.type != Type.Emotion)
				throw new InvalidCastException($"'{line}' is not a emotion!");
			return line.lineData.Trim('[', ']', ' ', '\t');
		}

		/// <summary>
		/// Casts the current <see cref="ScriptLine"/> as an speaker value, trimmed.
		/// Must have <see cref="Type.Speaker"/>.
		/// </summary>
		/// <param name="line">The line input.</param>
		/// <returns></returns>
		/// <exception cref="InvalidCastException">'{line}' is not a speaker!</exception>
		public static string CastSpeaker(ScriptLine line)
		{

			if (line.type != Type.Speaker)
				throw new InvalidCastException($"'{line}' is not a speaker command!");
			return line.lineData.Remove(line.lineData.IndexOf("::")).Trim();
		}

		/// <summary>
		/// Indicating whether this line is an indent.
		/// </summary>
		public bool IsIndent => type == Type.BeginIndent || type == Type.EndIndent;

		/// <summary> The raw string contents. </summary>
		public readonly string lineData;
		/// <summary> The current document name it is tied to. </summary>
		public string ScriptDocument => docPointer.Invoke();
		// I know strings are stored on a heap, regardless of structs. But it would
		// - help knowing multiple instances of the same document shares a pointer
		// - to the same thing and not just multiplying it over and over again.
		private Func<string> docPointer; // scriptDocument;
		/// <summary> The index where it appears in <see cref="ScriptDocument"/>. </summary>
		public readonly int Index;
		/// <summary> The type of line it detects which. </summary>
		public readonly Type type;

		public ScriptLine(ScriptLine source)
		{
			lineData = source.lineData;
			type = source.type;
			docPointer = source.docPointer;
			Index = source.Index;
		}
		/// <summary>
		/// Stores data on a individual line based from <paramref name="scriptDocument"/>.
		/// </summary>
		/// <param name="lineData">The line data.</param>
		/// <param name="scriptDocument">The scriptName document.</param>
		/// <param name="index">The index.</param>
		public ScriptLine(string lineData, Func<string> scriptDocument, int index)
		{
			lineData = lineData.Trim();
			if (lineData.Contains("//")) // Comments
				lineData.Remove(lineData.IndexOf("//"));
			this.lineData = lineData;
			docPointer = scriptDocument;
			Index = index;
			type = ParseLineAsType(lineData);
		}

		public void Deconstruct(out Type type, out int index, out string lineData)
		{
			type = this.type;
			index = Index;
			lineData = this.lineData;
		}

		public override int GetHashCode()
		{
			return lineData.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			if (obj is ScriptLine line)
				return this == line;
			return base.Equals(obj);
		}
		public override string ToString() => $"{type}.{Index}\t: {lineData}";
	}
}