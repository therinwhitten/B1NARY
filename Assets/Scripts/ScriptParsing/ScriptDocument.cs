namespace B1NARY.Scripting.Experimental
{
	using System;
	using System.IO;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using B1NARY.UI;
	using System.Collections.ObjectModel;
	using UnityEngine;

	// working on this made me realize this is just a really fancy ScriptNode.
	// - oh well!
	public sealed class ScriptDocument
	{
		


		public readonly string documentName;
		public ScriptLine CurrentLine => data.Current;
		private IEnumerator<ScriptLine> data;
		public ReadOnlyCollection<ScriptLine> documentData;
		public IReadOnlyDictionary<int, ScriptNode> ScriptNodes => scriptNodes;
		private readonly Dictionary<int, ScriptNode> scriptNodes;

		private Dictionary<string, Delegate> commands;

		public bool AdditiveEnabled { get; private set; } = false;

		private ScriptDocument()
		{
			
		}

		

		/// <summary>
		/// Moves to the next line, automatically executes commands before stopping.
		/// </summary>
		/// <returns>A line that does not have any commands, emotions, etc.</returns>
		/// <exception cref="IndexOutOfRangeException">it has reached to end of script.</exception>
		public ScriptLine NextLine()
		{
			if (data.MoveNext())
			{
				if (ParseLine(data.Current))
					return NextLine();
				return data.Current;
			}
			// It should constantly move to other scripts to prevent this from happening.
			throw new IndexOutOfRangeException($"{nameof(ScriptDocument)} has reached to end of script.");
		}

		/// <summary>
		/// Parses the line to do various things. Commands to interact with the
		/// scene with the script, emotions to show emotes to the current speaker,
		/// changing speakers, etc.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if it can go for another line without 
		/// any worries of pausing. Otherwise, <see langword="false"/>
		/// </returns>
		public bool ParseLine(ScriptLine line)
		{
			switch (line.type)
			{
				case ScriptLine.Type.Normal:
					ScriptHandler.PlayVoiceActor(line);
					CharacterManager.Instance.changeLightingFocus();
					DialogueSystem.Instance.Say(line.lineData);
					return false;
				case ScriptLine.Type.Emotion:
					string expression = ScriptLine.CastEmotion(line);
					CharacterManager.Instance.changeExpression(DialogueSystem.Instance.CurrentSpeaker, expression);
					return true;
				case ScriptLine.Type.Speaker:
					string speaker = ScriptLine.CastSpeaker(line);
					DialogueSystem.Instance.CurrentSpeaker = speaker;
					return true;
				case ScriptLine.Type.Command:
					var (command, arguments) = ScriptLine.CastCommand(line);
					if (commands.TryGetValue(command, out Delegate @delegate))
						@delegate.DynamicInvoke(arguments);
					else
						throw new Exception();
					return true;
				case ScriptLine.Type.BeginIndent:
				case ScriptLine.Type.EndIndent:
					throw new ArgumentException("Managed to hit a intentation"
						+ $"on line '{line.Index}'.");
				case ScriptLine.Type.Empty:
				default:
					Debug.LogError($"There seems to be an enum as '{line.type}' that is not part of the switch command case. Skipping.");
					return true;
			}
		}

		public sealed class Factory
		{
			public static explicit operator ScriptDocument(Factory factory)
				=> factory.Parse();

			private readonly string documentName;
			private readonly ScriptLine[] scriptLines;
			private readonly List<IReadOnlyDictionary<string, Delegate>> categorizedCommands
				= new List<IReadOnlyDictionary<string, Delegate>>();
			private readonly List<ScriptNodeParser> scriptNodeParsers =
				new List<ScriptNodeParser>();

			public Factory(string fullFilePath)
			{
				documentName = Path.GetFileNameWithoutExtension(fullFilePath);
				// TODO: add a way to parse it over time.
				string[] rawLines = File.ReadAllLines(fullFilePath);
				int i = 0;
				scriptLines = rawLines.Select(str => { i++; return new ScriptLine(str, () => documentName, i); }).ToArray();
			}
			public void AddNodeParserFunctionality(params ScriptNodeParser[] scriptNodeParsers)
				=> this.scriptNodeParsers.AddRange(scriptNodeParsers);
			public void AddCommandFunctionality(params IReadOnlyDictionary<string, Delegate>[] categorizedCommands) 
				=> this.categorizedCommands.AddRange(categorizedCommands);
			public ScriptDocument Parse()
			{
				var output = new ScriptDocument();
				// Assigning lines
				var list = new List<ScriptPair>();
				var nodes = new List<(int startIndex, int endIndex, int indentation)>();
				(int startIndex, int indentation)? incompletePair = null;
				for (int i = 0, indent = 0; i < scriptLines.Length; i++)
				{
					ScriptLine line = scriptLines[i];
					if (i > 0) // Index underflow check
						if (line.type == ScriptLine.Type.BeginIndent)
						{
							indent++;
							incompletePair = (i - 1, indent);
						}
						else if (line.type == ScriptLine.Type.EndIndent)
						{
							indent--;
							if (indent < 0)
								throw new Exception();
							nodes.Add((incompletePair.Value.startIndex, i, incompletePair.Value.indentation));
							incompletePair = null;
						}
					list.Add((ScriptPair)line);
				}
				output.documentData = Array.AsReadOnly(list.Select(pair => pair.scriptLine).ToArray());
				// Merging all dictionaries.
				output.commands = categorizedCommands.SelectMany(dict => dict).ToDictionary(pair => pair.Key, pair => pair.Value);
				// Assigning nodes, highest first
				nodes = nodes.OrderByDescending(pair => pair.indentation).ToList();
				for (int i = 0; i < nodes.Count; i++)
				{
					var subArray = new ScriptPair[nodes[i].endIndex - nodes[i].startIndex + 1];
					for (int ii = 0; i < subArray.Length; i++)
						subArray[ii] = list[nodes[i].startIndex + ii];
					list[nodes[i].startIndex] = new ScriptPair(list[nodes[i].startIndex].scriptLine, ParseNode(output.ParseLine, subArray));
				}
				output.data = new EntryNode(output.ParseLine, list.ToArray()).Perform();
				return output;
			}

			private ScriptNode ParseNode(Func<ScriptLine, bool> parseLine, ScriptPair[] subValues)
			{
				for (int i = 0; i < scriptNodeParsers.Count; i++)
				{
					ScriptNode output = scriptNodeParsers[i].Invoke(parseLine, subValues);
					if (output != null)
						return output;
				}
				return new ScriptNode(parseLine, subValues);
			}
		}

	}
	/*
	public class ScriptDocument //: IEnumerable<(bool isBlock, ScriptLine line)>
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
	*/
}