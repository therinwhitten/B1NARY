namespace B1NARY
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;
	using System.Text;
	using System.Threading.Tasks;
	using B1NARY.Sounds;
	using B1NARY.UI;
	using B1NARY.DataPersistence;

	public static class CommandsManager
	{
		private static DialogueSystem Dialogue => DialogueSystem.Instance;
		private static CharacterManager CharacterManager => CharacterManager.Instance;

		private static readonly HashSet<string> enabledHashset = new HashSet<string>()
		{ "on", "true", "enable" };
		private static readonly HashSet<string> disabledHashset = new HashSet<string>()
		{ "off", "false", "disable" };


		public static async void HandleWithArgs(string command, string[] args = null)
		{
			command = command.ToLower();
			if (args == null)
				args = Array.Empty<string>();
			Task commandBuilder = CommandBuilder();
			args = args.Select(@string => @string.Trim()).ToArray();
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
						TransitionHandler.Instance.SetNewStaticBackground(args[0]);
						TransitionHandler.Instance.SetNewAnimatedBackground(args[0]);
						break;
					}
				case "changescene":
					{
						if (args.Length != 1)
							throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						_ = TransitionHandler.Instance.TransitionToNextScene(args[0], 0.5f);
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
							TransitionHandler.Instance.LoopingAnimBG = true;
						else if (disabledHashset.Contains(args[0].ToLower()))
							TransitionHandler.Instance.LoopingAnimBG = false;
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
						TransitionHandler.Instance.SetNewAnimatedBackground(bgName);
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
				// Sounds
				case "fadeinsound":
					{
						if (args.Length != 2)
							throw new ArgumentException($"Command '{command}' " +
								$"doesn't take {args.Length} arguments!");
						float args1 = float.Parse(args[1]);
						try { AudioHandler.Instance.PlayFadedSound(args[0], args1); }
						catch (SoundNotFoundException)
						{
							Debug.LogWarning($"{args[0]} is not a valid soundfile Path!");
						}
						break;
					}
				case "fadeoutsound":
					if (args.Length != 2)
						throw new ArgumentException($"Command '{command}' " +
							$"doesn't take {args.Length} arguments!");
					try
					{
						AudioHandler.Instance.StopSoundViaFade(args[0].ToString().Trim(), float.Parse(args[1].ToString().Trim()));
					}
					catch (SoundNotFoundException ex)
					{
						Debug.LogWarning($"{args[0].ToString().Trim()} is not a valid soundfile Path!"
						+ ex);
					}
					catch (KeyNotFoundException ex)
					{
						Debug.LogWarning($"Cannot find sound to close: {args[0].ToString().Trim()}\n"
						+ ex);
					}
					break;
				case "playsound":
					if (args.Length != 1)
						throw new ArgumentException($"Command '{command}' " +
							$"doesn't take {args.Length} arguments!");
					try
					{
						AudioHandler.Instance.PlaySound(args[0].ToString().Trim());
					}
					catch (SoundNotFoundException ex)
					{
						Debug.LogWarning($"{args[0].ToString().Trim()} is not a valid soundfile Path!"
						+ ex);
					}
					catch (KeyNotFoundException ex)
					{
						Debug.LogWarning($"Cannot find sound: {args[0].ToString().Trim()}\n"
						+ ex);
					}
					break;
				case "stopsound":
					if (args.Length != 1)
						throw new ArgumentException($"Command '{command}' " +
							$"doesn't take {args.Length} arguments!");
					try
					{
						AudioHandler.Instance.StopSound(args[0].ToString().Trim());
					}
					catch (SoundNotFoundException ex)
					{
						Debug.LogWarning($"{args[0].ToString().Trim()} is not a valid soundfile Path!"
						+ ex);
					}
					catch (KeyNotFoundException ex)
					{
						Debug.LogWarning($"Cannot find sound to close: {args[0].ToString().Trim()}\n"
						+ ex);
					}
					break;
				/*
			case "startdelayedsound":
				{
					if (Extensions.TryInvoke<Coroutine, SoundNotFoundException>(() =>
					StartCoroutine(Delay(float.Parse(args[0].ToString().Trim()), () => AudioHandler.Instance.PlaySound(args[0].ToString().Trim()))), out _, out _))
						Debug.LogWarning($"{args[0]} is not a valid soundfile Path!");
					IEnumerator Delay(float seconds, Action playAfterDelay)
					{
						yield return new WaitForSeconds(seconds);
						playAfterDelay.Invoke();
					}
				}
				break;
				*/
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
			await commandBuilder;

			Task CommandBuilder()
			{
				var consoleLineBuilder = new StringBuilder($"Parsing command: {command}");
				if (args.Length > 0)
				{
					consoleLineBuilder.Append(" with Commands: ");
					for (int i = 0; i < args.Length; i++)
						consoleLineBuilder.Append($"{args[i]}{(i == args.Length - 1 ? "" : ",")}");
				}
				B1NARYConsole.Log(nameof(CommandsManager), consoleLineBuilder.ToString());
				return Task.CompletedTask;
			}
		}
	}
}