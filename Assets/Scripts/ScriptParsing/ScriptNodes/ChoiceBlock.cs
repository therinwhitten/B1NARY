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

	/// <summary>
	/// A node that keeps track of multiple <see cref="ScriptNode"/> and uses it's
	/// <see cref="ScriptNode.rootLine"/> as the choice.
	/// </summary>
	public sealed class ChoiceBlock : ScriptNode
	{
		/// <summary>
		/// A Dictionary that keeps track of the choices and the scriptNode that
		/// is linked up to it.
		/// </summary>
		private readonly IReadOnlyDictionary<string, ScriptNode> choices;

		public ChoiceBlock(ScriptDocument scriptDocument, ScriptPair[] subLines) : base(scriptDocument, subLines)
		{
			var choices = new Dictionary<string, ScriptNode>();
			var linesEnum = base.subLines.AsEnumerable().GetEnumerator();
			while (linesEnum.MoveNext())
			{
				if (!linesEnum.Current.HasScriptNode)
					continue;
				choices.Add(linesEnum.Current.scriptLine.lineData, linesEnum.Current.scriptNode);
				linesEnum = ScriptDocument.SkipNode(linesEnum, linesEnum.Current.scriptNode);
			}
			linesEnum.Dispose();
			this.choices = choices;
			if (choices.Count < 1)
				Debug.LogWarning($"Choice block of line '{rootLine.Index}' has one or less choices!");
			//Debug.Log($"Choice Block of line {subLines[0].scriptLine.Index}: \n{string.Join(",\n", this.choices.Keys)}");
		}

		public override IEnumerator<ScriptLine> Perform(bool pauseOnCommands)
		{
			document.ParseLine(new ScriptLine(string.Join(",", ((Command)rootLine).arguments), () => rootLine.ScriptDocument, rootLine.Index));
			ChoicePanel panel = ChoicePanel.StartNew(choices.Keys);
			panel.PickedChoice += str => document.NextLine();
			while (!panel.HasPickedChoice)
				yield return default;
			IEnumerator<ScriptLine> node = choices[panel.CurrentlyPickedChoice].Perform(pauseOnCommands);
			panel.Dispose();
			while (node.MoveNext())
				yield return node.Current;
		}
	}
}