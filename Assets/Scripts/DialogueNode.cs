using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;


public class DialogueNode
{
    public DialogueNode previous;
    public List<string> lines;
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
        index++;
    }

    public string getCurrentLine()
    {
        try
        {
            return lines[index];
        }
        catch (System.ArgumentOutOfRangeException)
        {
            Debug.Log("End reached. Returning to parent node...");
            if (previous != null)
            {
                ScriptParser.Instance.currentNode = previous;
                return previous.getCurrentLine();
            }
            else
            {
                return null;
            }
        }
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
        List<string> block = getOptionalBlock(lines, '{', '}', index);

        foreach (string line in block)
        {
            lines.RemoveAt(index);
        }
        lines.RemoveAt(index);
        removeEnclosers(block, '{', '}');
        string choiceName = "";
        for (int i = 0; i < block.Count; i++)
        {
            if (!block[i].Contains("["))
            {
                if (choiceName == "")
                {
                    choiceName = block[i].Trim();
                }
                else
                {
                    choiceName += System.Environment.NewLine + block[i].Trim();
                }
            }
            else
            {
                List<string> choiceBlock = getOptionalBlock(block, '[', ']', i - 1);
                i += choiceBlock.Count - 1;
                removeEnclosers(choiceBlock, '[', ']');
                DialogueNode choice = new DialogueNode(choiceBlock);
                choice.previous = this;
                choices[choiceName] = choice;
                choiceName = "";
            }
        }
        index--;
        ChoiceController choiceController = GameObject.Find("Choice Panel").GetComponent<ChoiceController>();
        choiceController.newChoice();
    }
    public void selectChoice(DialogueNode choice)
    {
        ScriptParser.Instance.currentNode = choice;
        ScriptParser.Instance.paused = false;
        ScriptParser.Instance.parseLine(choice.getCurrentLine());
    }
    public DialogueNode makeConditionalNode()
    {
        List<string> block = getOptionalBlock(lines, '{', '}', index);

        foreach (string line in block)
        {
            lines.RemoveAt(index);
        }
        lines.RemoveAt(index);
        int id = 1;
        foreach (string line in lines)
        {
            Debug.Log(id + ": " + line);
            id++;
        }
        removeEnclosers(block, '{', '}');

        // 5HEAD MOVE
        block.Insert(0, "");

        DialogueNode conditional = new DialogueNode(block);
        conditional.previous = this;
        return conditional;
    }
    public List<string> getOptionalBlock(List<string> lines, char b, char e, int id)
    {
        int depth = 0;
        bool found = false;
        List<string> Blines = new List<string>();
        // Regex commandRegex = new Regex("\\{(.*?)\\}");
        Regex regex = new Regex("\\" + b + "(.*?)\\" + e);
        // grabs the optional block - enclosed with {}
        // we use Rlines to check for the {abc} pattern across multiple lines,
        // without including the script commands
        for (int i = id + 1; i < lines.Count; i++)
        {
            if (regex.IsMatch(lines[i]))
            {
                Blines.Add(lines[i]);
                continue;
            }
            if (lines[i].Contains(b.ToString()))
            {
                found = true;
                depth++;
            }
            else if (lines[i].Contains(e.ToString()))
            {
                depth--;
            }
            Blines.Add(lines[i]);
            if (depth == 0 && found)
            {
                break;
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
                i--;
            }
        }
    }
}