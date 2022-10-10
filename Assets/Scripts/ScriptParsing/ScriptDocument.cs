﻿namespace B1NARY.Scripting.Experimental
{
	using System;
	using System.IO;
	using System.Collections.Generic;
	using System.Linq;
	using B1NARY.UI;
	using System.Collections.ObjectModel;
	using UnityEngine;
	using System.Text;

	public sealed class ScriptDocument
	{
		public static readonly HashSet<string> enabledHashset = new HashSet<string>()
		{ "on", "true", "enable" };
		public static readonly HashSet<string> disabledHashset = new HashSet<string>()
		{ "off", "false", "disable" };

		/// <summary>
		/// The file name that the <see cref="ScriptDocument"/> is a part of.
		/// </summary>
		public readonly string documentName;
		/// <summary>
		/// The current line it stopped at, usually at dialogue. You may be 
		/// able to get a command via multiple threads.
		/// </summary>
		public ScriptLine CurrentLine => data.Current;
		private IEnumerator<ScriptLine> data;
		/// <summary> A readonly array of <see cref="ScriptLine"/>. </summary>
		public ReadOnlyCollection<ScriptLine> documentData;
		private Dictionary<string, Delegate> commands;

		/// <summary>
		/// If the current dialogue should be added instead of skipping to a new
		/// line.
		/// </summary>
		public bool AdditiveEnabled { get; set; } = false;

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
			switch (line.type)
			{
				case ScriptLine.Type.Normal:
					if (performLine != null)
						performLine.Invoke(line);
					else
						Debug.Log($"{nameof(performLine)} has no assignment for" +
							$" a speaking line to run. Printing; \n {line}");
					return false;
				case ScriptLine.Type.Emotion:
					string expression = ScriptLine.CastEmotion(line);
					B1NARY.CharacterController.Instance.charactersInScene[DialogueSystem.Instance.CurrentSpeaker].characterScript.ChangeExpression(expression);
					return true;
				case ScriptLine.Type.Speaker:
					string speaker = ScriptLine.CastSpeaker(line);
					DialogueSystem.Instance.CurrentSpeaker = speaker;
					return true;
				case ScriptLine.Type.Command:
					var (command, arguments) = ScriptLine.CastCommand(line);
					if (commands.ContainsKey(command) == false)
					{
						var exceptionBuilder = new StringBuilder($"{line} does not have a command for it!");
						if (arguments.Any())
						{
							exceptionBuilder.Append("\n Arguments: ");
							foreach (string argument in arguments)
								exceptionBuilder.Append($"{argument}, ");
						}
						throw new MissingMethodException(exceptionBuilder.ToString().TrimEnd(',', ' '));
					}
					commands[command].DynamicInvoke(arguments);
					return true;
				case ScriptLine.Type.DocumentFlag:
				case ScriptLine.Type.BeginIndent:
				case ScriptLine.Type.EndIndent:
					throw new ArgumentException("Managed to hit a intentation"
						+ $"on line '{line.Index}'.");
				case ScriptLine.Type.Empty:
					return true;
				default:
					Debug.LogError($"There seems to be an enum as '{line.type}' that is not part of the switch command case. Skipping.");
					return true;
			}
		}
		private Action<ScriptLine> performLine;


		//*-------------- FACTORY ----------------*//
		/// <summary>
		/// A initializable factory that has various commands to build up 
		/// functionality of <see cref="ScriptDocument"/>.
		/// </summary>
		public sealed class Factory
		{
			public static explicit operator ScriptDocument(Factory factory)
				=> factory.Parse(true);

			private readonly string documentName;
			private readonly List<IReadOnlyDictionary<string, Delegate>> categorizedCommands
				= new List<IReadOnlyDictionary<string, Delegate>>();
			private readonly List<ScriptNodeParser> scriptNodeParsers =
				new List<ScriptNodeParser>();
			private Action<ScriptLine> normalAction;
			private IEnumerator<ScriptLine> fileData;

			public Factory(string fullFilePath)
			{
				documentName = Path.GetFileNameWithoutExtension(fullFilePath);
				if (!File.Exists(fullFilePath))
					throw new ArgumentException($"{fullFilePath} does not lead to a playable file!");
				// TODO: add a way to parse it over time.
				fileData = LineReader();
				IEnumerator<ScriptLine> LineReader()
				{
					var reader = new StreamReader(fullFilePath);
					for (int i = 1; !reader.EndOfStream; i++)
						yield return new ScriptLine(reader.ReadLine(), () => documentName, i);
				}
			}
			public void AddNodeParserFunctionality(params ScriptNodeParser[] scriptNodeParsers)
				=> this.scriptNodeParsers.AddRange(scriptNodeParsers);
			public void AddCommandFunctionality(params IReadOnlyDictionary<string, Delegate>[] categorizedCommands) 
				=> this.categorizedCommands.AddRange(categorizedCommands);
			public void AddNormalOperationsFunctionality(Action<ScriptLine> action)
				=> normalAction = action;
			public ScriptDocument Parse(bool dontPauseOnCommand)
			{
				var output = new ScriptDocument();
				// Assigning lines
				var list = new List<ScriptPair>();
				var nodes = new List<(int startIndex, int endIndex, int indentation)>();
				var incompletePairs = new Stack<int>();
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
							throw new InvalidOperationException($"{fileData.Current} is a end Indent, but there is no start indent to end with!");
						int startIndex = incompletePairs.Pop();
						// line number to array index.
						nodes.Add((startIndex, fileData.Current.Index - 1, indentation));
					}
					list.Add((ScriptPair)fileData.Current);
				}
				output.documentData = Array.AsReadOnly(
					list.Select(pair => pair.scriptLine).ToArray());
				// Merging all dictionaries.
				output.commands = categorizedCommands
					.SelectMany(dict => dict)
					.ToDictionary(pair => pair.Key, pair => pair.Value);
				// Assigning nodes, highest first
				var nodeQueue = new Queue<(int startIndex, int endIndex)>
					(nodes.OrderByDescending(pair => pair.indentation)
					.Select(triple => (triple.startIndex, triple.endIndex)));
				
				while (nodeQueue.Count > 0)
				{
					var (startIndex, endIndex) = nodeQueue.Dequeue();
					var subArray = list.Skip(startIndex + 1)
						.Take(endIndex - startIndex + 1)
						.ToArray();
					list[startIndex] = new ScriptPair(list[startIndex].scriptLine,
						ParseNode(output.ParseLine, subArray));
				}
				// Messing with the baseline entry scriptnode code.
				list.InsertRange(0, new ScriptPair[]
				{
					(ScriptPair)new ScriptLine("::Start", () => documentName, -1),
					(ScriptPair)new ScriptLine("{", () => documentName, 0)
				});
				list.Add((ScriptPair)new ScriptLine("::End", () => documentName, list.Count));
				output.data = new ScriptNode(output.ParseLine, list.ToArray()).Perform(dontPauseOnCommand);
				output.performLine = normalAction;
				return output;

			}

			private ScriptNode ParseNode(Func<ScriptLine, bool> parseLine, ScriptPair[] subValues)
			{
				for (int i = 0; i < scriptNodeParsers.Count; i++)
				{
					ScriptNode node = scriptNodeParsers[i].Invoke(parseLine, subValues);
					if (node != null)
						return node;
				}
				return new ScriptNode(parseLine, subValues);
			}
		}

	}
}