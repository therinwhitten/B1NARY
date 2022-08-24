namespace B1NARY.Scripting
{
	using System.Linq;
	using B1NARY.UI;
	using System.Collections.Generic;
	using UnityEngine;
	using System.Text.RegularExpressions;
	using B1NARY.Scripting.Experimental;

	public class DialogueNode
	{
		public DialogueNode previous;
		public List<DialogueLine> lines;
		public int index;
		Regex commandRegex = new Regex("\\{(.*?)\\}");

		Regex emoteRegex = new Regex("\\[(.*?)\\]");

		public Dictionary<string, DialogueNode> choices;
		public DialogueNode(IEnumerable<DialogueLine> lines)
		{
			previous = null;
			this.choices = new Dictionary<string, DialogueNode>();
			this.lines = lines.ToList();
			index = 0;
		}

		[SerializeField]
		private ChoicePanel choicePanel;
		private ChoicePanel ChoicePanel
		{
			get
			{
				if (choicePanel == null)
					choicePanel = Object.FindObjectOfType<ChoicePanel>();
				return choicePanel;
			}
		}

		public DialogueLine GetCurrentLine()
		{

			if (index > lines.Count - 1)
				if (previous != null)
				{
					ScriptParser.Instance.currentNode = previous;
					return previous.GetCurrentLine();
				}
				else
					return null;
			DialogueLine output = lines[index];
			if (output.line.Contains("//"))
				output.line = output.line.Remove(output.line.IndexOf("//")); // Comments haha
			return output;
		}

		public DialogueLine nextLine()
		{
			index++;
			return GetCurrentLine();
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
			ScriptParser.Instance.PlayVA(lines[index]);
			DialogueSystem.Instance.Say(choiceTitle);
			List<DialogueLine> block = getOptionalBlock(lines, '{', '}', index);

			// A for loop might work better..
			foreach (DialogueLine line in block)
			{
				lines.RemoveAt(index);
			}
			lines.RemoveAt(index);
			removeEnclosers(block, '{', '}');
			string choiceName = "";
			for (int i = 0; i < block.Count; i++)
			{
				if (!block[i].line.Contains("["))
				{
					if (choiceName == "")
					{
						choiceName = block[i].line.Trim();
					}
					else
					{
						choiceName += System.Environment.NewLine + block[i].line.Trim();
					}
				}
				else
				{
					List<DialogueLine> choiceBlock = getOptionalBlock(block, '[', ']', i - 1);
					i += choiceBlock.Count - 1;
					removeEnclosers(choiceBlock, '[', ']');
					DialogueNode choice = new DialogueNode(choiceBlock);
					choice.previous = this;
					choices[choiceName] = choice;
					choiceName = "";
				}
			}
			index--; ;
			ChoicePanel.Initialize(choices.Keys.ToArray());
			ChoicePanel.PickedChoice += line => ChoicePanel.Dispose();
		}
		public void selectChoice(DialogueNode choice)
		{
			ScriptParser.Instance.currentNode = choice;
			ScriptParser.Instance.paused = false;
			ScriptParser.Instance.PlayLine(new ScriptLine(choice.GetCurrentLine())).FreeBlockPath();
		}
		public DialogueNode makeConditionalNode()
		{
			List<DialogueLine> block = getOptionalBlock(lines, '{', '}', index);

			foreach (DialogueLine line in block)
			{
				lines.RemoveAt(index);
			}
			lines.RemoveAt(index);
			removeEnclosers(block, '{', '}');

			// 5HEAD MOVE
			block.Insert(0, new DialogueLine("", -1, lines.First().scriptName));

			DialogueNode conditional = new DialogueNode(block);
			conditional.previous = this;
			return conditional;
		}
		public List<DialogueLine> getOptionalBlock(List<DialogueLine> lines, char b, char e, int id)
		{

			int depth = 0;
			bool found = false;
			List<DialogueLine> Blines = new List<DialogueLine>();
			// Regex commandRegex = new Regex("\\{(.*?)\\}");
			Regex regex = new Regex("\\" + b + "(.*?)\\" + e);
			// grabs the optional block - enclosed with {}
			// we use Rlines to check for the {abc} pattern across multiple lines,
			// without including the script commands
			for (int i = id + 1; i < lines.Count; i++)
			{
				if (regex.IsMatch(lines[i].line))
				{
					Blines.Add(lines[i]);
					continue;
				}
				if (lines[i].line.Contains(b.ToString()))
				{
					found = true;
					depth++;
				}
				else if (lines[i].line.Contains(e.ToString()))
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
		private void removeEnclosers(List<DialogueLine> block, char encloser1, char encloser2)
		{
			// List<string> block = lineBlock;
			int enclosersFound = 0;
			// remove first encloser

			for (int i = 0; i < block.Count && enclosersFound == 0; i++)
			{
				if (block[i].line.Contains(encloser1.ToString()))
				{
					block[i].line = block[i].line.Trim();
					block[i].line = block[i].line.TrimStart(encloser1);
					enclosersFound++;
				}
			}
			// remove second encloser

			for (int i = block.Count - 1; i >= 0 && enclosersFound == 1; i--)
			{

				if (block[i].line.Contains(encloser2.ToString()))
				{
					block[i].line = block[i].line.Trim();
					block[i].line = block[i].line.TrimEnd(encloser2);
					enclosersFound++;
				}
			}

			// remove empty lines if there are any
			// also trim the block of empty spaces
			for (int i = 0; i < block.Count; i++)
			{
				block[i].line = block[i].line.Trim();
				if (block[i].line.Equals(""))
				{
					block.RemoveAt(i);
					i--;
				}
			}
		}
	}
}