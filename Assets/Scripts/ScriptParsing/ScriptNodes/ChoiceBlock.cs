namespace B1NARY.Scripting
{
	using B1NARY.DataPersistence;
	using B1NARY.UI;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEditor.Experimental.GraphView;
	using UnityEngine;

	/// <summary>
	/// A node that keeps track of multiple <see cref="ScriptNode"/> and uses it's
	/// <see cref="ScriptNode.rootLine"/> as the choice.
	/// </summary>
	[NodeCommandCondition("choice")]
	public sealed class ChoiceBlock : ScriptNode
	{
		/// <summary>
		/// A Dictionary that keeps track of the choices and the scriptNode that
		/// is linked up to it.
		/// </summary>
		private readonly IReadOnlyDictionary<ScriptLine, ScriptNode> choices;

		public ChoiceBlock(ScriptDocument scriptDocument, ScriptPair[] subLines, int index) : base(scriptDocument, subLines, index)
		{
			var choices = new Dictionary<ScriptLine, ScriptNode>();
			var linesEnum = base.subLines.AsEnumerable().GetEnumerator();
			while (linesEnum.MoveNext())
			{
				if (!linesEnum.Current.HasScriptNode)
					continue;
				choices.Add(linesEnum.Current.scriptLine, linesEnum.Current.scriptNode);
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
			document.ParseLine(new ScriptLine(string.Join(",", ScriptLine.CastCommand(rootLine).arguments), rootLine.resourcesFilePath, rootLine.ScriptDocument, rootLine.Index));
			if (PersistentData.Instance.GameSlotData.choice.TryGetValue(rootLine.Index, out var line))
			{
				using (IEnumerator<ScriptLine> node = choices[line].Perform(pauseOnCommands))
					while (node.MoveNext())
						yield return node.Current;
				yield break;
			}
			ChoicePanel panel = ChoicePanel.StartNew(choices.Keys);
			panel.PickedChoice += str => document.NextLine();
			while (!panel.HasPickedChoice)
				yield return default;
			using (IEnumerator<ScriptLine> node = choices[panel.CurrentlyPickedChoice.Value].Perform(pauseOnCommands))
			{
				panel.Dispose();
				while (node.MoveNext())
					yield return node.Current;
			}
		}
	}
}