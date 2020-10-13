﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandsManager
{
    DialogueSystem dialogue;
    EmotesSystem emotes;

    CharacterManager characterManager;

    public CommandsManager()
    {
        this.dialogue = DialogueSystem.instance;
        this.emotes = EmotesSystem.instance;
        this.characterManager = CharacterManager.instance;
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
                if (args[0].Equals("on"))
                {
                    dialogue.Say("");
                    dialogue.additiveTextEnabled = true;
                    Debug.Log("Set additive text to on!");
                }
                else if (args[0].Equals("off"))
                {
                    dialogue.additiveTextEnabled = false;
                    Debug.Log("Set additive text to off!");
                }
                break;
            case "spawnChar":
                characterManager.spawnCharacter(args[0].ToString());
                break;
            case "anim":
                characterManager.changeAnimation(dialogue.currentSpeaker, args[0].ToString());
                break;
        }
    }
}
