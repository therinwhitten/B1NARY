namespace B1NARY.Scripting
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	[NodeCommandCondition("remote")]
	public class RemoteBlock : ScriptNode
	{
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