using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using System;

public class ScriptParser : Singleton<ScriptParser>
{

    public bool scriptChanged = false;
    public string scriptName;
    public bool paused = false;
    bool choice = false;
    public int lineIndex = 0;
    public int continueIndex = 0;

    string path { get { return Application.streamingAssetsPath + "/Docs/" + scriptName + ".txt"; } }

    public Dictionary<string, List<string>> currentChoiceOptions;
    DialogueSystem dialogue { get { return DialogueSystem.Instance; } }


    CommandsManager commands { get { return CommandsManager.Instance; } }

    StreamReader reader = null;

    string currentLine;

    // regex for grabbing rich text tags
    Regex richRegex = new Regex("<(.*?)>");

    // regex for grabbing expressions
    Regex emoteRegex = new Regex("\\[(.*?)\\]");

    // regex for commands
    Regex commandRegex = new Regex("\\{(.*?)\\}");




    // Start is called before the first frame update
    void Start()
    {
        TextAsset textFile = Resources.Load<TextAsset>("Docs/CharacterPrefabTestScript");
        DontDestroyOnLoad(this.gameObject);
        // initialize();
    }

    public override void initialize()
    {
        lineIndex = 0;
        reader = new StreamReader(path);
        readNextLine();
        parseLine(currentLine);
    }
    public void changeScriptFile(string newScript, int position = 1)
    {
        scriptName = newScript;
        reader = new StreamReader(path);
        lineIndex = 0;
        while (lineIndex != position)
        {
            readNextLine();
        }
        parseLine(currentLine);
        scriptChanged = false;
    }


    // Update is called once per frame
    void Update()
    {
        if (!TransitionManager.Instance.commandsAllowed)
        {
            return;
        }

        if ((Input.GetKeyDown(KeyCode.Space)
        || Input.GetKeyDown(KeyCode.Mouse0)
        || Input.GetKey(KeyCode.LeftControl))
         && !paused)
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
            }
            else
            // if the dialogue is still being written out just skip to the end of the line
            {
                dialogue.StopSpeaking();
                dialogue.speechText.text = dialogue.targetSpeech;
            }
        }
    }
    private void waitThenDO(System.Action action)
    {
        Instance.StartCoroutine(waitForTransitionsThenDo(action));
    }
    IEnumerator waitForTransitionsThenDo(System.Action action)
    {
        while (!TransitionManager.Instance.commandsAllowed)
        {
            yield return new WaitForEndOfFrame();
        }
        action();
    }
    void playVA()
    {
        GameObject charObject = null;
        CharacterManager.Instance.charactersInScene.TryGetValue(DialogueSystem.Instance.currentSpeaker, out charObject);
        if (charObject != null)
        {
            try
            {
                CharacterScript charScript = charObject.GetComponent<CharacterScript>();
                charScript.speak();
            }
            catch (System.IndexOutOfRangeException)
            {
                Debug.LogWarning("Character has no voice!");
            }

        }
    }
    void parseLine(string line)
    {
        // RICH TEXT
        // Unity already supports rich text natively,
        // we just need to make sure the typewriter
        // works properly with it
        waitThenDO(() =>
        {
            if (line == null || paused)
            {
                return;
            }
            if (richRegex.IsMatch(line))
            {
                playVA();
                CharacterManager.Instance.changeLightingFocus();
                dialogue.SayRich(currentLine);
                return;
            }
            // handles speaker change. Also handles which character's expressions/animations are being controlled
            if (line.Contains("::"))
            {
                string newSpeaker = line.Split(new[] { "::" }, System.StringSplitOptions.None)[0];
                dialogue.currentSpeaker = newSpeaker;

                // update character sprite to current speaker sprite
                readNextLine();
                parseLine(currentLine);
                return;
            }


            // CHANGING EXPRESSIONS
            // expressions in the script will be written like this: [happy]
            // expressions must be on their own lines 
            if (emoteRegex.IsMatch(line))
            {
                // Debug.Log(line);
                char[] tagChars = { '[', ']', ' ' };
                string expression = line.Trim(tagChars);
                CharacterManager.Instance.changeExpression(dialogue.currentSpeaker, expression);
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
                    ArrayList args = new ArrayList(commandWords[0].ToString().Split(','));

                    commands.handleWithArgs(command.ToLower(), args);
                }
                else
                {
                    commands.handleWithArgs(command.ToLower(), null);
                }
                if (scriptChanged)
                {
                    return;
                }
                readNextLine();
                parseLine(currentLine);
                return;
            }

            // if it's not a command simply display the text
            playVA();
            CharacterManager.Instance.changeLightingFocus();
            dialogue.Say(currentLine);
        });

    }
    public void parseChoice(string choiceTitle = "")
    {
        paused = true;
        choice = true;
        DialogueSystem.Instance.Say(choiceTitle);
        StreamReader newReader = new StreamReader(path);
        currentChoiceOptions = new Dictionary<string, List<string>>();
        List<string> choiceStrings = new List<string>();
        continueIndex = 0;
        string line = "";
        while (continueIndex < lineIndex)
        {
            line = newReader.ReadLine();
            continueIndex++;
        }
        string choiceName = "";
        newReader.ReadLine();

        while (!line.Trim().Equals("}"))
        {
            line = newReader.ReadLine();
            choiceStrings.Add(line);
            continueIndex++;
        }
        choiceStrings.RemoveAt(choiceStrings.Count - 1);
        continueIndex += 2;
        for (int i = 0; i < choiceStrings.Count; i++)
        {
            string item = choiceStrings[i].Trim();
            if (item.Equals("["))
            {
                int j = i + 1;
                List<string> choiceLines = new List<string>();
                while (!choiceStrings[j].Trim().Equals("]"))
                {
                    choiceLines.Add(choiceStrings[j].Trim());
                    j++;
                }
                // Debug.Log("choice lines found: " + choiceLines.Count);
                currentChoiceOptions.Add(choiceName, choiceLines);
                choiceName = "";
                i = j;
                continue;
            }
            choiceName.Trim();
            if (choiceName == "")
            {
                choiceName = item.Trim();
            }
            else
            {
                choiceName += System.Environment.NewLine + item.Trim();
            }
            // Debug.Log("choice name found: " + choiceName);
        }
        ChoiceController choiceController = GameObject.Find("Choice Panel").GetComponent<ChoiceController>();
        choiceController.newChoice();
    }

    public void selectChoice(string choiceName)
    {
        Debug.Log("Selected option: " + choiceName);
        MemoryStream temp = new MemoryStream();
        List<string> mylist = currentChoiceOptions[choiceName];
        string continuestr = "{changescript: " + scriptName + ", " + continueIndex + "}";
        // Debug.Log(continuestr);
        mylist.Add(continuestr);
        string text = string.Join("\n", mylist.ToArray());
        byte[] buffer = Encoding.UTF8.GetBytes(text);
        temp.Write(buffer, 0, buffer.Length);
        paused = false;
        temp.Position = 0;
        reader = new StreamReader(temp);
        readNextLine();
        parseLine(currentLine);
    }
    public List<string> getOptionalBlock()
    {
        paused = true;
        StreamReader newReader = new StreamReader(path);
        List<string> choiceStrings = new List<string>();
        continueIndex = 0;
        string line = "";
        while (continueIndex < lineIndex)
        {
            line = newReader.ReadLine();
            continueIndex++;
        }
        // newReader.ReadLine();

        while (!line.Trim().Equals("}"))
        {
            line = newReader.ReadLine();
            choiceStrings.Add(line);
            continueIndex++;
        }
        choiceStrings.RemoveAt(choiceStrings.Count - 1);
        // continueIndex++;
        return choiceStrings;
    }

    public void playOptionalBlock(List<string> block)
    {
        string continuestr = "{changescript: " + scriptName + ", " + continueIndex + "}";
        // Debug.Log(continuestr);
        MemoryStream temp = new MemoryStream();
        block.Add(continuestr);
        string text = string.Join("\n", block.ToArray());
        byte[] buffer = Encoding.UTF8.GetBytes(text);
        temp.Write(buffer, 0, buffer.Length);
        paused = false;
        temp.Position = 0;
        reader = new StreamReader(temp);
        readNextLine();
        parseLine(currentLine);
    }


    private void OnApplicationQuit()
    {
        reader.Close();
    }
    void readNextLine()
    {
        currentLine = reader.ReadLine();
        lineIndex++;
    }
}
