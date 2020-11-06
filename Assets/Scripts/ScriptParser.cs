using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;


public class ScriptParser : Singleton<ScriptParser>
{
    public bool scriptChanged = false;
    public string scriptName;
    string path { get { return Application.streamingAssetsPath + "/Docs/" + scriptName + ".txt"; } }

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
        // TextAsset textFile = Resources.Load<TextAsset>("Docs/CharacterPrefabTestScript");
        DontDestroyOnLoad(this.gameObject);
        // initialize();
    }

    public override void initialize()
    {
        reader = new StreamReader(path);
        readNextLine();
        parseLine(currentLine);
    }
    public void changeScriptFile(string newScript)
    {
        scriptName = newScript;
        reader = new StreamReader(path);

        readNextLine();
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
            if (line == null)
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

                    commands.handleWithArgs(command, args);
                }
                else
                {
                    commands.handle(command);
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
    private void OnApplicationQuit()
    {
        reader.Close();
    }
    void readNextLine()
    {
        currentLine = reader.ReadLine();
    }
}
