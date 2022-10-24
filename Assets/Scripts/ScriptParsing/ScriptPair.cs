namespace B1NARY.Scripting
{
	using System;

	public struct ScriptPair
	{
		public static explicit operator ScriptPair(ScriptLine line)
			=> new ScriptPair(line, null);
		public ScriptPair(ScriptLine line, ScriptNode node)
		{
			scriptLine = line;
			scriptNode = node;
		}

		public readonly ScriptLine scriptLine;
		public ScriptNode scriptNode;
		public bool HasScriptNode => scriptNode != null;
		public ScriptLine.Type LineType => scriptLine.type;
		public void Deconstruct(out ScriptLine line, out ScriptNode node)
		{
			line = scriptLine;
			node = scriptNode;
		}
		public override string ToString() => $"{nameof(ScriptPair)} {{ {nameof(scriptLine)}: {scriptLine}, {nameof(scriptNode)}: {scriptNode} }}";
	}
}
