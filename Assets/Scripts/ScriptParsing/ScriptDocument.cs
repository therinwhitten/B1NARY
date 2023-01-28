﻿namespace B1NARY.Scripting
{
	using System;
	using System.IO;
	using System.Collections.Generic;
	using System.Linq;
	using B1NARY.UI;
	using CharacterController = CharacterManagement.CharacterController;
	using System.Collections.ObjectModel;
	using UnityEngine;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Reflection;

	/// <summary>
	/// A document that tracks the <see cref="ScriptNode"/>s and parses information
	/// from the given file document.
	/// </summary>
	public sealed class ScriptDocument : IDisposable
	{
		/// <summary>
		/// Saves the contents as a binary file. 
		/// </summary>
		public static void SerializeContents(string path)
		{
			string[] lines = File.ReadAllLines(path);
			int endIndex = path.LastIndexOf('.');
			string otherPath = path.Remove(endIndex) + ".rdsgpijoersgpiojsegrpiojgerspiojgers";
			using (var stream = new FileStream(otherPath, FileMode.Create))
				new BinaryFormatter().Serialize(stream, lines);
		}
		/// <summary>
		/// Takes the incoming stream that iterates the contents of the 
		/// <see cref="ScriptNode"/>, and it has just hit a detected scriptNode
		/// from the currentLine, which this commands skips to the end bracket.
		/// </summary>
		/// <param name="start"> the enumeration to reference. </param>
		/// <returns> 
		/// The same <see cref="IEnumerator{ScriptPair}"/>, but skipped the detected
		/// <see cref="ScriptNode"/>.
		/// </returns>
		public static IEnumerator<ScriptPair> SkipNode(IEnumerator<ScriptPair> start, ScriptNode node)
		{
			if (start.Current.scriptLine.Index > node.endIndex)
				throw new InvalidOperationException($"index of '{start.Current.scriptLine.Index}' is greater than '{node.endIndex}'");
			int oldIndex = start.Current.scriptLine.Index;
			while (start.Current.scriptLine.Index != node.endIndex)
				start.MoveNext();
			//start.MoveNext(); // To skip the end bracket.
			//Debug.Log($"Skipped from {oldIndex} to {start.Current.scriptLine}");
			return start;
		}

		public static readonly HashSet<string> enabledHashset = new HashSet<string>()
		{ "on", "true", "enable" };
		public static readonly HashSet<string> disabledHashset = new HashSet<string>()
		{ "off", "false", "disable" };


		/// <summary>
		/// The document's file name in the <see cref="documentPath"/>.
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// The full document path that is located on the drive.
		/// </summary>
		public FileInfo DocumentPath { get; private set; }
		/// <summary>
		/// The path that takes to the general voicelines.
		/// </summary>
		public string VoiceResourcesPath { get; private set; }
		/// <summary>
		/// The current line it stopped at, usually at dialogue. You may be 
		/// able to get a command via multiple threads.
		/// </summary>
		public ScriptLine CurrentLine => data.Current;
		private IEnumerator<ScriptLine> data;
		/// <summary> A readonly array of <see cref="ScriptLine"/>. </summary>
		public ReadOnlyCollection<ScriptLine> documentData;
		private IReadOnlyDictionary<string, OverloadableCommand> commands;

		/// <summary>
		/// All nodes attached to the document. Does not include the base node
		/// that it starts with.
		/// </summary>
		public ReadOnlyCollection<ScriptNode> nodes;

		/// <summary>
		/// This is <see langword="null"/> by default. This is a return value when
		/// commands return something; such as the <see cref="RemoteBlock"/> as 
		/// an example.
		/// </summary>
		public Func<bool, IEnumerator<ScriptLine>> returnValue;

		private ScriptDocument()
		{

		}

		/// <summary>
		/// Moves to the next line, automatically executes commands before stopping.
		/// </summary>
		/// <returns>A line that does not have any commands, emotions, etc.</returns>
		/// <exception cref="IndexOutOfRangeException">it has reached to end of scriptName.</exception>
		public ScriptLine NextLine()
		{
			if (data.MoveNext())
			{
				//using (var reader = new StreamWriter(new FileStream($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Gay.txt"}", FileMode.Append)))
				//	reader.WriteLine(data.Current);
				return data.Current;
			}
			Dispose();
			// It should constantly move to other scripts to prevent this from happening.
			throw new IndexOutOfRangeException($"{nameof(ScriptDocument)} has reached to end of scriptName.");
		}
		/// <summary>
		/// Parses the line to do various things. Commands to interact with the
		/// scene with the scriptName, emotions to show emotes to the current speaker,
		/// changing speakers, etc.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if it can go for another line without 
		/// any worries of pausing. Otherwise, <see langword="false"/>
		/// </returns>
		public bool ParseLine(ScriptLine line)
		{
			DebugLineTester.Instance.AddLine(line.lineData);
			switch (line.type)
			{
				case ScriptLine.Type.Normal:
					CharacterController.Instance.ActiveCharacter.SayLine(line);
					return false;
				case ScriptLine.Type.Emotion:
					string expression = ScriptLine.CastEmotion(line);
					CharacterController.Instance.ActiveCharacter.CurrentExpression = expression;
					return true;
				case ScriptLine.Type.Speaker:
					string speaker = ScriptLine.CastSpeaker(line);
					if (!CharacterController.Instance.ChangeActiveCharacter(speaker))
						Debug.LogError($"There is no character named '{speaker}'!");
					return true;
				case ScriptLine.Type.Command:
					return !OverloadableCommand.Invoke(commands, ScriptLine.CastCommand(line));
				case ScriptLine.Type.DocumentFlag:
				case ScriptLine.Type.BeginIndent:
				case ScriptLine.Type.EndIndent:
					throw new ArgumentException("Managed to hit a intentation "
						+ $"on line '{line.Index}'.");
				case ScriptLine.Type.Empty:
					return true;
				default:
					Debug.LogError($"There seems to be an enum as '{line.type}'" +
						" that is not part of the switch command case. Skipping.");
					return true;
			}
		}

		public void Dispose()
		{
			data.Dispose();
		}




		//*-------------- FACTORY ----------------*//
		/// <summary>
		/// A initializable factory that has various commands to build up 
		/// functionality of <see cref="ScriptDocument"/>.
		/// </summary>
		public sealed class Factory
		{
			public static explicit operator ScriptDocument(Factory factory)
				=> factory.Parse(false);

			/// <summary>
			/// The document's file name in the <see cref="documentPath"/>.
			/// </summary>
			public readonly string Name;
			/// <summary>
			/// The full document path that is located on the drive.
			/// </summary>
			public readonly FileInfo documentPath;
			/// <summary>
			/// The path that takes to the general voicelines.
			/// </summary>
			public readonly string voiceResourcesPath;
			/// <summary>
			/// A collection of parsers that allows them make them more defined
			/// and causes different behaviour as such.
			/// </summary>
			private readonly List<NodeCondition> scriptNodeParsers = 
				new List<NodeCondition>();
			/// <summary>
			/// A type-writer like way to read through the file while compiling
			/// the document.
			/// </summary>
			private Func<ScriptDocument, IEnumerator<ScriptLine>> fileData;
			/// <summary>
			/// A collection of accumulated commands to convert them as a 
			/// <see cref="Lookup{string, Delegate}"/> to invoke.
			/// </summary>
			public readonly CommandArray commands = new CommandArray();

			/// <summary>
			/// Creates an instance of the <see cref="Factory"/> of 
			/// <see cref="ScriptDocument"/>.
			/// </summary>
			/// <param name="fullFilePath"> 
			/// a full path to get the document and define <see cref="fileData"/>
			/// and <see cref="documentPath"/> and <see cref="documentName"/>.
			/// </param>
			/// <exception cref="ArgumentException"/>
			public Factory(FileInfo filePath)
			{
				if (!filePath.Exists)
					throw new ArgumentException($"{filePath.FullName} does not lead to a playable file!");
				documentPath = filePath;
				Name = filePath.Name.Remove(filePath.Name.LastIndexOf('.'));
				voiceResourcesPath = "Voice\\" + ScriptHandler.ToVisual(filePath);
				// TODO: add a way to parse it over time.
				fileData = LineReader;
				IEnumerator<ScriptLine> LineReader(ScriptDocument document)
				{
					using (var reader = new StreamReader(filePath.Open(FileMode.Open, FileAccess.Read)))
						for (int i = 1; !reader.EndOfStream; i++)
							yield return new ScriptLine(reader.ReadLine(), document, i);
				}
			}
			/// <summary>
			/// Adds a single or multiple methods that further defines the
			/// <see cref="ScriptNode"/> to add behaviour to it.
			/// </summary>
			/// <param name="scriptNodeParsers"> The parsers to add. </param>
			public void AddNodeParserFunctionality(params Type[] nodes)
			{
				for (int i = 0; i < nodes.Length; i++)
					AddNodeParserFunctionality(nodes[i]);
			}

			/// <summary>
			/// Adds a single or multiple methods that further defines the
			/// <see cref="ScriptNode"/> to add behaviour to it.
			/// </summary>
			/// <param name="scriptNodeParsers"> The parsers to add. </param>
			public void AddNodeParserFunctionality(Type node)
			{
				if (!node.IsSubclassOf(typeof(ScriptNode)))
					throw new InvalidOperationException($"'{node.Name}' is not derived from '{nameof(ScriptNode)}'!");
				if (node == typeof(ScriptNode))
					return;
				if (scriptNodeParsers.Any(nodeCon => nodeCon.type == node))
					return;
				Func<ScriptPair[], bool> func = pairs => true;
				if (ConditionAttribute.TryGetAttribute(node, out ConditionAttribute attribute))
					func = attribute.Predicate;
				ConstructorInfo info = node.GetConstructor(new Type[] { typeof(ScriptDocument), typeof(ScriptPair[]), typeof(int) });
				if (info == null)
					throw new MissingMethodException($"class '{node.Name}' has " +
						"a missing default constructor!");
				ScriptNodeParser<ScriptNode> scriptNodeParser = (doc, sublines, index) =>
					(ScriptNode)info.Invoke(new object[] { doc, sublines, index });
				scriptNodeParsers.Add(new NodeCondition(node, func, scriptNodeParser));
			}

			/// <summary>
			/// Compiles all information in the <see cref="Factory"/>.
			/// </summary>
			/// <param name="pauseOnCommand">
			/// Because <see cref="IEnumerator{T}"/> will have their method argument
			/// persists, you determine if the document should pause their commands
			/// or not, by default, this should be <see langword="true"/>.
			/// </param>
			/// <returns> The fully completed document, ready to use. </returns>
			/// <exception cref="InvalidDataException"/>
			public ScriptDocument Parse(bool pauseOnCommand)
			{
				var output = new ScriptDocument()
				{
					Name = Name,
					DocumentPath = documentPath,
					VoiceResourcesPath = voiceResourcesPath,
				};
				// Assigning lines
				var list = new List<ScriptPair>();
				var nodes = new List<(int startIndex, int endIndex, int indentation)>();
				var incompletePairs = new Stack<int>();
				IEnumerator<ScriptLine> fileData = this.fileData(output);
				while (fileData.MoveNext())
				{
					if (fileData.Current.type == ScriptLine.Type.BeginIndent)
					{
						// line number to array index + under the bracket.
						incompletePairs.Push(fileData.Current.Index - 2);
					}
					else if (fileData.Current.type == ScriptLine.Type.EndIndent)
					{
						int indentation = incompletePairs.Count;
						if (incompletePairs.Count == 0)
							throw new InvalidDataException(fileData.Current.ToString() +
								" is a end Indent, but there is no start indent to end with!");
						int startIndex = incompletePairs.Pop();
						// line number to array index.
						nodes.Add((startIndex, fileData.Current.Index - 1, indentation));
					}
					list.Add((ScriptPair)fileData.Current);
				}
				fileData.Dispose();
				if (incompletePairs.Count > 0)
					throw new InvalidDataException($"{Name} has " +
						$"incomplete indentations/brackets!");
				output.documentData = Array.AsReadOnly(
					list.Select(pair => pair.scriptLine).ToArray());
				// Merging all dictionaries.
				output.commands = commands;
#if DEBUG
				Debug.Log($"VV --- All commands --- VV\n{string.Join("\n", output.commands.Select(pair => $"'{pair.Key}' lengths: {string.Join(", ", pair.Value.Select(del => del.Method.GetParameters().Length))}"))}");
#endif
				// Assigning nodes, highest first
				IEnumerator<(int startIndex, int endIndex)> nodeQueue = 
					nodes.OrderByDescending(pair => pair.indentation)
					.Select(triple => (triple.startIndex, triple.endIndex))
					.GetEnumerator();
				var nodeArray = new ScriptNode[nodes.Count];
				
				for (int i = 0; nodeQueue.MoveNext(); i++)
				{
					var (startIndex, endIndex) = nodeQueue.Current;
					var subArray = list.Skip(startIndex)
						.Take(endIndex - startIndex + 1)
						.ToArray();
					nodeArray[i] = ParseNode(output, subArray, i);
					list[startIndex] = new ScriptPair(list[startIndex].scriptLine, nodeArray[i]);
				}
				// Messing with the baseline entry scriptnode code.
				list.InsertRange(0, new ScriptPair[]
				{
					(ScriptPair)new ScriptLine("::Start", output, -1),
					(ScriptPair)new ScriptLine("{", output, 0)
				});
				list.Add((ScriptPair)new ScriptLine("::End", output, list.Count));
				output.nodes = Array.AsReadOnly(nodeArray);
				output.data = new ScriptNode(output, list.ToArray(), -1).Perform(pauseOnCommand);
				return output;
			}

			/// <summary>
			/// Whenever a possible <see cref="ScriptNode"/> is found, then it will
			/// iterate through the parsers.
			/// </summary>
			/// <param name="document"> The document to reference. </param>
			/// <param name="subValues"> The group of data the scriptNode is allowed to use. </param>
			/// <returns> The defined or default <see cref="ScriptNode"/>. </returns>
			private ScriptNode ParseNode(ScriptDocument document, ScriptPair[] subValues, int index)
			{
				for (int i = 0; i < scriptNodeParsers.Count; i++)
				{
					if (!scriptNodeParsers[i].Predicate(subValues))
						continue;
					return scriptNodeParsers[i].Create(document, subValues, index);//.Invoke(new object[] { document, subValues, index });
				}
				return new ScriptNode(document, subValues, index);
			}
		}

	}
}