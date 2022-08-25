namespace B1NARY.Scripting.Experimental
{
	using System;
	using System.Collections.Generic;


	public class EntryNode : ScriptNode
	{
		public EntryNode(Func<ScriptLine, bool> parseLine, ScriptPair[] subLines) : base(parseLine, subLines)
		{

		}

		public override IEnumerator<ScriptLine> Perform()
		{
			for (int i = 0; i < subLines.Length; i++)
			{
				if (subLines[i].HasScriptNode)
				{
					IEnumerator<ScriptLine> subNode = subLines[i].scriptNode.Perform();
					for (int ii = 0; subNode.MoveNext(); ii++)
						yield return subNode.Current;
					i += subLines[i].scriptNode.lineLength;
				}
				if (parseLine.Invoke(subLines[i].scriptLine))
					continue;
				yield return subLines[i].scriptLine;
			}
		}
	}
}