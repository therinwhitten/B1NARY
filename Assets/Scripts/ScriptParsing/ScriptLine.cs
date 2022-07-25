using System;
using System.Linq;
using System.Text.RegularExpressions;

public struct ScriptLine
{
	/// <summary> Regex to determine if it contains rich flags. </summary>
	public static readonly Regex richRegex = new Regex("<(.*?)>");
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
		/// <summary> Rich text, contains special effects. </summary>
		Rich,
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
	}

	/// <summary>
	/// Determines what type of line it is at a glance.
	/// </summary>
	/// <param name="lineData">The input line, comments must be removed before-hand.</param>
	/// <returns> The type of line it is. </returns>
	public static Type ParseLineAsType(string lineData)
	{
		lineData = lineData.Trim();
		if (string.IsNullOrEmpty(lineData))
			return Type.Empty;
		if (richRegex.IsMatch(lineData))
			return Type.Rich;
		if (lineData.Length == 1)
		{
			throw new NotImplementedException("Indents not implemented");
		}
		if (emoteRegex.IsMatch(lineData))
			return Type.Emotion;
		if (commandRegex.IsMatch(lineData))
			return Type.Command;
		return Type.Normal;
	}
	public static (string command, string[] arguments) CastCommand(ScriptLine line)
	{
		if (line.type != Type.Command)
			throw new InvalidCastException($"'{line}' is not a command!");
		string[] dataArray = line.lineData.Trim('{', '}').Split(':', ',');
		return (dataArray.First().Trim(), dataArray.Skip(1).Select(str => str.Trim()).ToArray());
	}

	public bool IsIndent => type == Type.BeginIndent || type == Type.EndIndent;

	public readonly string lineData;
	public string ScriptDocument => docPointer.Invoke();
	private Func<string> docPointer;//scriptDocument;
	public readonly int Index;
	public readonly Type type;
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

	public override string ToString() => $"{type}.{Index}\t: {lineData}";
}