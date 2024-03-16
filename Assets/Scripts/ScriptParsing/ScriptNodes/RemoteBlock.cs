namespace B1NARY.Scripting
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	
	public class RemoteBlock : ScriptElement
	{
		public static Predicate<List<ScriptLine>> Predicate => (lines) => lines[0].Type == ScriptLine.LineType.Command && lines[0].RawLine.Contains("remote");
		public static IEnumerator<ScriptNode> CallRemote(ScriptDocument document, string key)
		{
			Dictionary<string, RemoteBlock> blockDictionary = (
				from node in document.AllElements
				where node is RemoteBlock
				select (RemoteBlock)node).ToDictionary(node => node.Key);
			if (blockDictionary.TryGetValue(key, out var defaultNode))
				return defaultNode.RemoteCall();
			Debug.LogWarning($"'{key}' remote node is not found in the document!\nAvailible Node Names: {string.Join(", ", blockDictionary.Keys)}");
			return null;
		}

		public readonly string Key;

		public RemoteBlock(ScriptDocumentConfig config, List<ScriptLine> blockNodeData) : base(config, blockNodeData)
		{
			Key = ScriptLine.CastCommand(PrimaryLine).arguments.Single();
		}

		public override IEnumerator<ScriptNode> EnumerateThrough(int localIndex)
		{
			yield break;
		}

		public IEnumerator<ScriptNode> RemoteCall()
		{
			return base.EnumerateThrough(0);
		}
	}
}