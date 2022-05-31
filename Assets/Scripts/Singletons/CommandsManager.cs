﻿using System.Collections;
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
			args[i] = argsUnfiltered[i].ToString().Trim().ToLower();
		switch (command)
		{
			case "additive":
				if (args[0].Equals("on"))
				{
					dialogue.Say("");
					dialogue.additiveTextEnabled = true;
					Debug.Log("Set additive text to on!");

					// ScriptParser.Instance.currentNode.nextLine();
					// ScriptParser.Instance.parseLine(ScriptParser.Instance.currentNode.getCurrentLine());
				}
				else if (args[0].Equals("off"))
				{
					dialogue.additiveTextEnabled = false;
					Debug.Log("Set additive text to off!");
					// ScriptParser.Instance.currentNode.nextLine();
					// ScriptParser.Instance.parseLine(ScriptParser.Instance.currentNode.getCurrentLine());
				}
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
				TransitionManager.Instance.animatedBG.isLooping = args[0].Equals("true");
				break;
			case "playbg":
				string bgName;
				if (argsUnfiltered != null)
					bgName = args[0];
				else
					bgName = "";
				TransitionManager.Instance.playBG(bgName);
				break;
			case "changename":
				CharacterManager.Instance.changeName(args[0], args[1]);
				break;
			case "fadeinsound":
				AudioManager.Instance.FadeIn(args[0], float.Parse(args[1]));
				break;
			case "fadeoutsound":
				AudioManager.Instance.FadeOut(args[0], float.Parse(args[1]));
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
