namespace B1NARY
{
	using System.IO;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Text.RegularExpressions;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.InputSystem;
	using B1NARY.DesignPatterns;
	using B1NARY.Scripting.Experimental;
	using B1NARY.Scripting;
	using B1NARY.UI;
	using System;
	using System.Collections;

	public class ScriptParser : SingletonAlt<ScriptParser>
	{
		public PlayerInput playerInput;
		[HideInInspector]
		public bool scriptChanged = false;
		public string scriptName;
		public bool paused = false;
		bool choice = false;

		// public int lineIndex = 0;
		// public int continueIndex = 0;

		public Dictionary<string, AudioClip> voiceLines { get; set; }

		string Path => $"{Application.streamingAssetsPath}/Docs/{scriptName}.txt";

		public Dictionary<string, List<string>> currentChoiceOptions;

		public DialogueNode currentNode;

		protected override void SingletonAwake()
		{
			DontDestroyOnLoad(gameObject);
			playerInput.actions.FindAction("Mouse Press", true).performed += OnClick;
			playerInput.actions.FindAction("Fast Skip", true).performed += OnClick;
		}

		public void Initialize()
		{
			ChangeScriptFile(scriptName, 0);
			//reader = new StreamReader(Path);
			//currentNode = new DialogueNode(GetLines());
			// readNextLine();

			//CurrentParsedLineTask = ParseLine(currentNode.GetCurrentLine(), true);
		}

		public void ChangeScriptFile(string newScript, int position = 0)
		{
			scriptChanged = true;
			currentNode = new DialogueNode(GetLines(Path, scriptName));
			// This is fucking retarded. I don't remember why I did this and now
			// I'm too afraid to change it
			position -= 2;
			position = Mathf.Clamp(position, 0, int.MaxValue);
			currentNode.moveIndex(position);
			scriptChanged = false;
			PlayLine(new ScriptLine(currentNode.GetCurrentLine())).FreeBlockPath();
		}

		DialogueLine[] GetLines(string path, string documentName)
		{
			var lines = new List<DialogueLine>();
			using (var streamReader = new StreamReader(path))
				for (int i = 1; !streamReader.EndOfStream; i++)
					lines.Add(new DialogueLine(streamReader.ReadLine(), i, documentName));
			return lines.ToArray();
		}

		private void OnClick(InputAction.CallbackContext callbackContext)
		{
			//if (SceneManager.IsSwitchingScenes)
			//	return;
			if (currentNode == null)
				return;
			if (DialogueSystem.Instance.SpeakingTask.IsCompleted)
			{
				var scriptLine = new ScriptLine(currentNode.nextLine());
				PlayLine(scriptLine).FreeBlockPath();/*.ContinueWith(task => { if (task.IsFaulted) 
						Debug.LogException(task.Exception); })*/
			}
			else
				// if the dialogue is still being written out just skip to the end of the line
				DialogueSystem.Instance.StopSpeaking(true).Wait();
		}
		public void PlayVA(DialogueLine line)
		{
			string currentSpeaker = DialogueSystem.Instance.CurrentSpeaker;
			if (CharacterManager.Instance.charactersInScene.TryGetValue(currentSpeaker, out GameObject charObject))
				charObject.GetComponent<CharacterScript>().Speak(currentSpeaker, new ScriptLine(line));
			else
				Debug.LogError($"Character '{currentSpeaker}' does not exist, on line {line.index}!");
		}

		/*
		public void ParseLine(DialogueLine Line)
		{
			if (Line == null || paused)
				return;
			var line = new ScriptLine(Line);
			switch (line.type)
			{
				case ScriptLine.Type.Normal:
					PlayVA(Line);
					CharacterManager.Instance.changeLightingFocus();
					DialogueSystem.Instance.Say(line.lineData);
					break;
				case ScriptLine.Type.Emotion:
					string expression = ScriptLine.CastEmotion(line);
					CharacterManager.Instance.changeExpression(DialogueSystem.Instance.CurrentSpeaker, expression);
					goto skipToNextLine;
				case ScriptLine.Type.Speaker:
					string speaker = ScriptLine.CastSpeaker(line);
					DialogueSystem.Instance.CurrentSpeaker = speaker;
					goto skipToNextLine;
				case ScriptLine.Type.Command:
					CommandsManager.HandleWithArgs(line);
					if (scriptChanged)
						break;
					goto skipToNextLine;
				case ScriptLine.Type.BeginIndent:
				case ScriptLine.Type.EndIndent:
					throw new ArgumentException("Managed to hit a intentation"
						+ $"on line '{line.Index}'.");
				case ScriptLine.Type.Empty:
				default:
					goto skipToNextLine;
				skipToNextLine:
					if (ReadNextLine(out DialogueLine dialogueLine))
						ParseLine(dialogueLine);
					break;
			}
		}

		bool ReadNextLine(out DialogueLine scriptLine)
		{
			if (currentNode != null)
			{
				scriptLine = currentNode.GetCurrentLine();
				return true;
			}
			scriptLine = null;
			return false;
		}
		*/
		public async Task PlayLine(ScriptLine line)
		{
			if (paused)
				return;
			if (currentNode == null)
				return;
			switch (line.type)
			{
				case ScriptLine.Type.Normal:
					PlayVA((DialogueLine)line);
					CharacterManager.Instance.changeLightingFocus();
					DialogueSystem.Instance.Say(line.lineData);
					break;
				case ScriptLine.Type.Emotion:
					string expression = ScriptLine.CastEmotion(line);
					CharacterManager.Instance.changeExpression(DialogueSystem.Instance.CurrentSpeaker, expression);
					goto skipToNextLine;
				case ScriptLine.Type.Speaker:
					string speaker = ScriptLine.CastSpeaker(line);
					DialogueSystem.Instance.CurrentSpeaker = speaker;
					goto skipToNextLine;
				case ScriptLine.Type.Command:
					CommandsManager.HandleWithArgs(line);
					if (scriptChanged)
						break;
					goto skipToNextLine;
				case ScriptLine.Type.BeginIndent:
				case ScriptLine.Type.EndIndent:
					throw new ArgumentException("Managed to hit a intentation"
						+ $"on line '{line.Index}'.");
				case ScriptLine.Type.Empty:
				default:
					Debug.LogError($"There seems to be an enum as '{line.type}' that is not part of the switch command case. Skipping.");
					goto skipToNextLine;
				skipToNextLine:
					var nextLine = new ScriptLine(currentNode.nextLine());
					await PlayLine(nextLine);
					break;
			}
		}
		/*
		[Obsolete]
		public void PerformNextLine(DialogueLine line = null)
		{
			if (currentNode == null || paused)
				return;
			line = line == null ? currentNode.GetCurrentLine() : line;
			if (line == null)
				return;
			var scriptLine = new ScriptLine(line);
			switch (scriptLine.type)
			{
				case ScriptLine.Type.Normal:
					PlayVA(line);
					CharacterManager.Instance.changeLightingFocus();
					DialogueSystem.Instance.Say(scriptLine.lineData);
					break;
				case ScriptLine.Type.Emotion:
					string expression = ScriptLine.CastEmotion(scriptLine);
					CharacterManager.Instance.changeExpression(DialogueSystem.Instance.CurrentSpeaker, expression);
					goto skipToNextLine;
				case ScriptLine.Type.Speaker:
					string speaker = ScriptLine.CastSpeaker(scriptLine);
					DialogueSystem.Instance.CurrentSpeaker = speaker;
					goto skipToNextLine;
				case ScriptLine.Type.Command:
					CommandsManager.HandleWithArgs(scriptLine);
					if (scriptChanged)
						break;
					goto skipToNextLine;
				case ScriptLine.Type.BeginIndent:
				case ScriptLine.Type.EndIndent:
					throw new ArgumentException("Managed to hit a intentation"
						+ $"on line '{scriptLine.Index}'.");
				case ScriptLine.Type.Empty:
				default:
					goto skipToNextLine;
				skipToNextLine:
					PerformNextLine();
					break;
			}
		}
		*/
	}
}