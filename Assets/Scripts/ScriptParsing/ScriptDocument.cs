namespace B1NARY.Scripting.Experimental
{
	using System.IO;
	using System.Collections.Generic;

	public class ScriptDocument
	{
		public readonly string documentName;
		// starts at 1, indexes at 0.
		public IReadOnlyList<ScriptLine> RawScriptContents => data;
		public IReadOnlyDictionary<int, ScriptNode> ScriptNodes => scriptNodes;
		private readonly List<ScriptLine> data;
		private readonly Dictionary<int, ScriptNode> scriptNodes;

		private int _index = -1;
		public int ScriptIndex => ListIndex + 1;
		public int ListIndex
		{
			get => _index;
			private set
			{
				if (value <= _index)
					return;
				for (int i = _index; i <= value; i++)
				{
					_index++;
					// Do on-site parsing here.
				}
			}
		}

		public ScriptLine NextLine()
		{
			ListIndex++;
			return data[ListIndex];
		}

		public ScriptDocument(string fullFilePath, params ScriptNodeParser[] nodeParsers)
		{
			this.nodeParsers = nodeParsers;
			documentName = Path.GetFileNameWithoutExtension(fullFilePath);
			data = new List<ScriptLine>();
			using (var streamReader = new StreamReader(new FileStream(fullFilePath, FileMode.Open)))
				for (int i = 0; !streamReader.EndOfStream; i++)
				{
					string dataLine = streamReader.ReadLine();
					data.Add(new ScriptLine(dataLine, () => documentName, i + 1));
				}
			for (int i = 0; i < RawScriptContents.Count; i++)
				if (DetermineIfScriptNode(i))
					scriptNodes.Add(i, ParseNode(i));
		}

		private bool DetermineIfScriptNode(int listIndex) =>
			RawScriptContents[listIndex + 1].type == ScriptLine.Type.BeginIndent;
		private ScriptNodeParser[] nodeParsers;
		private ScriptNode ParseNode(int listIndex)
		{
			for (int i = 0; i < nodeParsers.Length; i++)
			{
				ScriptNode definedNode = nodeParsers[i].Invoke(listIndex, () => RawScriptContents, () => ScriptNodes);
				if (definedNode != null)
					return definedNode;
			}
			return new ScriptNode(listIndex, () => RawScriptContents, () => ScriptNodes);
		}
	}
}