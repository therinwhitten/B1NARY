namespace B1NARY.Scripting.Experimental
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UnityEngine;
	using B1NARY.UI;
	using System.Linq;
	using B1NARY.Audio;
	using B1NARY.DesignPatterns;
	using UnityEngine.InputSystem;

	public class ScriptHandler : SingletonAlt<ScriptHandler>
	{
		public static IReadOnlyDictionary<string, Delegate> ScriptDelegateCommands = new Dictionary<string, Delegate>()
		{
			["additive"] = (Action<string>)(boolRaw =>
			{
				bool setting;
				boolRaw = boolRaw.ToLower().Trim();
				if (ScriptDocument.enabledHashset.Contains(boolRaw))
					setting = true;
				else if (ScriptDocument.disabledHashset.Contains(boolRaw))
					setting = false;
				else
					throw new InvalidCastException(boolRaw);
				Instance.scriptDocument.AdditiveEnabled = setting;
				DialogueSystem.Instance.additiveTextEnabled = setting;
			}),
			["changescript"] = (Action<string>)(scriptPath =>
			{
				Instance.InitializeNewScript(Application.streamingAssetsPath + '/' + scriptPath);
			}),
		};
		public static ScriptNode GetDefinedScriptNodes(Func<ScriptLine, bool> parseLine, ScriptPair[] subLines)
		{
			if (subLines.First().LineType != ScriptLine.Type.Command)
				return null;
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

		public string ScriptName { get; private set; } = string.Empty;
		public ScriptDocument ScriptDocument => scriptDocument;
		private ScriptDocument scriptDocument;
		public string StartupScriptPath;
		public PlayerInput playerInput;
		public string[] nextLineButtons;
		/// <summary>
		/// A value that determines if it is running a scriptName.
		/// </summary>
		public bool IsActive { get; private set; } = false;
		public ScriptLine CurrentLine => scriptDocument.CurrentLine;
		private void Awake()
		{
			foreach (string key in nextLineButtons)
				playerInput.actions.FindAction(key, true).performed += context => NextLine().FreeBlockPath();
		}

		public void InitializeNewScript(string scriptPath = "")
		{
			string finalPath = string.IsNullOrWhiteSpace(scriptPath) ? StartupScriptPath : scriptPath;
			var scriptFactory = new ScriptDocument.Factory(finalPath);
			scriptFactory.AddNodeParserFunctionality(GetDefinedScriptNodes);
			scriptFactory.AddNormalOperationsFunctionality(line =>
			{
				PlayVoiceActor(line);
				CharacterManager.Instance.changeLightingFocus();
				DialogueSystem.Instance.Say(line.lineData);
			});
			scriptFactory.AddCommandFunctionality(
				AudioHandler.AudioDelegateCommands, 
				SceneManager.SceneDelegateCommands,
				DialogueSystem.DialogueDelegateCommands,
				ScriptHandler.ScriptDelegateCommands,
				CharacterManager.CharacterDelegateCommands, 
				TransitionManager.TransitionDelegateCommands);
			scriptDocument = (ScriptDocument)scriptFactory;
			IsActive = true;
		}
		public void PlayVoiceActor(ScriptLine line)
		{
			string currentSpeaker = DialogueSystem.Instance.CurrentSpeaker;
			if (CharacterManager.Instance.charactersInScene.TryGetValue(currentSpeaker, out GameObject charObject))
				charObject.GetComponent<CharacterScript>().Speak(currentSpeaker, line);
			else
				Debug.LogError($"Character '{currentSpeaker}' does not exist!");
		}
		public Task NextLine()
		{
			if (scriptDocument == null)
				return Task.CompletedTask;
			try
			{
				scriptDocument.NextLine();
			}
			catch (IndexOutOfRangeException)
			{
				IsActive = false;
				throw;
			}
			return Task.CompletedTask;
		}

	}
}