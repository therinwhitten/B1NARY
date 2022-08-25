namespace B1NARY.Scripting.Experimental
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using UnityEngine;
	
	public delegate ScriptNode ScriptNodeParser(Func<ScriptLine, bool> parseLine, ScriptPair[] subLines);
	public class ScriptNode
	{
		/// <summary>
		/// The line where it has been located right above 
		/// <see cref="ScriptLine.Type.BeginIndent"/>
		/// </summary>
		public readonly ScriptLine rootLine;
		/// <summary>
		/// The total 'Array' size, <see cref="SubLines"/> and <see cref="rootLine"/>
		/// included.
		/// </summary>
		public readonly int lineLength;
		/// <summary>
		/// A readonly array of the contents. <see cref="ScriptLine.Type.BeginIndent"/>
		/// is at the very start and <see cref="ScriptLine.Type.EndIndent"/> at
		/// the end is guaranteed.
		/// </summary>
		public ReadOnlyCollection<ScriptPair> SubLines
			=> Array.AsReadOnly(subLines);
		protected ScriptPair[] subLines;
		protected readonly Func<ScriptLine, bool> parseLine;
		public ScriptNode(Func<ScriptLine, bool> parseLine, ScriptPair[] subLines)
		{
			this.parseLine = parseLine;
			lineLength = subLines.Length;
			rootLine = subLines.First().scriptLine;
			this.subLines = subLines.Skip(1).ToArray();
		}
		/// <summary>
		/// Performs <see cref="ScriptLine"/> and uses <see cref="ScriptDocument.ParseLine(ScriptLine)"/>
		/// to determine to yield return a value or continue.
		/// </summary>
		public virtual IEnumerator<ScriptLine> Perform()
		{
			// i as 1 to skip the bracket, length - 1 for the same.
			for (int i = 1; i < subLines.Length - 1; i++)
			{
				if (subLines[i].HasScriptNode)
				{
					IEnumerator<ScriptLine> subNode = subLines[i].scriptNode.Perform();
					for (int ii = 0; subNode.MoveNext(); ii++)
						yield return subNode.Current;
					i += subLines[i].scriptNode.lineLength;
				}
				if (parseLine.Invoke(subLines[i].scriptLine))
					continue;
				yield return subLines[i].scriptLine;
			}
		}
	}
}
	/*
	public delegate ScriptNode ScriptNodeParser(int rootListIndex, Func<IReadOnlyList<ScriptLine>> list, Func<IReadOnlyDictionary<int, ScriptNode>> nodes);
	public delegate void HandleLine(ScriptLine line);
	public class ScriptNode
	{
		public int NewIndex { get; protected set; }
		public readonly int rootListIndex;
		public int RootScriptIndex => rootListIndex + 1;
		protected IReadOnlyList<ScriptLine> ScriptData => _scriptData.Invoke();
		private readonly Func<IReadOnlyList<ScriptLine>> _scriptData;
		protected IReadOnlyDictionary<int, ScriptNode> NodeList => _nodeList.Invoke();
		private readonly Func<IReadOnlyDictionary<int, ScriptNode>> _nodeList;

		public ScriptNode(int rootListIndex, Func<IReadOnlyList<ScriptLine>> list, Func<IReadOnlyDictionary<int, ScriptNode>> nodeList)
		{
			_nodeList = nodeList;
			NewIndex = rootListIndex;
			this.rootListIndex = rootListIndex;
			_scriptData = list;
		}
		public virtual IEnumerator Perform(HandleLine line)
		{
			int bracket = -1;
			if (ScriptData[rootListIndex + 1].type != ScriptLine.Type.BeginIndent)
				throw new Exception();
			else
				bracket++;
			for (NewIndex = rootListIndex + 2; true; NewIndex++)
			{
				if (ScriptData[NewIndex].type == ScriptLine.Type.BeginIndent)
					bracket++;
				else if (ScriptData[NewIndex].type == ScriptLine.Type.EndIndent)
				{
					bracket--;
					if (bracket < 0)
					{
						NewIndex++;
						Debug.Log($"Finished ScriptNode with rootLine: {ScriptData[rootListIndex]}");
						yield break;
					}
				}
				else
				{
					if (NodeList.TryGetValue(NewIndex, out ScriptNode scriptNode))
					{
						IEnumerator enumerator = scriptNode.Perform(line);
						Debug.Log($"Starting a nested ScriptNode of '{scriptNode.ScriptData[scriptNode.rootListIndex]}'");
						while (enumerator.MoveNext())
							yield return null;
						NewIndex = scriptNode.NewIndex;
						continue;
					}
					else
						line.Invoke(ScriptData[NewIndex]);
				}
				yield return null;
			}
		}
	}

	/*
	public sealed class ScriptDocument
	{
		private ScriptDocument scriptData;

		private ScriptDocument(ScriptDocument scriptData)
		{
			this.scriptData = scriptData;
		}

		/*
		public ScriptLine CurrentLine { get; private set; }
		public int Index
		{
			get => CurrentLine.Index == _index ? _index : throw new IndexOutOfRangeException(
				$"'{nameof(_index)}' value of {_index} is out of sync with " +
				$"{nameof(CurrentLine)} of {CurrentLine.Index}!");
			set
			{
				if (value < _index)
					return;
			}
		}
		private int _index = -1;


		public ScriptDocument(string fullFilePath)
		{

		}

		public ScriptLine NextLine()
		{


			return CurrentLine;
		}
		*/