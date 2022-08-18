namespace B1NARY.Scripting.Experimental
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using B1NARY.DataPersistence;
	using UnityEngine;

	public sealed class IfBlock : ScriptNode
	{
		public IfBlock(int rootListIndex, Func<IReadOnlyList<ScriptLine>> list,
			Func<IReadOnlyDictionary<int, ScriptNode>> nodeList) : base(rootListIndex, list, nodeList)
		{

		}

		public override IEnumerator Perform(HandleLine line)
		{
			var (command, arguments) = ScriptLine.CastCommand(ScriptData[rootListIndex]);
			bool canPerform = bool.Parse(arguments[1]);
			if (PersistentData.bools.ContainsKey(command))
				canPerform = PersistentData.bools[command];
			if (!canPerform)
			{
				int bracket = -1;
				if (ScriptData[rootListIndex + 1].type != ScriptLine.Type.BeginIndent)
					throw new Exception();
				else
					bracket++;
				for (NewIndex = rootListIndex + 2; true; NewIndex++)
				{
					if (ScriptData[NewIndex].type == ScriptLine.Type.BeginIndent)
						bracket++;
					else if (ScriptData[NewIndex].type == ScriptLine.Type.EndIndent)
					{
						bracket--;
						if (bracket < 0)
						{
							NewIndex++;
							Debug.Log($"Finished ScriptNode with rootLine: {ScriptData[rootListIndex]}");
							yield break;
						}
					}
				}
			}
			IEnumerator trueValue = base.Perform(line);
			while (trueValue.MoveNext())
				yield return null;
		}
	}
}