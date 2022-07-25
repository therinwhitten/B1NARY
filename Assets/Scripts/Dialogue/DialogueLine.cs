public class DialogueLine
{
	public static implicit operator ScriptLine(DialogueLine line)
		=> new ScriptLine(line.line, () => line.scriptName, line.index);

	public string line;
	public string scriptName;
	public int index;
	public DialogueLine(string line, int index, string scriptName)
	{
		this.line = line;
		this.index = index;
		this.scriptName = scriptName;
	}

}