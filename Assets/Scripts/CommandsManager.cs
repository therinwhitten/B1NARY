using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandsManager
{
    DialogueSystem dialogue;
    EmotesSystem emotes;

    public CommandsManager(DialogueSystem dialogue, EmotesSystem emotes)
    {
        this.dialogue = dialogue;
        this.emotes = emotes;

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
        }
    }
}
