﻿namespace B1NARY.Scripting.Experimental
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using B1NARY.DataPersistence;
	using UnityEngine;

	public sealed class IfBlock : ScriptNode
	{
		public IfBlock(Func<ScriptLine, bool> parseLine, ScriptPair[] subLines) : base(parseLine, subLines)
		{

		}
		/// <summary>
		/// Gets a value indicating whether this block can perform.
		/// </summary>
		public bool CanPerform
		{
			get // {if: Name, Bool}
			{
				string[] arguments = ScriptLine.CastCommand(rootLine).arguments;
				bool canPerform = bool.Parse(arguments[1]);
				if (PersistentData.bools.TryGetValue(arguments[0], out bool value))
					canPerform = value;
				return canPerform;
			}
		}


		public override IEnumerator<ScriptLine> Perform(bool pauseOnCommands)
		{
			if (!CanPerform)
				yield break;
			IEnumerator<ScriptLine> @base = base.Perform(pauseOnCommands);
			while (@base.MoveNext())
				yield return @base.Current;
		}
	}
}