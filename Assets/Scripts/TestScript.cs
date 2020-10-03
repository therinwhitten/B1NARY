using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class TestScript : MonoBehaviour
{

    DialogueSystem dialogue;
    public TextAsset script = null;
    string path = "Assets/Resources/Docs/Script.txt";

    StreamReader reader = null;

    private List<string> lines = null;
    // Start is called before the first frame update
    void Start()
    {
        dialogue = DialogueSystem.instance;

        //Read the text from directly from the test.txt file
        reader = new StreamReader(path);
        lines = new List<string>();
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            lines.Add(line);
        }
    }



    string readLine()
    {
        return reader.ReadLine();
    }
    int index = 0;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!dialogue.isSpeaking || dialogue.isWaitingForUserInput)
            {
                if (index > lines.Count - 1)
                {
                    return;
                }
                Say(lines[index]);
                index++;
            }
        }
    }

    void Say(string s)
    {
        string[] parts = s.Split(new[] { "::" }, System.StringSplitOptions.None);
        string speech = (parts.Length >= 2) ? parts[1] : parts[0];
        string speaker = (parts.Length >= 2) ? parts[0] : "";

        dialogue.SayAdd(speech, speaker);
    }

}
