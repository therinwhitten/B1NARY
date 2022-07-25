using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using System;

public class ScriptParser : Singleton<ScriptParser>
{
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
	DialogueSystem dialogue { get { return DialogueSystem.Instance; } }

	StreamReader reader = null;

	// public string currentNode.GetCurrentLine() { get { return currentNode.GetCurrentLine(); } }

	// regex for grabbing rich text tags
	Regex richRegex = new Regex("<(.*?)>");

	// regex for grabbing expressions
	Regex emoteRegex = new Regex("\\[(.*?)\\]");

	// regex for commands
	Regex commandRegex = new Regex("\\{(.*?)\\}");
	public DialogueNode currentNode;



	// Start is called before the first frame update
	void Awake()
	{
		DontDestroyOnLoad(this.gameObject);
	}

	public override void initialize()
	{
		// lineIndex = 0;
		reader = new StreamReader(Path);
		currentNode = new DialogueNode(GetLines());
		// readNextLine();

		ParseLine(currentNode.GetCurrentLine());
	}

	public void ChangeScriptFile(string newScript, int position = 0)
	{
		scriptName = newScript;
		reader = new StreamReader(Path);
		currentNode = new DialogueNode(GetLines());
		// This is fucking retarded. I don't remember why I did this and now
		// I'm too afraid to change it
		position -= 2;
		position = Mathf.Clamp(position, 0, int.MaxValue);
		currentNode.moveIndex(position);
		scriptChanged = false;

		ParseLine(currentNode.GetCurrentLine());
	}

	List<DialogueLine> GetLines()
	{
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

	// Update is called once per frame
	void Update()
	{
		if (!TransitionHandler.CommandsAllowed)
			return;

		// TODO: Make sure to use the new input system!
		if ((Input.GetKeyDown(KeyCode.Space)
		|| Input.GetKeyDown(KeyCode.Mouse0)
		|| Input.GetKey(KeyCode.LeftControl))
		 && !paused)
		{
			if (!dialogue.isSpeaking || dialogue.isWaitingForUserInput)
			{
				// // if end of file has been reached
				// if (currentNode.endReached())
				// {
				//     if (currentNode.previous != null)
				//     {
				//         // if we reached the end of the node but there's a parent node,
				//         // continue from where we left off
				//         DialogueNode previousNode = currentNode.previous;
				//         currentNode = previousNode;
				//         currentNode.index--;
				//         parseLine(currentNode.GetCurrentLine());
				//     }
				//     else
				//     {
				//         return;
				//     }
				// }
				// else grab next line
				ReadNextLine();
				if (currentNode != null)
					ParseLine(currentNode.GetCurrentLine());
			}
			else
			// if the dialogue is still being written out just skip to the end of the line
			{
				dialogue.StopSpeaking();
				dialogue.speechText.text = dialogue.targetSpeech;
			}
		}
	}
	private void waitThenDO(Action action)
	{
		Instance.StartCoroutine(waitForTransitionsThenDo(action));
	}
	IEnumerator waitForTransitionsThenDo(Action action)
	{
		while (!TransitionHandler.CommandsAllowed)
		{
			yield return new WaitForEndOfFrame();
		}
		action();
	}
	public void PlayVA(DialogueLine Line)
	{
		string currentSpeaker = DialogueSystem.Instance.currentSpeaker;
		if (CharacterManager.Instance.charactersInScene.TryGetValue(currentSpeaker, out GameObject charObject))
			charObject.GetComponent<CharacterScript>().Speak(currentSpeaker, Line);
		else
			Debug.LogError($"Character '{currentSpeaker}' does not exist!");
	}
	public void ParseLine(DialogueLine Line)
	{

		// RICH TEXT
		// Unity already supports rich text natively,
		// we just need to make sure the typewriter
		// works properly with it
		waitThenDO(() =>
		{
			if (Line == null || paused)
			{
				return;
			}
			string line = Line.line;
			int index = Line.index;
			if (richRegex.IsMatch(line))
			{
				PlayVA(Line);
				CharacterManager.Instance.changeLightingFocus();
				dialogue.SayRich(currentNode.GetCurrentLine().line);
				return;
			}
			// handles speaker change. Also handles which character's expressions/animations are being controlled
			if (line.Contains("::"))
			{
				string newSpeaker = line.Split(new[] { "::" }, System.StringSplitOptions.None)[0];
				dialogue.currentSpeaker = newSpeaker;

				// update character sprite to current speaker sprite
				ReadNextLine();
				ParseLine(currentNode.GetCurrentLine());
				return;
			}


			// CHANGING EXPRESSIONS
			// expressions in the script will be written like this: [happy]
			// expressions must be on their own lines 
			if (emoteRegex.IsMatch(line))
			{
				// Debug.Log(line);
				char[] tagChars = { '[', ']', ' ' };
				string expression = line.Trim(tagChars);
				CharacterManager.Instance.changeExpression(dialogue.currentSpeaker, expression);
				ReadNextLine();
				ParseLine(currentNode.GetCurrentLine());
				return;
			}

			// COMMANDS
			// These will be any other type of commands 
			// that aren't rich text tags or emotion controls
			if (commandRegex.IsMatch(line))
			{
				char[] tagChars = { '{', '}', ' ' };
				string command = line.Trim(tagChars);


				if (command.Contains(":"))
				{
					string[] commandComponents = command.Split(':');
					command = commandComponents[0];
					CommandsManager.HandleWithArgs(command, commandComponents[1].Split(','));
				}
				else
				{
					CommandsManager.HandleWithArgs(command, null);
				}
				if (scriptChanged)
				{
					return;
				}
				ReadNextLine();
				ParseLine(currentNode.GetCurrentLine());
				return;
			}

			// if it's not a command simply display the text
			PlayVA(Line);
			CharacterManager.Instance.changeLightingFocus();
			dialogue.Say(line);
		});

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
