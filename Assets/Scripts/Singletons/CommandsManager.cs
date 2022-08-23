namespace B1NARY
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;
	using System.Text;
	using System.Threading.Tasks;
	using B1NARY.Audio;
	using B1NARY.UI;
	using B1NARY.DataPersistence;
	using B1NARY.Scripting;
	using B1NARY.Scripting.Experimental;

	public static class CommandsManager
	{
		private static DialogueSystem Dialogue => DialogueSystem.Instance;
		private static CharacterManager CharacterManager => CharacterManager.Instance;

		public static readonly HashSet<string> enabledHashset = new HashSet<string>()
		{ "on", "true", "enable" };
		public static readonly HashSet<string> disabledHashset = new HashSet<string>()
		{ "off", "false", "disable" };

		public static void HandleWithArgs(ScriptLine line)
		{
			var (command, args) = ScriptLine.CastCommand(line);
			HandleWithArgs(command, args, 
				AudioHandler.AudioDelegateCommands, 
				SceneManager.SceneDelegateCommands,
				DialogueSystem.DialogueDelegateCommands);
		}
		public static void HandleWithArgs(string command, string[] args, params IReadOnlyDictionary<string, Delegate>[] categorizedCommands)
		{
			command = command.ToLower();
			for (int i = 0; i < categorizedCommands.Length; i++)
				if (categorizedCommands[i].ContainsKey(command))
				{
					categorizedCommands[i][command].DynamicInvoke(args);
					Debug.Log("Custom command");
					return;
				}
			Debug.Log("Alt");
			switch (command)
			{
				case "spawncharacter":
				case "spawnchar":
					{
						if (args.Length > 3 || args.Length < 2)
							throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						if (args.Length == 2)
							CharacterManager.SummonCharacter(args[0], args[1]);
						else
							CharacterManager.SummonCharacter(args[0], args[1], args[2]);
						return;
					}
				case "changeanimation":
				case "animation":
				case "anim":
					{
						if (args.Length != 2)
							throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						CharacterManager.changeAnimation(args[0], args[1]);
						return;
					}
				case "movecharacter":
				case "movechar":
					{
						if (args.Length != 2)
							throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						CharacterManager.moveCharacter(args[0], args[1]);
						return;
					}
				case "changebg":
					{
						if (args.Length != 1)
							throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						TransitionManager.Instance.Backgrounds.SetNewStaticBackground(args[0]);
						TransitionManager.Instance.Backgrounds.SetNewAnimatedBackground(args[0]);
						return;
					}
				case "changescript":
					{
						if (args.Length == 0 || args.Length > 2)
							throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						ScriptParser.Instance.scriptChanged = true;
						if (args.Length == 1)
							ScriptParser.Instance.ChangeScriptFile(args[0]);
						else if (int.TryParse(args[1], out int num))
							ScriptParser.Instance.ChangeScriptFile(args[0], num);
						else
							throw new ArgumentException($"{args[1]} is not a valid number!");
						return;
					}
				case "emptyscene":
					{
						if (args.Length > 0)
							throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						CharacterManager.Instance.emptyScene();
					}
					return;
				case "loopbg":
					{
						if (args.Length != 1)
							throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						if (enabledHashset.Contains(args[0].ToLower()))
							TransitionManager.Instance.Backgrounds.LoopingAnimBG = true;
						else if (disabledHashset.Contains(args[0].ToLower()))
							TransitionManager.Instance.Backgrounds.LoopingAnimBG = false;
						else
							throw new ArgumentException($"{args[0]} is not a valid " +
								$"argument for {command}!");
					}
					return;
				case "playbg":
					{
						string bgName = args.Length == 0 ? ""
							: args.Length == 1 ? args[0].ToLower()
							: throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						TransitionManager.Instance.Backgrounds.SetNewAnimatedBackground(bgName);
						return;
					}
				case "changename":
					{
						if (args.Length != 2)
							throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						CharacterManager.Instance.changeName(args[0], args[1]);
						return;
					}
				case "choice":
					if (args.Length != 1)
						throw new ArgumentException($"Command '{command}' " +
							$"doesn't take {args.Length} arguments!");
					ScriptParser.Instance.paused = true;
					ScriptParser.Instance.currentNode.parseChoice(args[0].ToString().Trim());
					return;
				case "setbool":
					if (args.Length != 2)
						throw new ArgumentException($"Command '{command}' " +
							$"doesn't take {args.Length} arguments!");
					PersistentData.bools[args[0].ToString().Trim()] = bool.Parse(args[1].ToString().Trim());
					return;
				case "if":
					ScriptParser.Instance.paused = true;

					DialogueNode conditional = ScriptParser.Instance.currentNode.makeConditionalNode();
					bool var = PersistentData.bools[args[0].ToString().Trim()];
					bool val = bool.Parse(args[1].ToString().Trim());
					if (var == val)
					{
						// Debug.Log("Condition met. Playing optional block...");
						ScriptParser.Instance.currentNode = conditional;
						ScriptParser.Instance.paused = false;
					}
					else
					{
						// Debug.Log("Condition not met. Skipping...");
						ScriptParser.Instance.currentNode.index--;
						ScriptParser.Instance.paused = false;
					}
					return;
			}
			throw new Exception();
			/*
			Task CommandBuilder()
			{
				var consoleLineBuilder = new StringBuilder($"Parsing command: {command}");
				if (args.Length > 0)
				{
					consoleLineBuilder.Append(" with Commands: ");
					for (int i = 0; i < args.Length; i++)
						consoleLineBuilder.Append($"{args[i]}{(i == args.Length - 1 ? "" : ", ")}");
				}
				B1NARYConsole.Log(nameof(CommandsManager), consoleLineBuilder.ToString());
				return Task.CompletedTask;
			}*/
		}
	}
}