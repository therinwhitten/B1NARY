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
	using UnityEngine;

	/// <summary>
	/// A node that keeps track of multiple <see cref="ScriptNode"/> and uses it's
	/// <see cref="ScriptNode.rootLine"/> as the choice.
	/// </summary>
	public sealed class ChoiceBlock : ScriptElement
	{
		public static Predicate<List<ScriptLine>> Predicate => (lines) => lines[0].Type == ScriptLine.LineType.Command && lines[0].RawLine.Contains("choice");
		/// <summary>
		/// A Dictionary that keeps track of the choices and the scriptNode that
		/// is linked up to it.
		/// </summary>
		private readonly IReadOnlyDictionary<ScriptLine, ScriptElement> choices;


		
		public ChoiceBlock(ScriptDocumentConfig config, List<ScriptLine> blockNodeData) : base(config, blockNodeData)
		{
			var choices = new Dictionary<ScriptLine, ScriptElement>();
			for (int i = 0; i < Lines.Count; i++)
			{
				if (Lines[i] is ScriptElement element)
					choices.Add(element.PrimaryLine, element);
				else
					throw new InvalidCastException($"line {Lines[i].PrimaryLine.Index} is not an element!");
			}
			if (choices.Count < 1)
				Debug.LogWarning($"Choice block of line '{PrimaryLine.Index}' has one or less choices!");
			this.choices = choices;
		}

		public override IEnumerator<ScriptNode> EnumerateThrough(int localIndex)
		{
			ScriptLine currentLine = LinesWithElements[localIndex].PrimaryLine;
			if (choices.TryGetValue(currentLine, out ScriptElement element))
			{
				return element.EnumerateThrough(0);
			}
			return base.EnumerateThrough(localIndex);
		}
	}
}