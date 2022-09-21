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

		public GameObject dialogueSystemPrefab;
		/// <summary> Gets the name of the script loaded. </summary>
		public string ScriptName { get; private set; } = string.Empty;
		/// <summary> 
		/// A read-only version of the currently stored 
		/// <see cref="Experimental.ScriptDocument"/>. 
		/// </summary>
		public ScriptDocument ScriptDocument => scriptDocument;
		private ScriptDocument scriptDocument;
		[Tooltip("Where the script starts when it initializes a new script.")]
		public string StartupScriptPath;
		public PlayerInput playerInput;
		public string[] nextLineButtons;
		/// <summary>
		/// A value that determines if it is running a script and ready to use.
		/// </summary>
		public bool IsActive { get; private set; } = false;
		/// <summary> Gets the current line. </summary>
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
				DialogueSystem.Instance.Say(line.lineData);
			});
			scriptFactory.AddCommandFunctionality(
				SFXAudioController.AudioDelegateCommands, 
				SceneManager.SceneDelegateCommands,
				DialogueSystem.DialogueDelegateCommands,
				ScriptHandler.ScriptDelegateCommands,
				B1NARY.CharacterController.CharacterDelegateCommands,
				TransitionManager.TransitionDelegateCommands);
			scriptDocument = (ScriptDocument)scriptFactory;
			IsActive = true;
		}
		public void PlayVoiceActor(ScriptLine line)
		{
			string currentSpeaker = DialogueSystem.Instance.CurrentSpeaker;
			if (B1NARY.CharacterController.Instance.charactersInScene.TryGetValue(currentSpeaker, out var charObject))
				charObject.characterScript.SayLine(line);
			else
				Debug.LogError($"Character '{currentSpeaker}' does not exist!");
		}
		/// <summary>
		/// Plays lines until it hits a normal dialogue or similar.
		/// </summary>
		/// <returns> The <see cref="ScriptLine"/> it stopped at. </returns>
		public Task<ScriptLine> NextLine()
		{
			if (scriptDocument == null)
				return Task.FromResult(default(ScriptLine));
			try
			{
				return Task.FromResult(scriptDocument.NextLine());
			}
			catch (IndexOutOfRangeException)
			{
				IsActive = false;
				throw;
			}
		}
	}
}