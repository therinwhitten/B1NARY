namespace B1NARY.ScriptingBeta
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public sealed class ChoiceBlock : ScriptNode
	{
		public ChoiceBlock(int rootListIndex, Func<IReadOnlyList<ScriptLine>> list,
			Func<IReadOnlyDictionary<int, ScriptNode>> nodeList) : base(rootListIndex, list, nodeList)
		{

		}
		public override IEnumerator Perform(HandleLine line)
		{
			throw new NotImplementedException();
		}
	}
}