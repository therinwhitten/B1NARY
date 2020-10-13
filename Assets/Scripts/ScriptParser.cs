using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;


public class ScriptParser : MonoBehaviour
{
    string path = Application.streamingAssetsPath + "/Docs/CharacterPrefabTestScript.txt";

    DialogueSystem dialogue;

    EmotesSystem emotes;

    CommandsManager commands;

    StreamReader reader = null;

    string currentLine;

    // regex for grabbing rich text tags
    Regex richRegex = new Regex("<(.*?)>");

    // regex for grabbing emotes
    Regex emoteRegex = new Regex("\\[(.*?)\\]");

    // regex for commands
    Regex commandRegex = new Regex("\\{(.*?)\\}");




    // Start is called before the first frame update
    void Start()
    {
        // TextAsset textFile = Resources.Load<TextAsset>("Docs/CharacterPrefabTestScript");

        dialogue = DialogueSystem.instance;
        emotes = EmotesSystem.instance;
        commands = new CommandsManager();
        reader = new StreamReader(path);
    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!dialogue.isSpeaking || dialogue.isWaitingForUserInput)
            {
                // if end of file has been reached
                if (reader.Peek() == -1)
                {
                    return;
                }
                // else grab next line
                readNextLine();
                parseLine(currentLine);
                // Say(lines[index]);
                // index++;
            }
        }
    }

    void parseLine(string line)
    {
        // RICH TEXT
        // Unity already supports rich text natively,
        // we just need to make sure the typewriter
        // works properly with it
        if (richRegex.IsMatch(line))
        {
            dialogue.SayRich(currentLine);
            return;
        }
        // handles speaker change. Also handles which character's emotes are being controlled
        if (line.Contains("::"))
        {
            string newSpeaker = line.Split(new[] { "::" }, System.StringSplitOptions.None)[0];
            dialogue.speakerNameText.text = newSpeaker;

            // update character sprite to current speaker sprite
            readNextLine();
            parseLine(currentLine);
            return;
        }


        // CHANGING EMOTES
        // emotes in the script will be written like this: [happy]
        // emotes must be on their own lines 
        if (emoteRegex.IsMatch(line))
        {
            // Debug.Log(line);
            char[] tagChars = { '[', ']', ' ' };
            string emote = line.Trim(tagChars);
            // Debug.Log(emote);
            emotes.changeEmote(emote);
            readNextLine();
            parseLine(currentLine);
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
                ArrayList commandWords = new ArrayList(command.Split(':'));
                command = commandWords.Cast<string>().ElementAt(0);
                commandWords.RemoveAt(0);
                ArrayList args = commandWords;

                commands.handleWithArgs(command, args);
            }
            else
            {
                commands.handle(command);
            }
            readNextLine();
            parseLine(currentLine);
            return;
        }

        // if it's not a command simply display the text
        dialogue.Say(currentLine);
    }
    void readNextLine()
    {
        currentLine = reader.ReadLine();
    }
}
