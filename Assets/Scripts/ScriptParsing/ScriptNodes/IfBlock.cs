﻿namespace B1NARY.Scripting
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using B1NARY.DataPersistence;
	using UnityEngine;

	public sealed class IfBlock : ScriptNode
	{
		public IfBlock(ScriptDocument scriptDocument, ScriptPair[] subLines) : base(scriptDocument, subLines)
		{

		}
		/// <summary>
		/// Gets a value indicating whether this block can perform.
		/// </summary>
		public bool CanPerform
		{
			get // {if: Name, Bool}
			{
				string[] arguments = ((Command)rootLine).arguments;
				bool canPerform = bool.Parse(arguments[1]);
				if (PersistentData.Instance.bools.TryGetValue(arguments[0], out bool value))
					canPerform = value;
				return canPerform;
			}
		}


		public override IEnumerator<ScriptLine> Perform(bool pauseOnCommands)
		{
			Debug.Log($"Starting If Statement in line {rootLine.Index}: {CanPerform}");
			if (!CanPerform)
			{
				// The else block behaviour

				yield break;
				// TODO: Add 'else'
			}
			IEnumerator<ScriptLine> @base = base.Perform(pauseOnCommands);
			while (@base.MoveNext())
				yield return @base.Current;
		}
	}
}