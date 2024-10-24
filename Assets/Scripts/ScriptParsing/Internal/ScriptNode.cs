﻿namespace B1NARY.Scripting
{
	/// <summary>
	/// 
	/// </summary>
	public class ScriptNode
	{
		public ScriptElement Parent { get; internal set; }
		/// <summary>
		/// A singular line for the node. For the <see cref="ScriptElement"/>,
		/// this is the root line that appears before the bracket.
		/// </summary>
		public ScriptLine PrimaryLine { get; }
		public int GlobalIndex => PrimaryLine.Index;
		/// <summary>
		/// The total length of the node.
		/// </summary>
		public virtual int Length => 1;

		public ScriptNode(ScriptLine rootLine)
		{
			PrimaryLine = rootLine;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return PrimaryLine.ToString();
		}
	}
}