using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;


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
			else if (ScriptData[NewIndex].type != ScriptLine.Type.EndIndent)
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
//*/