namespace B1NARY.Scripting.Experimental
{
	using B1NARY.UI;
	using B1NARY.Audio;
	using B1NARY.DataPersistence;
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UnityEngine;

	public static partial class GameCommands
	{
		private static DialogueSystem Dialogue => DialogueSystem.Instance;
		private static CharacterManager CharacterManager => CharacterManager.Instance;

		private static readonly HashSet<string> enabledHashset = new HashSet<string>()
		{ "on", "true", "enable" };
		private static readonly HashSet<string> disabledHashset = new HashSet<string>()
		{ "off", "false", "disable" };

		public static Task HandleScriptCommand(ScriptLine line)
		{
			var pair = ScriptLine.CastCommand(line);
			return HandleScriptCommand(pair.command, pair.arguments);
		}
		public static Task HandleScriptCommand(string command, params string[] args)
		{
			command = command.ToLower();
			if (AudioHandler.AudioDelegateCommands.TryGetValue(command, out var @delegate))
			{
				@delegate.DynamicInvoke(args);
				return Task.CompletedTask;
			}
			else if (SceneManager.SceneDelegateCommands.TryGetValue(command, out var del2))
			{
				del2.DynamicInvoke(args);
				return Task.CompletedTask;
			}
			switch (command)
			{
				case "additive":
					{
						if (args.Length != 1)
							throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						if (enabledHashset.Contains(args[0].ToLower()))
						{
							Dialogue.Say("");
							Dialogue.additiveTextEnabled = true;
							// ScriptParser.Instance.currentNode.nextLine();
							// ScriptParser.Instance.parseLine(ScriptParser.Instance.currentNode.GetCurrentLine());
						}
						else if (disabledHashset.Contains(args[0].ToLower()))
						{
							Dialogue.additiveTextEnabled = false;
							// Debug.Log("Set additive text to off!");
							// ScriptParser.Instance.currentNode.nextLine();
							// ScriptParser.Instance.parseLine(ScriptParser.Instance.currentNode.GetCurrentLine());
						}
						else
							throw new ArgumentException($"{args[0]} is not a valid " +
								$"argument for {command}!");
						break;
					}
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
						break;
					}
				case "changeanimation":
				case "animation":
				case "anim":
					{
						if (args.Length != 2)
							throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						CharacterManager.changeAnimation(args[0], args[1]);
						break;
					}
				case "movecharacter":
				case "movechar":
					{
						if (args.Length != 2)
							throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						CharacterManager.moveCharacter(args[0], args[1]);
						break;
					}
				case "changebg":
					{
						if (args.Length != 1)
							throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						TransitionManager.Instance.Backgrounds.SetNewStaticBackground(args[0]);
						TransitionManager.Instance.Backgrounds.SetNewAnimatedBackground(args[0]);
						break;
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
						break;
					}
				case "emptyscene":
					{
						if (args.Length > 0)
							throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						CharacterManager.Instance.emptyScene();
					}
					break;
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
					break;
				case "playbg":
					{
						string bgName = args.Length == 0 ? ""
							: args.Length == 1 ? args[0].ToLower()
							: throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						TransitionManager.Instance.Backgrounds.SetNewAnimatedBackground(bgName);
						break;
					}
				case "changename":
					{
						if (args.Length != 2)
							throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						CharacterManager.Instance.changeName(args[0], args[1]);
						break;
					}
				case "choice":
					if (args.Length != 1)
						throw new ArgumentException($"Command '{command}' " +
							$"doesn't take {args.Length} arguments!");
					ScriptParser.Instance.paused = true;
					ScriptParser.Instance.currentNode.parseChoice(args[0].ToString().Trim());
					break;
				case "setbool":
					if (args.Length != 2)
						throw new ArgumentException($"Command '{command}' " +
							$"doesn't take {args.Length} arguments!");
					PersistentData.bools[args[0].ToString().Trim()] = bool.Parse(args[1].ToString().Trim());
					break;
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
					break;
			}
			return Task.CompletedTask;
		}
	}
}