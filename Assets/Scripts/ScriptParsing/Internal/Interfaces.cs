namespace B1NARY.Scripting
{
	using System;

	public interface IPointerConverter
	{
		int ToLocal(int globalPoint);
		int ToGlobal(int localPoint);
	}
	public interface IDocumentWatcher
	{
		bool NextNode(out ScriptNode line);
		ScriptNode CurrentNode { get; }
		bool EndOfDocument { get; }
	}
}