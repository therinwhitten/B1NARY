using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class ScriptParser : MonoBehaviour
{
    string path = "Assets/Resources/Docs/TestScript.txt";

    DialogueSystem dialogue;

    EmotesSystem emotes;

    StreamReader reader = null;

    string currentLine;

    private bool additiveText = false;

    // regex for grabbing rich text tags
    Regex richRegex = new Regex("<(.*?)>");

    // regex for grabbing emotes
    Regex emoteRegex = new Regex("\\[(.*?)\\]");

    // Start is called before the first frame update
    void Start()
    {
        dialogue = DialogueSystem.instance;
        emotes = EmotesSystem.instance;
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

        // if it's not a command simply display the text
        dialogue.Say(currentLine);
    }
    void readNextLine()
    {
        currentLine = reader.ReadLine();
    }
}
