namespace B1NARY.Scripting.Experimental
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UnityEngine;
	using B1NARY.UI;
	using System.Linq;

	public class ScriptHandler : MonoBehaviour
	{
		public static void PlayVoiceActor(ScriptLine line)
		{
			string currentSpeaker = DialogueSystem.Instance.CurrentSpeaker;
			if (CharacterManager.Instance.charactersInScene.TryGetValue(currentSpeaker, out GameObject charObject))
				charObject.GetComponent<CharacterScript>().Speak(currentSpeaker, line);
			else
				Debug.LogError($"Character '{currentSpeaker}' does not exist!");
		}
		public static ScriptNode GetDefinedScriptNodes(Func<ScriptLine, bool> parseLine, ScriptPair[] subLines)
		{
			string command = ScriptLine.CastCommand(subLines.First().scriptLine).command;
			switch (command.Trim().ToLower())
			{
				case "if":
					return new IfBlock(parseLine, subLines);
				case "choice":
					return new ChoiceBlock(parseLine, subLines);
			}
			return null;
		}


		private ScriptDocument scriptDocument;
		[SerializeField]
		private string scriptName;

		public void InitializeNewScript()
		{
			var scriptDocument = new ScriptDocument.Factory(scriptName);
			scriptDocument.AddNodeParserFunctionality(GetDefinedScriptNodes);
			this.scriptDocument = (ScriptDocument)scriptDocument;
		}

	}
}