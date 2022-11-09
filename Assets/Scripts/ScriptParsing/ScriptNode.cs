namespace B1NARY.Scripting
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using UnityEngine;
	
	/// <summary>
	/// A delegate that is related to the <see cref="ScriptNode.ScriptNode(ScriptDocument, ScriptPair[])"/>
	/// constructor.
	/// </summary>
	/// <param name="document"> The reference of the document to tie to. </param>
	/// <param name="subLines"> The contents of the <see cref="ScriptNode"/> </param>
	/// <returns> A node, or a class that derived from it. </returns>
	public delegate ScriptNode ScriptNodeParser(ScriptDocument document, ScriptPair[] subLines);
	/// <summary>
	/// A block of <see cref="ScriptLine"/>s that is run over time via 
	/// <see cref="Perform"/>
	/// </summary>
	public class ScriptNode
	{
		/// <summary>
		/// The line count of all the contents, and the <see cref="rootLine"/> and 
		/// brackets. 
		/// </summary>
		public int LineLength
		{
			get
			{
				const int @base =
					// Root Line
					1 +
					// Start index
					1 +
					// End index
					1;
				return @base + SubLines.Count;
			}
		}
		/// <summary>
		/// The line where it has been located right above 
		/// <see cref="ScriptLine.Type.BeginIndent"/>
		/// </summary>
		public readonly ScriptLine rootLine;
		/// <summary>
		/// Index of the start index, this is guaranteed to be one above <see cref="rootLine"/>
		/// </summary>
		public readonly int beginIndex;
		/// <summary>
		/// Index of the end index.
		/// </summary>
		public readonly int endIndex;
		/// <summary>
		/// A readonly array of the contents. <see cref="ScriptLine.Type.BeginIndent"/>
		/// is at the very start and <see cref="ScriptLine.Type.EndIndent"/> at
		/// the end is guaranteed.
		/// </summary>
		public IReadOnlyList<ScriptPair> SubLines
			=> subLines;
		/// <summary>
		/// The data stored for the sub-block
		/// </summary>
		protected readonly List<ScriptPair> subLines;
		protected readonly ScriptDocument document;
		/// <summary>
		/// Creates an instance of a node, or a block of lines to read through.
		/// </summary>
		/// <param name="document"> The document to reference to. </param>
		/// <param name="subLines"> The contents of the node. </param>
		public ScriptNode(ScriptDocument document, ScriptPair[] subLines)
		{
			const int skip = 2; // Skip leading line and bracket
			this.document = document;
			rootLine = subLines[0].scriptLine;
			beginIndex = subLines[1].scriptLine.Index;
			this.subLines = new List<ScriptPair>(subLines.Skip(skip));
			endIndex = this.subLines[this.subLines.Count - 1].scriptLine.Index;
			this.subLines.RemoveAt(this.subLines.Count - 1);
			//Debug.Log($"Last Line: {this.subLines[this.subLines.Count - 1].scriptLine}\n From block {beginIndex}");
			//Debug.Log($"ScriptNode Starts with '{subLines[0].scriptLine}'\nStart Bracket = {subLines[1].scriptLine.Index}, End Bracket = {subLines.Last().scriptLine.Index}\nAll Lines: \n{string.Join(",\n", subLines.Skip(2).Select(pair => pair.scriptLine))}");
		}
		/// <summary>
		/// Performs <see cref="ScriptLine"/> and uses <see cref="ScriptDocument.ParseLine(ScriptLine)"/>
		/// to determine to yield return a value or continue.
		/// </summary>
		public virtual IEnumerator<ScriptLine> Perform(bool pauseOnCommands)
		{
			for (int i = 0; i < subLines.Count; i++)
			{
				ScriptLine CurrentLine() => subLines[i].scriptLine;
				Debug.Log(CurrentLine());
				if (subLines[i].HasScriptNode)
				{
					IEnumerator<ScriptLine> subNode = subLines[i].scriptNode.Perform(pauseOnCommands);
					while (subNode.MoveNext())
						yield return subNode.Current;
					i += subLines[i].scriptNode.LineLength;
				}
				if (InvokeCommand(CurrentLine(), pauseOnCommands))
					continue;
				yield return CurrentLine();
			}
		}
		public virtual bool InvokeCommand(ScriptLine line, bool pauseOnCommands)
		{
			try
			{
				bool returnVar = document.ParseLine(line);
				return pauseOnCommands ? returnVar && line.type != ScriptLine.Type.Command : returnVar;
			}
			catch (Exception ex) when (!ex.Message.Contains("Managed to hit a intentation"))
			{
				Debug.LogException(new Exception("There is an error in the scriptDocument", ex));
				return line.type != ScriptLine.Type.Normal;
			}
		}
	}
}