using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DialogueNode
{
    public DialogueNode previous;
    List<string> lines;
    int index;

    public DialogueNode(List<string> lines)
    {
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
        return (index == (lines.Count - 1));
    }
}