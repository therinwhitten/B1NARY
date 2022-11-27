namespace B1NARY.Scripting
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using B1NARY.DataPersistence;
	using UnityEngine;

	public sealed class IfBlock : ScriptNode
	{
		public IfBlock(ScriptDocument scriptDocument, ScriptPair[] subLines, int index) : base(scriptDocument, subLines, index)
		{

		}
		/// <summary>
		/// Gets a value indicating whether this block can perform.
		/// </summary>
		public bool CanPerform
		{
			get // {if: Name}
			{
				string argumentName = ScriptLine.CastCommand(rootLine).arguments.Single();
				bool output = true;
				while (argumentName[0] == '!') // Inverter
				{
					output = !output;
					argumentName = argumentName.Substring(1);
				}

				if (PersistentData.Instance.Booleans.TryGetValue(argumentName, out bool value))
					return output == value;
				throw new MissingFieldException($"{argumentName} doesn't exist in the saves!");
			}
		}


		public override IEnumerator<ScriptLine> Perform(bool pauseOnCommands)
		{
			Debug.Log($"Starting If Statement in line {rootLine.Index}: {CanPerform}");
			if (!CanPerform)
			{
				// The else block behaviour
				if (document.nodes[nodeIndex + 1] is ElseBlock elseBlock)
				{
					using (var elseEnumerator = elseBlock.IfStatementPerform(pauseOnCommands)) 
						while (elseEnumerator.MoveNext())
							yield return elseEnumerator.Current;
				}
				yield break;
			}
			using (IEnumerator<ScriptLine> @base = base.Perform(pauseOnCommands))
				while (@base.MoveNext())
					yield return @base.Current;
		}
	}
	public sealed class ElseBlock : ScriptNode
	{
		public ElseBlock(ScriptDocument scriptDocument, ScriptPair[] subLines, int index) : base(scriptDocument, subLines, index)
		{

		}

		public override IEnumerator<ScriptLine> Perform(bool pauseOnCommands)
		{
			yield break;
		}
		internal IEnumerator<ScriptLine> IfStatementPerform(bool pauseOnCommands)
		{
			return base.Perform(pauseOnCommands);
		}
	}
}