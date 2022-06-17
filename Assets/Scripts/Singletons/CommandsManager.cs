using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Globalization;

public class CommandsManager : Singleton<CommandsManager>
{
	[SerializeField] private AudioHandler audioHandler;


	DialogueSystem dialogue { get { return DialogueSystem.Instance; } }

	CharacterManager characterManager { get { return CharacterManager.Instance; } }

	void Start()
	{
		initialize();
	}
	public override void initialize()
	{
		// Debug.Log("Commands manager initialized");
	}

	public void HandleWithArgs(string command, ArrayList argsUnfiltered)
	{
		string[] args = new string[argsUnfiltered.Count];
		for (int i = 0; i < args.Length; i++)
			args[i] = argsUnfiltered[i].ToString().Trim();
		switch (command)
		{
			case "additive":
				if (args[0].ToLower().Equals("on"))
				{
					dialogue.Say("");
					dialogue.additiveTextEnabled = true;
					Debug.Log("Set additive text to on!");

					// ScriptParser.Instance.currentNode.nextLine();
					// ScriptParser.Instance.parseLine(ScriptParser.Instance.currentNode.getCurrentLine());
				}
				else if (args[0].ToLower().Equals("off"))
				{
					dialogue.additiveTextEnabled = false;
					Debug.Log("Set additive text to off!");
					// ScriptParser.Instance.currentNode.nextLine();
					// ScriptParser.Instance.parseLine(ScriptParser.Instance.currentNode.getCurrentLine());
				}
				else
					throw new ArgumentException($"{args[0]} is not a valid " +
						"argument for additive!");
				break;
			case "spawnchar":
				if (argsUnfiltered.Count == 2)
				{
					characterManager.spawnCharacter(args[0], args[1], "");
				}
				else
				{
					characterManager.spawnCharacter(args[0], args[1], args[2]);
				}
				break;
			case "anim":
				characterManager.changeAnimation(args[0], args[1]);
				break;
			case "movechar":
				characterManager.moveCharacter(args[0], args[1]);
				break;
			case "changebg":
				TransitionManager.TransitionBG(args[0]);
				break;
			case "changescene":
				TransitionManager.transitionScene(args[0]);
				break;
			case "changescript":
				ScriptParser.Instance.scriptChanged = true;
				if (argsUnfiltered.Count == 2)
				{
					ScriptParser.Instance.ChangeScriptFile(args[0], int.Parse(args[1]));
				}
				else
				{
					ScriptParser.Instance.ChangeScriptFile(args[0]);
				}
				break;
			case "emptyscene":
				CharacterManager.Instance.emptyScene();
				break;
			case "loopbg":
				TransitionManager.Instance.animatedBG.isLooping = args[0].ToLower().Equals("true");
				break;
			case "playbg":
				string bgName;
				if (argsUnfiltered != null)
					bgName = args[0].ToLower();
				else
					bgName = "";
				TransitionManager.Instance.playBG(bgName);
				break;
			case "changename":
				CharacterManager.Instance.changeName(args[0], args[1]);
				break;
			// Sounds
			case "fadeinsound":
				if (!Extensions.TryInvoke<SoundNotFoundException>(() => 
					AudioHandler.Instance.PlayFadedSound(args[0], float.Parse(args[1])), out _))
					Debug.LogWarning($"{args[0]} is not a valid soundfile Path!");
				break;
			case "fadeoutsound":
				try
				{
					AudioHandler.Instance.StopSoundViaFade(args[0], float.Parse(args[1]));
				}
				catch (SoundNotFoundException ex)
					{ Debug.LogWarning($"{args[0]} is not a valid soundfile Path!"
						+ ex); }
				catch (KeyNotFoundException ex)
					{ Debug.LogWarning($"Cannot find sound: {args[0]}\n"
						+ ex); }
				break;
			case "playsound":
				if (Extensions.TryInvoke<SoundNotFoundException>(() =>
					AudioHandler.Instance.PlaySound(args[0]), out _))
					Debug.LogWarning($"{args[0]} is not a valid soundfile Path!");
				break;
			case "stopsound":
				if (Extensions.TryInvoke<SoundNotFoundException>(() =>
					AudioHandler.Instance.StopSound(args[0]), out _))
					Debug.LogWarning($"{args[0]} is not a valid soundfile Path!");
				break;
			case "startdelayedsound":
				{
					if (Extensions.TryInvoke<Coroutine, SoundNotFoundException>(() =>
					StartCoroutine(Delay(float.Parse(args[0]), () => AudioHandler.Instance.PlaySound(args[0]))), out _, out _))
						Debug.LogWarning($"{args[0]} is not a valid soundfile Path!");
					IEnumerator Delay(float seconds, Action playAfterDelay)
					{
						yield return new WaitForSeconds(seconds);
						playAfterDelay.Invoke();
					}
				}
				break;
			case "choice":
				ScriptParser.Instance.paused = true;
				ScriptParser.Instance.currentNode.parseChoice(args[0]);
				break;
			case "setbool":
				PersistentData.Instance.state.bools[args[0]] = bool.Parse(args[1]);
				break;
			case "if":
				ScriptParser.Instance.paused = true;

				DialogueNode conditional = ScriptParser.Instance.currentNode.makeConditionalNode();
				bool var = PersistentData.Instance.state.bools[args[0]];
				bool val = bool.Parse(args[1]);
				if (var == val)
				{
					// Debug.Log("Condition met. Playing optional block...");
					ScriptParser.Instance.currentNode = conditional;
					ScriptParser.Instance.paused = false;
				}
				else
				{
					// Debug.Log("Condition not met. Skipping...");
					ScriptParser.Instance.currentNode.index--;
					ScriptParser.Instance.paused = false;
				}
				break;
		}
	}
}
