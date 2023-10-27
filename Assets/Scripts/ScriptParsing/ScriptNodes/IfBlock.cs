namespace B1NARY.Scripting
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using B1NARY.DataPersistence;
	using UnityEngine;

	public sealed class IfBlock : ScriptElement
	{
		public static Predicate<List<ScriptLine>> Predicate => (lines) => lines[0].Type == ScriptLine.LineType.Command && lines[0].RawLine.Contains("if");

		/// <summary>
		/// Gets a value indicating whether this block can perform.
		/// </summary>
		public bool CanPerform
		{
			get
			{
				string[] argumentName = ScriptLine.CastCommand(PrimaryLine).arguments;
				if (argumentName.Length > 1 || argumentName.Length <= 0)
					throw new IndexOutOfRangeException($"This should have only one parameter, and it is for the name of the parameter!");
				bool output = true;
				while (argumentName[0][0] == '!') // Inverter
				{
					output = !output;
					argumentName[0] = argumentName[0].Substring(1);
				}

				if (SaveSlot.ActiveSlot.booleans.TryGetValue(argumentName[0], out bool value))
					return output == value;
				throw new MissingFieldException($"{argumentName[0]} doesn't exist in the saves!");
			}
		}
		public IfBlock(ScriptDocumentConfig config, List<ScriptLine> blockNodeData) : base(config, blockNodeData)
		{

		}
		public override IEnumerator<ScriptNode> EnumerateThrough(int localIndex)
		{
			if (CanPerform)
				return base.EnumerateThrough(localIndex);
			for (int i = 0; i < Parent.Elements.Count; i++)
			{
				if (ReferenceEquals(Parent.Elements[i], this))
					if (i + 1 < Parent.Elements.Count)
						if (Parent.Elements[i + 1] is ElseBlock elseBlock)
							return elseBlock.ElseEnumerate();
			}
			return Array.Empty<ScriptNode>().AsEnumerable().GetEnumerator();
		}
	}
	public sealed class ElseBlock : ScriptElement
	{
		public static Predicate<List<ScriptLine>> Predicate => (lines) => lines[0].Type == ScriptLine.LineType.Command && lines[0].RawLine.Contains("else");
		public ElseBlock(ScriptDocumentConfig config, List<ScriptLine> blockNodeData) : base(config, blockNodeData)
		{

		}
		public override IEnumerator<ScriptNode> EnumerateThrough(int localIndex)
		{
			yield break;
		}
		internal IEnumerator<ScriptNode> ElseEnumerate()
		{
			using var enumerator = base.EnumerateThrough(0);
			while (enumerator.MoveNext())
				yield return enumerator.Current;
		}
	}
}