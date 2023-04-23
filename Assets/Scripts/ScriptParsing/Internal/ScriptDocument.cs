namespace B1NARY.Scripting
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using UnityEngine;
	using System.IO;
	using System.Linq;

	public class ScriptDocument : ScriptElement
	{
		public static readonly HashSet<string> enabledHashset = new HashSet<string>()
		{ "on", "true", "enable" };
		public static readonly HashSet<string> disabledHashset = new HashSet<string>()
		{ "off", "false", "disable" };

		/// <summary>
		/// The file location it was read off of. May be <see langword="null"/>.
		/// </summary>
		public virtual FileInfo ReadFile { get; set; }
		private readonly ScriptDocumentConfig documentConfig;
		public IReadOnlyList<ScriptElement> AllElements
		{
			get
			{
				if (m_allElements is null)
				{
					List<ScriptElement> allElements = new List<ScriptElement>();
					Recursive(this);
					m_allElements = allElements;
					void Recursive(ScriptElement subElement)
					{
						allElements.AddRange(subElement.Elements);
						for (int i = 0; i < subElement.Elements.Count; i++)
							Recursive(subElement.Elements[i]);
					}
				}
				return m_allElements;
			}
		}
		private IReadOnlyList<ScriptElement> m_allElements;

		public ScriptDocument(ScriptDocumentConfig config, FileInfo file) : this(config, File.ReadAllText(file.FullName))
		{
			ReadFile = file;
		}
		public ScriptDocument(ScriptDocumentConfig config, string contents) : base(config, Init(contents))
		{
			documentConfig = config;
		}
		private static List<ScriptLine> Init(string fileContents)
		{
			string[] contentsArray = fileContents.Split('\n');
			List<ScriptLine> allLines = new List<ScriptLine>(contentsArray.Length + 3);
			allLines.Add(new ScriptLine("Document", -1));
			allLines.Add(new ScriptLine("::start", 0));
			for (int i = 0; i < contentsArray.Length; i++)
				allLines.Add(new ScriptLine(contentsArray[i], i + 1));
			allLines.Add(new ScriptLine("::end", 0));
			return allLines;
		}

		public override int ToLocal(int globalPoint)
		{
			return globalPoint;
		}
		public override int ToGlobal(int localPoint)
		{
			return localPoint;
		}

		internal bool InvokeLine(ScriptLine line)
		{
			switch (line.Type)
			{
				case ScriptLine.LineType.Normal:
					documentConfig.InvokeNormal(line);
					return false;
				case ScriptLine.LineType.Command:
					var (command, @params) = ScriptLine.CastCommand(line);
					return !documentConfig.Commands.B1NARY_Invoke(command, @params);
				case ScriptLine.LineType.Attribute:
					documentConfig.InvokeAttribute(ScriptLine.CastAttribute(line));
					return true;
				case ScriptLine.LineType.Empty:
					return true;
				case ScriptLine.LineType.Entry:
					documentConfig.InvokeEntry(ScriptLine.CastEntry(line));
					return true;
				case ScriptLine.LineType.DocumentFlag:
				case ScriptLine.LineType.EndIndent:
				case ScriptLine.LineType.BeginIndent:
					throw new InvalidCastException($"Line is an indent!: {line}");
				default:
					Debug.LogError($"There seems to be an enum as '{line.Type}'" +
						" that is not part of the switch command case. Skipping.");
					return true;
			}
		}

		public IDocumentWatcher StartAtLine(int line) => new Watcher(EnumerateThrough(line));
		public override IEnumerator<ScriptNode> EnumerateThrough(int localIndex)
		{
			using (var enumerator = base.EnumerateThrough(localIndex))
				while (enumerator.MoveNext())
				{
					ScriptNode currentNode = enumerator.Current;
					if (currentNode is null)
						yield return null;
					if (InvokeLine(currentNode.PrimaryLine) && !documentConfig.stopOnAllLines)
						continue;
					yield return currentNode;
				}
		}

		public IDocumentWatcher Start() => StartAtLine(0);


		public class Watcher : IDocumentWatcher
		{
			private IEnumerator<ScriptNode> enumerator;
			public Watcher(IEnumerator<ScriptNode> enumerator)
			{
				this.enumerator = enumerator;
			}

			public bool NextNode(out ScriptNode node)
			{
				if (EndOfDocument)
				{
					node = null;
					return false;
				}	
				bool output = enumerator.MoveNext();
				if (!output)
					EndOfDocument = true;
				node = enumerator.Current;
				return output;
			}

			public ScriptNode CurrentNode => enumerator.Current;
			public bool EndOfDocument { get; private set; } = false;
		}
	}
}
