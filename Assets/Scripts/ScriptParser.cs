using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
public class ScriptParser : MonoBehaviour
{
    string path = "Assets/Resources/Docs/ParserExampleScript.txt";

    DialogueSystem dialogue;

    EmotesSystem emotes;

    StreamReader reader = null;

    string currentLine;

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
                if (reader.Peek() == -1)
                {
                    return;
                }
                readNextLine();
                parseLine(currentLine);
                // Say(lines[index]);
                // index++;
            }
        }
    }

    void parseLine(string line)
    {
        if (line.Contains("::"))
        {
            string newSpeaker = line.Split(new[] { "::" }, System.StringSplitOptions.None)[0];
            dialogue.speakerNameText.text = newSpeaker;
            // update character sprite to current speaker sprite
            readNextLine();
            parseLine(currentLine);
            return;
        }

        Regex regex = new Regex("<(.*?)>");

        if (regex.IsMatch(line))
        {
            // Debug.Log(line);
            char[] tagChars = { '<', '>', ' ' };
            string emote = line.Trim(tagChars);
            // Debug.Log(emote);
            emotes.changeEmote(emote);
            readNextLine();
            parseLine(currentLine);
            return;
        }


        dialogue.Say(currentLine);
    }
    void readNextLine()
    {
        currentLine = reader.ReadLine();
    }
}
