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
    [HideInInspector]
    public bool scriptChanged = false;
    public string scriptName;
    public bool paused = false;
    bool choice = false;
    // public int lineIndex = 0;
    // public int continueIndex = 0;

    public Dictionary<string, AudioClip> voiceLines;

    string path { get { return Application.streamingAssetsPath + "/Docs/" + scriptName + ".txt"; } }

    public Dictionary<string, List<string>> currentChoiceOptions;
    DialogueSystem dialogue { get { return DialogueSystem.Instance; } }


    CommandsManager commands { get { return CommandsManager.Instance; } }

    StreamReader reader = null;

    // public string currentNode.getCurrentLine() { get { return currentNode.getCurrentLine(); } }

    // regex for grabbing rich text tags
    Regex richRegex = new Regex("<(.*?)>");

    // regex for grabbing expressions
    Regex emoteRegex = new Regex("\\[(.*?)\\]");

    // regex for commands
    Regex commandRegex = new Regex("\\{(.*?)\\}");
    public DialogueNode currentNode;



    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        // initialize();
    }

    public override void initialize()
    {
        // lineIndex = 0;
        reader = new StreamReader(path);
        currentNode = new DialogueNode(getLines());
        // readNextLine();
        grabVoiceLines();

        parseLine(currentNode.getCurrentLine());
    }
    public void grabVoiceLines()
    {
        voiceLines = null;
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Voice/" + scriptName);
        voiceLines = new Dictionary<string, AudioClip>();

        foreach (AudioClip clip in clips)
        {
            // Debug.Log(clip.name.Trim());
            voiceLines.Add(clip.name.Trim(), clip);
        }
    }
    public void changeScriptFile(string newScript, int position = 0)
    {
        scriptName = newScript;
        reader = new StreamReader(path);
        currentNode = new DialogueNode(getLines());
        grabVoiceLines();
        position -= 2;
        position = Mathf.Clamp(position, 0, int.MaxValue);
        currentNode.moveIndex(position);
        scriptChanged = false;

        parseLine(currentNode.getCurrentLine());
    }

    List<DialogueLine> getLines()
    {
        List<DialogueLine> lines = new List<DialogueLine>();
        int i = 1;
        while (!reader.EndOfStream)
        {
            DialogueLine line = new DialogueLine(reader.ReadLine(), i);
            lines.Add(line);
            i++;
        }
        reader.Close();
        return lines;
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
                // // if end of file has been reached
                // if (currentNode.endReached())
                // {
                //     if (currentNode.previous != null)
                //     {
                //         // if we reached the end of the node but there's a parent node,
                //         // continue from where we left off
                //         DialogueNode previousNode = currentNode.previous;
                //         currentNode = previousNode;
                //         currentNode.index--;
                //         parseLine(currentNode.getCurrentLine());
                //     }
                //     else
                //     {
                //         return;
                //     }
                // }
                // else grab next line
                readNextLine();
                parseLine(currentNode.getCurrentLine());
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
    public void playVA(DialogueLine Line)
    {
        GameObject charObject = null;
        string currentSpeaker = DialogueSystem.Instance.currentSpeaker;
        CharacterManager.Instance.charactersInScene.TryGetValue(currentSpeaker, out charObject);
        if (charObject != null)
        {
            try
            {
                CharacterScript charScript = charObject.GetComponent<CharacterScript>();
                charScript.speak(currentSpeaker, Line);
            }
            catch (System.IndexOutOfRangeException)
            {
                Debug.LogWarning("Character has no voice!");
            }
        }
    }
    public void parseLine(DialogueLine Line)
    {

        // RICH TEXT
        // Unity already supports rich text natively,
        // we just need to make sure the typewriter
        // works properly with it
        waitThenDO(() =>
        {
            if (Line == null || paused)
            {
                return;
            }
            string line = Line.line;
            int index = Line.index;
            if (richRegex.IsMatch(line))
            {
                playVA(Line);
                CharacterManager.Instance.changeLightingFocus();
                dialogue.SayRich(currentNode.getCurrentLine().line);
                return;
            }
            // handles speaker change. Also handles which character's expressions/animations are being controlled
            if (line.Contains("::"))
            {
                string newSpeaker = line.Split(new[] { "::" }, System.StringSplitOptions.None)[0];
                dialogue.currentSpeaker = newSpeaker;

                // update character sprite to current speaker sprite
                readNextLine();
                parseLine(currentNode.getCurrentLine());
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
                parseLine(currentNode.getCurrentLine());
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
                parseLine(currentNode.getCurrentLine());
                return;
            }

            // if it's not a command simply display the text
            playVA(Line);
            CharacterManager.Instance.changeLightingFocus();
            dialogue.Say(line);
        });

    }

    private void OnApplicationQuit()
    {
        reader.Close();
    }
    void readNextLine()
    {
        currentNode.nextLine();
    }
}
