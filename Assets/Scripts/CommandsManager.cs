﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Globalization;

public class CommandsManager : Singleton<CommandsManager>
{
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
    public void handle(string command)
    {
        // Debug.Log("Non-argument command found!");
    }

    public void handleWithArgs(string command, ArrayList args)
    {
        switch (command)
        {
            case "additive":
                if (args[0].ToString().ToLower().Trim().Equals("on"))
                {
                    dialogue.Say("");
                    dialogue.additiveTextEnabled = true;
                    Debug.Log("Set additive text to on!");
                }
                else if (args[0].ToString().ToLower().Trim().Equals("off"))
                {
                    dialogue.additiveTextEnabled = false;
                    Debug.Log("Set additive text to off!");
                }
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
                characterManager.changeAnimation(dialogue.currentSpeaker, args[0].ToString().Trim());
                break;
            case "movechar":
                characterManager.moveCharacter(args[0].ToString().Trim(), args[1].ToString().Trim());
                break;
            case "changebg":
                TransitionManager.TransitionBG(args[0].ToString().Trim());
                break;
            case "changescene":
                TransitionManager.transitionScene(args[0].ToString().Trim());
                // TransitionManager.TransitionBG(args[0].ToString().Trim());
                break;
            case "changescript":
                ScriptParser.Instance.scriptChanged = true;
                if (args.Count == 2)
                {
                    ScriptParser.Instance.changeScriptFile(args[0].ToString().Trim(), int.Parse(args[1].ToString().Trim()));
                }
                else
                {
                    ScriptParser.Instance.changeScriptFile(args[0].ToString().Trim());
                }
                // Instance.StartCoroutine(waitForTransitionsThenChangeScript(args[0].ToString().Trim()));
                // TransitionManager.TransitionBG(args[0].ToString().Trim());
                break;
            case "emptyscene":
                CharacterManager.Instance.emptyScene();
                break;
            case "loopbg":
                TransitionManager.Instance.animatedBG.isLooping = args[0].ToString().ToLower().Trim().Equals("true");
                break;
            case "playbg":
                string bgName;
                if (args != null)
                { bgName = args[0].ToString().ToLower().Trim(); }
                else { bgName = ""; }
                TransitionManager.Instance.playBG(bgName);
                break;
            case "changename":
                CharacterManager.Instance.changeName(args[0].ToString().Trim(), args[1].ToString().Trim());
                break;
            case "fadeinsound":
                AudioManager.Instance.FadeIn(args[0].ToString().Trim(), float.Parse(args[1].ToString().Trim()));
                break;
            case "fadeoutsound":
                AudioManager.Instance.FadeOut(args[0].ToString().Trim(), float.Parse(args[1].ToString().Trim()));
                break;
            case "choice":
                ScriptParser.Instance.parseChoice(args[0].ToString().Trim());
                break;
        }


    }
}
