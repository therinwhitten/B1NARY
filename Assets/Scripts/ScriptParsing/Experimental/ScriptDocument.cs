namespace B1NARY.Scripting.Experimental
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Linq;
	using UnityEngine;

	[Serializable]
	public sealed class ScriptDocument : ScriptElement, IEnumerable<ScriptNode>
	{
		/// <summary>
		/// The file location it was read off of. May be <see langword="null"/>.
		/// </summary>
		public FileInfo ReadFile { get; }
		private readonly ScriptDocumentConfig documentConfig;

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

		internal bool InvokeLine(ScriptLine line)
		{
			switch (line.Type)
			{
				case ScriptLine.LineType.Normal:
					documentConfig.InvokeNormal();
					return false;
				case ScriptLine.LineType.Command:
					var (command, @params) = ScriptLine.CastCommand(line);
					return !documentConfig.Commands.B1NARY_Invoke(command, @params);
				case ScriptLine.LineType.Attribute:
					documentConfig.InvokeAttribute(ScriptLine.CastAttribute(line));
					//if (!CharacterController.Instance.ChangeActiveCharacter(speaker))
					//	Debug.LogError($"There is no character named '{speaker}'!");
					return true;
				case ScriptLine.LineType.Empty:
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

		public IEnumerator<ScriptNode> StartAtLine(int line)
		{
			// Initialize values
			ScriptNode currentNode = Lines[line];
			while (currentNode is ScriptElementPointer pointer)
			{
				currentNode = pointer.target.Lines[pointer.localPoint];
			}

			ScriptElement currentElement = currentNode.Parent;
			for (int i = currentElement.ToLocal(line); i < currentElement.LinesWithElements.Count; i++)
			{
				while (InvokeLine(currentElement.LinesWithElements[i].PrimaryLine))
					continue;
				yield return currentElement.LinesWithElements[i];
			}
		}

		public new IEnumerator<ScriptNode> GetEnumerator() => StartAtLine(0);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	}

}
