namespace B1NARY.Scripting
{
	using System;
	using System.Threading;

	///// <summary>
	///// A delegate that is related to the <see cref="ScriptNode.ScriptNode(ScriptDocument, ScriptPair[])"/>
	///// constructor.
	///// </summary>
	///// <param name="document"> The reference of the document to tie to. </param>
	///// <param name="subLines"> The contents of the <see cref="ScriptNode"/> </param>
	///// <returns> A node, or a class that derived from it. </returns>
	public delegate TNode ScriptNodeParser<TNode>(ScriptDocument document, ScriptPair[] subLines, int index) where TNode : ScriptNode;
	public struct NodeCondition
	{
		private readonly Func<ScriptPair[], bool> condition;
		private readonly ScriptNodeParser<ScriptNode> nodeCreator;
		public readonly Type type;
		public NodeCondition(Type type, Func<ScriptPair[], bool> condition, ScriptNodeParser<ScriptNode> nodeCreator)
		{
			this.type = type;
			this.condition = condition;
			this.nodeCreator = nodeCreator;
		}
		public bool Predicate(ScriptPair[] pairs) => condition(pairs);
		public ScriptNode Create(ScriptDocument document, ScriptPair[] subLines, int index) => nodeCreator(document, subLines, index);
	}
	public sealed class NodeCommandConditionAttribute : ConditionAttribute
	{
		public string CommandCondition { get; }
		public NodeCommandConditionAttribute(string condition) : base()
		{
			CommandCondition = condition;
		}
		public override bool Predicate(ScriptPair[] pairs) => Predicate(pairs[0].scriptLine);
		public bool Predicate(ScriptLine line)
		{
			if (line.type != ScriptLine.Type.Command)
				return false;
			string command = ScriptLine.CastCommand(line).command;
			if (command == CommandCondition)
				return true;
			return false;
		}
	}

	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public abstract class ConditionAttribute : Attribute
	{
		public static bool TryGetAttribute<TAttribute>(in Type type, out TAttribute attribute) where TAttribute : ConditionAttribute
		{
			var undefinedAtt = GetCustomAttribute(type, typeof(TAttribute), true);
			if (undefinedAtt != null)
			{
				attribute = (TAttribute)undefinedAtt;
				return true;
			}
			attribute = null;
			return false;
		}
		public static bool TryGetAttribute<TClass, TAttribute>(out TAttribute attribute) where TClass : class where TAttribute : ConditionAttribute
		{
			var undefinedAtt = GetCustomAttribute(typeof(TClass), typeof(TAttribute), true);
			if (undefinedAtt != null)
			{
				attribute = (TAttribute)undefinedAtt;
				return true;
			}
			attribute = null;
			return false;
		}

		public ConditionAttribute() : base()
		{

		}

		public abstract bool Predicate(ScriptPair[] pairs);
	}
}