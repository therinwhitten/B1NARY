namespace B1NARY.Scripting.Experimental
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class ScriptNode
	{
		public ScriptElement Parent { get; internal set; }
		/// <summary>
		/// A singular line for the node. For the <see cref="ScriptElement"/>,
		/// this is the root line that appears before the bracket.
		/// </summary>
		public ScriptLine PrimaryLine { get; }
		/// <summary>
		/// The total length of the node.
		/// </summary>
		public virtual int Length => 1;

		internal ScriptNode(ScriptLine rootLine)
		{
			PrimaryLine = rootLine;
		}

		public override string ToString()
		{
			return PrimaryLine.ToString();
		}
	}
}
