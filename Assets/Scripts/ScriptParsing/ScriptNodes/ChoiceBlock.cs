namespace B1NARY.Scripting
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
		private readonly IReadOnlyDictionary<string, ScriptNode> choices;

		public ChoiceBlock(ScriptDocument scriptDocument, ScriptPair[] subLines) : base(scriptDocument, subLines)
		{
			var choices = new Dictionary<string, ScriptNode>();
			var linesEnum = base.subLines.GetEnumerator();
			while (linesEnum.MoveNext())
			{
				if (!linesEnum.Current.HasScriptNode)
					continue;
				choices.Add(linesEnum.Current.scriptLine.lineData, linesEnum.Current.scriptNode);
				for (int i = 0, lineLength = linesEnum.Current.scriptNode.LineLength - 1; i < lineLength; i++)
					linesEnum.MoveNext();
			}
			linesEnum.Dispose();
			this.choices = choices;
			if (choices.Count < 1)
				Debug.LogWarning($"Choice block of line '{rootLine.Index}' has one or less choices!");
			//Debug.Log($"Choice Block of line {subLines[0].scriptLine.Index}: \n{string.Join(",\n", this.choices.Keys)}");
		}

		public override IEnumerator<ScriptLine> Perform(bool pauseOnCommands)
		{
			ChoicePanel panel = ChoicePanel.StartNew(choices.Keys);
			while (panel.CurrentlyPickedChoice == null)
				yield return default;
			IEnumerator<ScriptLine> node = choices[panel.CurrentlyPickedChoice].Perform(pauseOnCommands);
			panel.Dispose();
			while (node.MoveNext())
				yield return node.Current;
		}
	}
}