public class DialogueLine
{
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