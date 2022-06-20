using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Globalization;

public class CommandsManager : Singleton<CommandsManager>
{
	DialogueSystem dialogue => DialogueSystem.Instance;

	CharacterManager characterManager => CharacterManager.Instance;

	void Start()
	{
		initialize();
	}
	public override void initialize()
	{
		// Debug.Log("Commands manager initialized");
	}

	public void HandleWithArgs(string command, ArrayList args)
	{
		switch (command)
		{
			case "additive":
				if (args[0].ToString().ToLower().Trim().Equals("on"))
				{
					dialogue.Say("");
					dialogue.additiveTextEnabled = true;
					Debug.Log("Set additive text to on!");

					// ScriptParser.Instance.currentNode.nextLine();
					// ScriptParser.Instance.parseLine(ScriptParser.Instance.currentNode.getCurrentLine());
				}
				else if (args[0].ToString().ToLower().Trim().Equals("off"))
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
				if (args.Count == 2)
				{
					characterManager.spawnCharacter(args[0].ToString().Trim(), args[1].ToString().Trim(), "");
				}
				else
				{
					characterManager.spawnCharacter(args[0].ToString().Trim(), args[1].ToString().Trim(), args[2].ToString().Trim());
				}
				break;
			case "anim":
				characterManager.changeAnimation(args[0].ToString().Trim(), args[1].ToString().Trim());
				break;
			case "movechar":
				characterManager.moveCharacter(args[0].ToString().Trim(), args[1].ToString().Trim());
				break;
			case "changebg":
				TransitionManager.TransitionBG(args[0].ToString().Trim());
				break;
			case "changescene":
				TransitionManager.transitionScene(args[0].ToString().Trim());
				break;
			case "changescript":
				ScriptParser.Instance.scriptChanged = true;
				if (args.Count == 2)
				{
					ScriptParser.Instance.ChangeScriptFile(args[0].ToString().Trim(), int.Parse(args[1].ToString().Trim()));
				}
				else
				{
					ScriptParser.Instance.ChangeScriptFile(args[0].ToString().Trim());
				}
				break;
			case "emptyscene":
				CharacterManager.Instance.emptyScene();
				break;
			case "loopbg":
				TransitionManager.Instance.animatedBG.isLooping = args[0].ToString().Trim().ToLower().Equals("true");
				break;
			case "playbg":
				string bgName;
				if (args != null)
					bgName = args[0].ToString().Trim().ToLower();
				else
					bgName = "";
				TransitionManager.Instance.playBG(bgName);
				break;
			case "changename":
				CharacterManager.Instance.changeName(args[0].ToString().Trim(), args[1].ToString().Trim());
				break;
			// Sounds
			case "fadeinsound":
				if (!Extensions.TryInvoke<SoundNotFoundException>(() => 
					AudioHandler.Instance.PlayFadedSound(args[0].ToString().Trim(), float.Parse(args[1].ToString().Trim())), out _))
					Debug.LogWarning($"{args[0]} is not a valid soundfile Path!");
				break;
			case "fadeoutsound":
				try
				{
					AudioHandler.Instance.StopSoundViaFade(args[0].ToString().Trim(), float.Parse(args[1].ToString().Trim()));
				}
				catch (SoundNotFoundException ex)
					{ Debug.LogWarning($"{args[0].ToString().Trim()} is not a valid soundfile Path!"
						+ ex); }
				catch (KeyNotFoundException ex)
					{ Debug.LogWarning($"Cannot find sound: {args[0].ToString().Trim()}\n"
						+ ex); }
				break;
			case "playsound":
				if (Extensions.TryInvoke<SoundNotFoundException>(() =>
					AudioHandler.Instance.PlaySound(args[0].ToString().Trim()), out _))
					Debug.LogWarning($"{args[0]} is not a valid soundfile Path!");
				break;
			case "stopsound":
				if (Extensions.TryInvoke<SoundNotFoundException>(() =>
					AudioHandler.Instance.StopSound(args[0].ToString().Trim()), out _))
					Debug.LogWarning($"{args[0]} is not a valid soundfile Path!");
				break;
			case "startdelayedsound":
				{
					if (Extensions.TryInvoke<Coroutine, SoundNotFoundException>(() =>
					StartCoroutine(Delay(float.Parse(args[0].ToString().Trim()), () => AudioHandler.Instance.PlaySound(args[0].ToString().Trim()))), out _, out _))
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
				ScriptParser.Instance.currentNode.parseChoice(args[0].ToString().Trim());
				break;
			case "setbool":
				PersistentData.Instance.state.bools[args[0].ToString().Trim()] = bool.Parse(args[1].ToString().Trim());
				break;
			case "if":
				ScriptParser.Instance.paused = true;

				DialogueNode conditional = ScriptParser.Instance.currentNode.makeConditionalNode();
				bool var = PersistentData.Instance.state.bools[args[0].ToString().Trim()];
				bool val = bool.Parse(args[1].ToString().Trim());
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
