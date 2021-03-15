using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;


public class DialogueNode
{
    public DialogueNode previous;
    List<string> lines;
    public int index;
    Regex commandRegex = new Regex("\\{(.*?)\\}");

    Regex emoteRegex = new Regex("\\[(.*?)\\]");

    public Dictionary<string, DialogueNode> choices;
    public DialogueNode(List<string> lines)
    {
        previous = null;
        this.choices = new Dictionary<string, DialogueNode>();
        this.lines = lines;
        index = 0;
    }
    public void readLine()
    {
        // ScriptParser.Instance.parseLine(lines[index]);
        index++;
    }

    public string getCurrentLine()
    {
        return lines[index];
    }
    public void nextLine()
    {
        index++;
    }
    public void moveIndex(int newIndex)
    {
        index = newIndex;
    }
    public bool endReached()
    {
        return (index >= (lines.Count - 1));
    }
    public void parseChoice(string choiceTitle)
    {
        ScriptParser.Instance.paused = true;
        choices = new Dictionary<string, DialogueNode>();
        DialogueSystem.Instance.Say(choiceTitle);
        List<string> block = getOptionalBlock(commandRegex, lines, index);
        int continueIndex = index + block.Count;
        Debug.Log(lines[continueIndex]);
        removeEnclosers(block, '{', '}');
        string choiceName = "";
        for (int i = 0; i < block.Count; i++)
        {
            if (!block[i].Contains("["))
            {
                if (choiceName == "")
                {
                    choiceName = block[i];
                }
                else
                {
                    choiceName += System.Environment.NewLine + block[i];
                }
                List<string> choiceBlock = getOptionalBlock(emoteRegex, block, i);
                i += choiceBlock.Count;
                removeEnclosers(choiceBlock, '[', ']');
                DialogueNode choice = new DialogueNode(choiceBlock);
                choice.previous = this;
                choices[choiceName] = choice;
                choiceName = "";
            }
        }
        index = continueIndex - 1;
        ChoiceController choiceController = GameObject.Find("Choice Panel").GetComponent<ChoiceController>();
        choiceController.newChoice();
    }
    public void selectChoice(string choiceName)
    {
        ScriptParser.Instance.currentNode = choices[choiceName];
        ScriptParser.Instance.paused = false;
        ScriptParser.Instance.parseLine(choices[choiceName].getCurrentLine());
    }
    public DialogueNode makeConditionalNode()
    {
        List<string> block = getOptionalBlock(commandRegex, lines, index);
        int continueIndex = index + block.Count + 1;
        Debug.Log(index);
        Debug.Log(continueIndex);
        index = continueIndex;
        removeEnclosers(block, '{', '}');
        DialogueNode conditional = new DialogueNode(block);
        conditional.previous = this;
        return conditional;
    }
    public List<string> getOptionalBlock(Regex regex, List<string> lines, int i)
    {
        i++;
        List<string> Blines = new List<string>();
        string Rlines = "";
        // grabs the optional block - enclosed with {}
        // we use Rlines to check for the {abc} pattern across multiple lines,
        // without including the script commands
        while (!regex.IsMatch(Rlines))
        {
            if (regex.IsMatch(lines[i]))
            {
                Blines.Add(lines[i]);
                i++;
                continue;
            }
            else
            {
                Blines.Add(lines[i]);
                Rlines += (" " + lines[i]);
                i++;
            }
        }
        return Blines;
    }
    // removes enclosing brackets from a given block of text.
    // removes resulting empty lines if there are any
    private void removeEnclosers(List<string> block, char encloser1, char encloser2)
    {
        // List<string> block = lineBlock;
        int enclosersFound = 0;
        // remove first encloser

        for (int i = 0; i < block.Count && enclosersFound == 0; i++)
        {
            if (block[i].Contains(encloser1.ToString()))
            {
                block[i] = block[i].Trim();
                block[i] = block[i].TrimStart(encloser1);
                enclosersFound++;
            }
        }
        // remove second encloser

        for (int i = block.Count - 1; i >= 0 && enclosersFound == 1; i--)
        {

            if (block[i].Contains(encloser2.ToString()))
            {
                block[i] = block[i].Trim();
                block[i] = block[i].TrimEnd(encloser2);
                enclosersFound++;
            }
        }

        // remove empty lines if there are any
        // also trim the block of empty spaces
        for (int i = 0; i < block.Count; i++)
        {
            block[i] = block[i].Trim();
            if (block[i].Equals(""))
            {
                block.RemoveAt(i);
            }
        }
    }
}