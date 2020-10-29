using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

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
        Debug.Log("Commands manager initialized");
    }
    public void handle(string command)
    {
        Debug.Log("Non-argument command found!");
    }

    public void handleWithArgs(string command, ArrayList args)
    {
        switch (command)
        {
            case "additive":
                if (args[0].ToString().Trim().Equals("on"))
                {
                    dialogue.Say("");
                    dialogue.additiveTextEnabled = true;
                    Debug.Log("Set additive text to on!");
                }
                else if (args[0].ToString().Trim().Equals("off"))
                {
                    dialogue.additiveTextEnabled = false;
                    Debug.Log("Set additive text to off!");
                }
                break;
            case "spawnChar":
                if (args.Count == 1)
                {
                    characterManager.spawnCharacter(args[0].ToString().Trim());
                }
                else
                {
                    characterManager.spawnCharacter(args[0].ToString().Trim(), args[1].ToString().Trim());
                }
                break;
            case "anim":
                characterManager.changeAnimation(dialogue.currentSpeaker, args[0].ToString().Trim());
                break;
            case "moveChar":
                characterManager.moveCharacter(args[0].ToString().Trim(), args[1].ToString().Trim());
                break;
            case "changeBG":
                TransitionManager.TransitionBG(args[0].ToString().Trim());
                break;
            case "changeScene":
                TransitionManager.transitionScene(args[0].ToString().Trim());
                // TransitionManager.TransitionBG(args[0].ToString().Trim());
                break;
            case "changeScript":
                ScriptParser.Instance.changeScriptFile(args[0].ToString().Trim());
                // Instance.StartCoroutine(waitForTransitionsThenChangeScript(args[0].ToString().Trim()));
                // TransitionManager.TransitionBG(args[0].ToString().Trim());
                break;
            case "emptyScene":
                CharacterManager.Instance.emptyScene();
                break;
        }



        // IEnumerator waitForTransitionsThenDo(Action action)
        // {
        //     while (TransitionManager.transitioningBG != null || TransitionManager.transitioningScene != null)
        //     {
        //         yield return new WaitForEndOfFrame();
        //     }
        //     action();
        // }
    }
}
