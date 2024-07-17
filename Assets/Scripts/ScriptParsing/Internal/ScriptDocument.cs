namespace B1NARY.Scripting
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using UnityEngine;
	using System.IO;
	using System.Linq;
	using OVSSerializer.IO;

	public class ScriptDocument : ScriptElement
	{
		public static readonly HashSet<string> enabledHashset = new HashSet<string>()
		{ "on", "true", "enable" };
		public static readonly HashSet<string> disabledHashset = new HashSet<string>()
		{ "off", "false", "disable" };

		/// <summary>
		/// The file location it was read off of. May be <see langword="null"/>.
		/// </summary>
		public virtual OSFile ReadFile { get; set; }
		public ScriptDocumentConfig DocumentConfig { get; }
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

		public ScriptDocument(ScriptDocumentConfig config, OSFile file) : this(config, file.ReadAllText())
		{
			ReadFile = file;
		}
		public ScriptDocument(ScriptDocumentConfig config, string contents) : base(config, Init(contents))
		{
			DocumentConfig = config;
		}
		private static List<ScriptLine> Init(string fileContents)
		{
			string[] contentsArray = fileContents.Split('\n');
			List<ScriptLine> allLines = new(contentsArray.Length + 3);
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
					DocumentConfig.InvokeNormal(line);
					return false;
				case ScriptLine.LineType.Command:
					var (command, @params) = ScriptLine.CastCommand(line);
					return !DocumentConfig.Commands.B1NARY_Invoke(command, @params);
				case ScriptLine.LineType.Attribute:
					DocumentConfig.InvokeAttribute(ScriptLine.CastAttribute(line));
					return true;
				case ScriptLine.LineType.Empty:
					return true;
				case ScriptLine.LineType.Entry:
					DocumentConfig.InvokeEntry(ScriptLine.CastEntry(line));
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

		public IDocumentWatcher StartAtLine(int line) => new Watcher(EnumerateWatcher(line));
		public override IEnumerator<ScriptNode> EnumerateThrough(int localIndex)
		{
			using var enumerator = base.EnumerateThrough(localIndex);
			while (enumerator.MoveNext())
			{
				ScriptNode currentNode = enumerator.Current;
				if (currentNode is null)
					yield return null;
				if (InvokeLine(currentNode.PrimaryLine) && !DocumentConfig.stopOnAllLines)
					continue;
				yield return currentNode;
			}
		}
		internal IEnumerator<(bool canSkip, ScriptNode currentNode)> EnumerateWatcher(int localIndex)
		{
			using var enumerator = base.EnumerateThrough(localIndex);
			while (enumerator.MoveNext())
			{
				ScriptNode currentNode = enumerator.Current;
				if (currentNode is null)
					yield return (false, null);
				bool canSkip = InvokeLine(currentNode.PrimaryLine) && !DocumentConfig.stopOnAllLines;
				yield return (canSkip, currentNode);
			}
		}

		public IDocumentWatcher Start() => StartAtLine(0);


		public class Watcher : IDocumentWatcher
		{
			private IEnumerator<(bool canSkip, ScriptNode currentNode)> enumerator;
			public Watcher(IEnumerator<(bool canSkip, ScriptNode currentNode)> enumerator)
			{
				this.enumerator = enumerator;
			}

			public bool NextNode(out ScriptNode node)
			{
				restart:
				if (EndOfDocument)
				{
					node = null;
					return false;
				}	
				bool output = enumerator.MoveNext();
				if (!output)
					EndOfDocument = true;
				node = enumerator.Current.currentNode;
				if (enumerator.Current.canSkip)
					goto restart;
				return output;
			}

			public ScriptNode CurrentNode => enumerator.Current.currentNode;
			public bool EndOfDocument { get; private set; } = false;
		}
	}
}
