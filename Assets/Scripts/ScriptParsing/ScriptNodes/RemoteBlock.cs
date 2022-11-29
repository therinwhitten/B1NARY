namespace B1NARY.Scripting
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public sealed class RemoteBlock : ScriptNode
	{
		public static new bool Predicate(ScriptPair[] pairs)
		{
			if (pairs[0].scriptLine.type != ScriptLine.Type.Command)
				return false;
			var command = ScriptLine.CastCommand(pairs[0].scriptLine);
			if (command.arguments.Length != 1)
				return false;
			if (command.command != "remote")
				return false;
			return true;
		}
		public static Func<bool, IEnumerator<ScriptLine>> CallRemote(ScriptDocument document, string key)
		{
			Dictionary<string, RemoteBlock> blockDictionary = (
				from node in document.nodes
				where node is RemoteBlock
				select (RemoteBlock)node).ToDictionary(node => node.Key);
			if (blockDictionary.TryGetValue(key, out var defaultNode))
				return (pauseOnCommands) => defaultNode.RemoteCall(pauseOnCommands);
			Debug.LogWarning($"'{key}' remote node is not found in the document!\nAvailible Node Names: {string.Join(", ", blockDictionary.Keys)}");
			return null;
		}

		public readonly string Key;

		public RemoteBlock(ScriptDocument document, ScriptPair[] subLines, int index) : base(document, subLines, index)
		{
			Key = ScriptLine.CastCommand(rootLine).arguments.Single();
		}

		public override IEnumerator<ScriptLine> Perform(bool pauseOnCommands)
		{
			Debug.Log("Bruh");
			yield break;
		}

		public IEnumerator<ScriptLine> RemoteCall(bool pauseOnCommands)
		{
			using (var enumerator = base.Perform(pauseOnCommands))
				while (enumerator.MoveNext())
					yield return enumerator.Current;
		}
	}
}