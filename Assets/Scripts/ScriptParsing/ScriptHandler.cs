using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class ScriptHandler : MonoBehaviour
{
	public static ScriptNode GetDefinedScriptNodes(int rootListIndex, Func<IReadOnlyList<ScriptLine>> list, Func<IReadOnlyDictionary<int, ScriptNode>> nodes)
	{
		var (command, arguments) = ScriptLine.CastCommand(list.Invoke()[rootListIndex]);
		switch (command.Trim().ToLower())
		{
			case "if":
				return new IfBlock(rootListIndex, list, nodes);
			case "choice":
				return new ChoiceBlock(rootListIndex, list, nodes);
		}
		return null;
	}


	private ScriptDocument scriptDocument;
	[SerializeField]
	private string scriptName;

	public void InitializeNewScript()
	{
		scriptDocument = new ScriptDocument(scriptName, GetDefinedScriptNodes);
	}

	IEnumerator scriptNodeEnumerator;
	private ScriptNode scriptNodeData;
	public Task NextLine()
	{
		if (scriptNodeEnumerator != null)
			if (scriptNodeEnumerator.MoveNext())
				return Task.CompletedTask;
			else
			{
				scriptNodeEnumerator = null;

				scriptNodeData = null;
			}

		ScriptLine currentLine = scriptDocument.NextLine();
		if (scriptDocument.ScriptNodes.TryGetValue(scriptDocument.ListIndex, out ScriptNode scriptNode))
		{
			scriptNodeEnumerator = scriptNode.Perform(ParseLine);
			scriptNodeData = scriptNode;
			scriptNodeEnumerator.MoveNext();
		}
		else
			ParseLine(currentLine);
		return Task.CompletedTask;
	}

	private void ParseLine(ScriptLine line)
	{
		switch (line.type)
		{
			case ScriptLine.Type.Normal:
				PlayVA(line);
				CharacterManager.Instance.changeLightingFocus();
				DialogueSystem.Instance.Say(line.lineData);
				break;
		}
	}
	private void PlayVA(ScriptLine line)
	{
		string currentSpeaker = DialogueSystem.Instance.currentSpeaker;
		if (CharacterManager.Instance.charactersInScene.TryGetValue(currentSpeaker, out GameObject charObject))
			charObject.GetComponent<CharacterScript>().Speak(currentSpeaker, line);
		else
			Debug.LogError($"Character '{currentSpeaker}' does not exist!");
	}
}