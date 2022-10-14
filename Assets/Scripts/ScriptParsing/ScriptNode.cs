namespace B1NARY.Scripting.Experimental
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using UnityEngine;
	
	public delegate ScriptNode ScriptNodeParser(Func<ScriptLine, bool> parseLine, ScriptPair[] subLines);
	/// <summary>
	/// A block of <see cref="ScriptLine"/>s that is run over time via 
	/// <see cref="Perform"/>
	/// </summary>
	public class ScriptNode
	{
		/// <summary>
		/// The line where it has been located right above 
		/// <see cref="ScriptLine.Type.BeginIndent"/>
		/// </summary>
		public readonly ScriptLine rootLine;
		/// <summary>
		/// The total 'Array' size, <see cref="SubLines"/> and <see cref="rootLine"/>
		/// included.
		/// </summary>
		public readonly int lineLength;
		/// <summary>
		/// A readonly array of the contents. <see cref="ScriptLine.Type.BeginIndent"/>
		/// is at the very start and <see cref="ScriptLine.Type.EndIndent"/> at
		/// the end is guaranteed.
		/// </summary>
		public ReadOnlyCollection<ScriptPair> SubLines
			=> Array.AsReadOnly(subLines);
		/// <summary>
		/// The data stored for the sub-block
		/// </summary>
		protected ScriptPair[] subLines;
		protected readonly Func<ScriptLine, bool> parseLine;
		public ScriptNode(Func<ScriptLine, bool> parseLine, ScriptPair[] subLines)
		{
			this.parseLine = parseLine;
			lineLength = subLines.Length;
			rootLine = subLines.First().scriptLine;
			this.subLines = subLines.Skip(1).ToArray();
		}
		/// <summary>
		/// Performs <see cref="ScriptLine"/> and uses <see cref="ScriptDocument.ParseLine(ScriptLine)"/>
		/// to determine to yield return a value or continue.
		/// </summary>
		public virtual IEnumerator<ScriptLine> Perform(bool pauseOnCommands)
		{
			// i as 1 to skip the bracket, length - 1 for the same.
			for (int i = 1; i < subLines.Length - 1; i++)
			{
				Debug.Log(subLines[i].scriptLine.Index);
				if (subLines[i].HasScriptNode)
				{
					IEnumerator<ScriptLine> subNode = subLines[i].scriptNode.Perform(pauseOnCommands);
					while (subNode.MoveNext())
						yield return subNode.Current;
					i += subLines[i].scriptNode.lineLength;
				}
				bool canFreelyPass = true;
				bool forceBlock = false;
				try
				{
					string command = ScriptLine.CastCommand(subLines[i].scriptLine).command;
					forceBlock = SceneManager.Commands.ContainsKey(command);
					canFreelyPass = parseLine.Invoke(subLines[i].scriptLine);
				}
				catch (Exception ex) when (!ex.Message.Contains("Managed to hit a intentation"))
				{
					Debug.LogException(new Exception("There is an error in the scriptDocument", ex));
					canFreelyPass = subLines[i].LineType != ScriptLine.Type.Normal;
				}
				if (forceBlock)
					yield return subLines[i].scriptLine;
				if (canFreelyPass && pauseOnCommands == false)
					continue;
				yield return subLines[i].scriptLine;
			}
		}
	}
}