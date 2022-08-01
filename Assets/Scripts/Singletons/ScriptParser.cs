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
	using B1NARY.ScriptingBeta;
	using B1NARY.UI;

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

		StreamReader reader = null;

		// public string currentNode.GetCurrentLine() { get { return currentNode.GetCurrentLine(); } }

		// regex for grabbing rich text tags
		Regex richRegex = new Regex("<(.*?)>");

		// regex for grabbing expressions
		Regex emoteRegex = new Regex("\\[(.*?)\\]");

		// regex for commands
		Regex commandRegex = new Regex("\\{(.*?)\\}");
		public DialogueNode currentNode;

		public Task<ScriptLine> CurrentParsedLineTask { get; private set; }

		// Awake is called before the first frame update
		protected override void SingletonAwake()
		{
			DontDestroyOnLoad(gameObject);
			playerInput.actions.FindAction("Mouse Press", true).performed += OnClick;
			playerInput.actions.FindAction("Fast Skip", true).performed += OnClick;
		}

		public Task Initialize()
		{
			// lineIndex = 0;
			B1NARYConsole.Log(nameof(ScriptParser), "Initializing..");
			ChangeScriptFile(scriptName, 0);
			return Task.CompletedTask;
			//reader = new StreamReader(Path);
			//currentNode = new DialogueNode(GetLines());
			// readNextLine();

			//CurrentParsedLineTask = ParseLine(currentNode.GetCurrentLine(), true);
		}

		public void ChangeScriptFile(string newScript, int position = 0)
		{
			B1NARYConsole.Log(nameof(ScriptParser), $"Changing script to {newScript} as pos {position}");
			scriptName = newScript;
			reader = new StreamReader(Path);
			currentNode = new DialogueNode(GetLines());
			// This is fucking retarded. I don't remember why I did this and now
			// I'm too afraid to change it
			position -= 2;
			position = Mathf.Clamp(position, 0, int.MaxValue);
			currentNode.moveIndex(position);
			scriptChanged = false;

			CurrentParsedLineTask = ParseLine(currentNode.GetCurrentLine());
		}

		List<DialogueLine> GetLines()
		{
			B1NARYConsole.Log(nameof(ScriptParser), $"Reading All lines of '{Path}'..");
			List<DialogueLine> lines = new List<DialogueLine>();
			int i = 1;
			while (!reader.EndOfStream)
			{
				DialogueLine line = new DialogueLine(reader.ReadLine(), i, scriptName);
				lines.Add(line);
				i++;
			}
			reader.Close();
			return lines;
		}

		private void OnClick(InputAction.CallbackContext callbackContext)
		{
			if (!TransitionHandler.CommandsAllowed)
				return;
			if (DialogueSystem.Instance.SpeakingTask.IsCompleted)
			{
				ReadNextLine();
				if (currentNode != null)
					_ = ParseLine(currentNode.GetCurrentLine());
			}
			else
			// if the dialogue is still being written out just skip to the end of the line
			{
				_ = DialogueSystem.Instance.StopSpeaking(true);
			}
		}
		public void PlayVA(DialogueLine line)
		{
			string currentSpeaker = DialogueSystem.Instance.CurrentSpeaker;
			B1NARYConsole.Log(nameof(ScriptParser), $"Playing VoiceLine '{line.index}'" +
				$"of character '{currentSpeaker}'");
			if (CharacterManager.Instance.charactersInScene.TryGetValue(currentSpeaker, out GameObject charObject))
				charObject.GetComponent<CharacterScript>().Speak(currentSpeaker, new ScriptLine(line));
			else
				B1NARYConsole.LogError(nameof(ScriptParser), $"Character '{currentSpeaker}' does not exist!");
		}
		public async Task<ScriptLine> ParseLine(DialogueLine Line, bool @override = false)
		{
			if (Line == null || paused)
				return default;
			var line = new ScriptLine(Line);
			Task task = @override ? Task.CompletedTask :
				Task.Run(() => SpinWait.SpinUntil(() => TransitionHandler.CommandsAllowed));
			await task;
			switch (line.type)
			{
				case ScriptLine.Type.Normal:
					PlayVA(Line);
					CharacterManager.Instance.changeLightingFocus();
					DialogueSystem.Instance.Say(line.lineData);
					return line;
				// CHANGING EXPRESSIONS
				// expressions in the script will be written like this: [happy]
				// expressions must be on their own lines 
				case ScriptLine.Type.Emotion:
					string expression = ScriptLine.CastEmotion(line);
					CharacterManager.Instance.changeExpression(DialogueSystem.Instance.CurrentSpeaker, expression);
					ReadNextLine();
					await ParseLine(currentNode.GetCurrentLine());
					return line;
				case ScriptLine.Type.Speaker:
					string speaker = ScriptLine.CastSpeaker(line);
					DialogueSystem.Instance.CurrentSpeaker = speaker;
					ReadNextLine();
					await ParseLine(currentNode.GetCurrentLine());
					return line;
				// COMMANDS
				// These will be any other type of commands 
				// that aren't rich text tags or emotion controls
				case ScriptLine.Type.Command:
					var (command, arguments) = ScriptLine.CastCommand(line);
					CommandsManager.HandleWithArgs(command, arguments);
					if (scriptChanged)
						return line;
					ReadNextLine();
					await ParseLine(currentNode.GetCurrentLine());
					return line;
				case ScriptLine.Type.BeginIndent:
				case ScriptLine.Type.EndIndent:
				case ScriptLine.Type.Empty:
				default:
					ReadNextLine();
					await ParseLine(currentNode.GetCurrentLine());
					return line;
			}
		}

		private void OnApplicationQuit()
		{
			if (reader != null)
				reader.Close();
		}
		void ReadNextLine()
		{
			if (currentNode != null)
				currentNode.nextLine();
		}
	}
}