namespace B1NARY.Scripting.Experimental
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public sealed class ChoiceBlock : ScriptNode
	{
		public ChoiceBlock(Func<ScriptLine, bool> parseLine, ScriptPair[] subLines) : base(parseLine, subLines)
		{

		}

		public override IEnumerator<ScriptLine> Perform()
		{
			throw new NotImplementedException();
		}
	}
}