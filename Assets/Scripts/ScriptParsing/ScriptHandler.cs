﻿namespace B1NARY.Scripting.Experimental
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
	using System.Threading;

	public class ScriptHandler : Singleton<ScriptHandler>
	{
		public static readonly IEnumerable<KeyValuePair<string, Delegate>> Commands = new Dictionary<string, Delegate>()
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
			["usegameobject"] = (Action<string>)(gameObjectName =>
			{
				GameObject @object = GameObject.Find(gameObjectName);
				if (@object == null)
					throw new MissingMemberException($"Gameobject '{gameObjectName}' is not found");
				@object.SetActive(true);
				Instance.ShouldPause = true;
				Task.Run(() =>
				{
					SpinWait.SpinUntil(() => !@object.activeSelf);
					Instance.ShouldPause = false;
				}).FreeBlockPath();
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
		/// If the game should pause input. Modifying the variable 
		/// can be stacked with other scripts to allow compatibility. This comes 
		/// into play if you a script allows it to continue, but another says it
		/// shouldn't. In this case, it will read <see langword="true"/>.
		/// </summary>
		public bool ShouldPause 
		{ 
			get => m_pauseIterations > 0;
			set { if (value) m_pauseIterations++; else m_pauseIterations = checked(m_pauseIterations - 1); }
		}
		private uint m_pauseIterations = 0;
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
			scriptFactory.AddCommandFunctionality(SFXAudioController.Commands);
			scriptFactory.AddCommandFunctionality(SceneManager.Commands);
			//scriptFactory.AddCommandFunctionality(DialogueSystem.DialogueDelegateCommands);
			scriptFactory.AddCommandFunctionality(ScriptHandler.Commands);
			scriptFactory.AddCommandFunctionality(B1NARY.CharacterController.Commands);
			scriptFactory.AddCommandFunctionality(TransitionManager.Commands);
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
			if (ShouldPause || scriptDocument == null)
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

	public interface IScriptCommands
	{
		IEnumerable<KeyValuePair<string, Delegate>> Commands { get; }
	}
}