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


		public override IEnumerator<ScriptLine> Perform()
		{
			if (!CanPerform)
				yield break;
			base.Perform();
		}
	}
}