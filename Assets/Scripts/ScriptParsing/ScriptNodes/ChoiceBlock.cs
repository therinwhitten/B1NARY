namespace B1NARY.Scripting.Experimental
{
	using B1NARY.UI;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;

	public sealed class ChoiceBlock : ScriptNode
	{
		private Dictionary<string, ScriptNode> choices;

		public ChoiceBlock(ScriptDocument scriptDocument, ScriptPair[] subLines) : base(scriptDocument, subLines)
		{
			choices = (
				from pair in subLines 
				where pair.scriptNode != null
				where pair.scriptNode.GetType() == typeof(ScriptNode)
				select pair.scriptNode)
				.ToDictionary(node => node.rootLine.lineData);
		}

		public override IEnumerator<ScriptLine> Perform(bool pauseOnCommands)
		{
			ChoicePanel panel = ChoicePanel.StartNew(choices.Keys);
			var taskCompletionSource = new TaskCompletionSource<string>();
			panel.PickedChoice += str => taskCompletionSource.SetResult(str);
			IEnumerator<ScriptLine> node = choices[taskCompletionSource.Task.Result].Perform(pauseOnCommands);
			while (node.MoveNext())
				yield return node.Current;
		}
	}
}