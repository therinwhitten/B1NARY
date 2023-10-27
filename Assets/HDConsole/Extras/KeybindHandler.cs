namespace HDConsole
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.InputSystem;
	using UnityEngine.InputSystem.Controls;
	using UnityEngine.InputSystem.LowLevel;

	// This class was implemented before multi-command commands were a thing,
	// - dont think about it too hard.
	internal static class KeybindHandler
	{
		public record BoundCommandValue(IList<string> Commands, InputAction Action);


		[return: CommandsFromGetter]
		public static HDCommand[] GetHDCommands() => new HDCommand[]
		{
			new HDCommand("bind", new string[] { "key" }, new string[] { "commands" }, (args) =>
			{
				if (args.Length == 1) // Display key command only
				{
					if (!boundCommands.TryGetValue(args[0], out BoundCommandValue commandValue))
					{
						HDConsole.WriteLine($"'{args[0]}' is not bound.");
						return;
					}
					HDConsole.WriteLine($"bind '{args[0]}' is '{CommandToString(commandValue.Commands)}'");
					return;
				}
				// Setting new bind
				BindNewKey(args[0], StringToCommandList($"bind {string.Join(' ', args)}"));
			}) { description = "An in-efficient way to invoke console commands via script.\nStack them back by typing bind {key} \"command1\" \"command2\"", },

			new HDCommand("unbind", new string[] { "key" }, (args) =>
			{
				RemoveKey(args[0]);
			}) { description = "Unbinds some inefficient buttons." },

			new HDCommand("unbind_all", (args) =>
			{
				RemoveAll();
			}) { description = "Actually is less destructive than you think; Unbinds keybinds from console." },

			Delay,
		};
		internal static HDCommand Delay =
			new("delay", new string[] { "delay (seconds)" }, (args) => { })
			{ description = "delays in seconds. Only useful for multiple commands." };

		public static SortedList<string, BoundCommandValue> boundCommands = new();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Constructor()
		{
			const string KEY = "HDConsole Keybinds";
			try
			{
				string output = PlayerPrefs.GetString(KEY, null);
				if (!string.IsNullOrEmpty(output))
					DeserializeFromString(output);
				
			} catch (Exception ex)
			{
				Debug.LogException(ex);
				PlayerPrefs.DeleteKey(KEY);
			}

			Application.quitting += () =>
			{
				PlayerPrefs.SetString(KEY, SerializeAsString(boundCommands));
				for (int i = 0; i < boundCommands.Values.Count; i++)
					boundCommands.Values[i].Action.Dispose();
			};
		}


		internal static void InvokeKeybind(string key)
		{
			IList<string> commands = boundCommands[key].Commands;
			HDConsole.Instance.StartCoroutine(Enumerator());
			IEnumerator Enumerator()
			{
				for (int i = 0; i < commands.Count; i++)
				{
					if (commands[i].StartsWith(Delay.command))
					{
						string[] split = HDCommand.SplitWithQuotations(commands[i]);
						yield return new WaitForSecondsRealtime(float.Parse(split[1]));
						continue;
					}
					HDConsole.Instance.InvokeFromString(commands[i]);
				}
			}
		}
		public static void BindNewKey(string key, IList<string> commands)
		{
			if (boundCommands.ContainsKey(key))
				RemoveKey(key);
			KeyControl keyControl = Keyboard.current.FindKeyOnCurrentKeyboardLayout(key);
			InputAction action = new(key, InputActionType.Button);
			action.AddBinding(keyControl);
			action.performed += (context) =>
			{
				if (!HDConsole.Instance.enabled)
					InvokeKeybind(key);
			};
			action.Enable();
			boundCommands[key] = new BoundCommandValue(commands, action);
		}
		public static void RemoveKey(string key)
		{
			BoundCommandValue boundCommandValue = boundCommands[key];
			boundCommandValue.Action.Dispose();
			boundCommands.Remove(key);
		}
		public static void RemoveAll()
		{
			for (int i = 0; i < boundCommands.Values.Count; i++)
				boundCommands.Values[i].Action.Dispose();
			boundCommands.Clear();
		}


		private static string CommandToString(IList<string> commands)
		{
			if (commands.Count < 2)
				return commands.FirstOrDefault();
			return string.Join("; ", commands);
		}
		private static IList<string> StringToCommandList(string rawInput)
		{
			rawInput = string.Join(' ', rawInput.Split(' ').Skip(2));
			string[] splitCommands = rawInput.Split(';');
			return splitCommands;
		}

		private static string SerializeAsString(IEnumerable<KeyValuePair<string, BoundCommandValue>> values)
		{
			StringBuilder output = new();
			using var enumerable = values.GetEnumerator();
			while (enumerable.MoveNext())
			{
				string commandOutput = CommandToString(enumerable.Current.Value.Commands);
				output.Append($"{enumerable.Current.Key}\n{commandOutput}\r");
			}

			return output.ToString();
		}
		private static void DeserializeFromString(string serializedString)
		{
			string[] split = serializedString.Split('\r');
			for (int i = 0; i < split.Length - 1; i++)
			{
				string[] line = split[i].Split('\n');
				string key = line[0];
				IList<string> commands = StringToCommandList(line[1]);
				BindNewKey(key, commands);
			}
		}
	}
}
