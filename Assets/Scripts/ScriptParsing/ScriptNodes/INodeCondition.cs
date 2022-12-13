namespace B1NARY.Scripting
{
	using System;

	/// <summary>
	/// A delegate that is related to the <see cref="ScriptNode.ScriptNode(ScriptDocument, ScriptPair[])"/>
	/// constructor.
	/// </summary>
	/// <param name="document"> The reference of the document to tie to. </param>
	/// <param name="subLines"> The contents of the <see cref="ScriptNode"/> </param>
	/// <returns> A node, or a class that derived from it. </returns>
	public delegate TNode ScriptNodeParser<TNode>(ScriptDocument document, ScriptPair[] subLines, int index) where TNode : ScriptNode;
	public struct NodeConditionReader
	{
		private readonly Func<ScriptPair[], bool> condition;
		private readonly ScriptNodeParser<ScriptNode> nodeCreator;
		public NodeConditionReader(Func<ScriptPair[], bool> condition, ScriptNodeParser<ScriptNode> nodeCreator)
		{
			this.condition = condition;
			this.nodeCreator = nodeCreator;
		}
		public bool Predicate(ScriptPair[] pairs) => condition(pairs);
		public ScriptNode Create(ScriptDocument document, ScriptPair[] subLines, int index) => nodeCreator(document, subLines, index);
	}
	//[AttributeUsage(AttributeTargets.Class)]
	//public sealed class NodeCommandConditionAttribute : Attribute
	//{
	//	public static bool TryGetAttribute<TClass>(TClass @class, out NodeCommandConditionAttribute attribute) where TClass : class
	//	{
	//		var undefinedAtt = GetCustomAttribute(@class.GetType(), typeof(NodeCommandConditionAttribute), true);
	//		if (undefinedAtt is null)
	//		{
	//			attribute = (NodeCommandConditionAttribute)undefinedAtt;
	//			return true;
	//		}
	//		attribute = null;
	//		return false;
	//	}
	//
	//	public string CommandCondition { get; }
	//	public NodeCommandConditionAttribute(string condition)
	//	{
	//		CommandCondition = condition;
	//	}
	//
	//	public bool Predicate(ScriptLine line)
	//	{
	//		if (line.type != ScriptLine.Type.Command)
	//			return false;
	//		string command = ScriptLine.CastCommand(line).command;
	//		if (command == CommandCondition)
	//			return true;
	//		return false;
	//	}
	//}
}