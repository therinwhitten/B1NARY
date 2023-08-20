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

	internal static class AliasHandler
	{
		public record BoundAliasValue(IList<string> Commands);


		[return: CommandToConsole]
		public static HDCommand[] GetHDCommands() => new HDCommand[]
		{
			new HDCommand("alias_set", new string[] { "name" }, new string[] { "commands" }, (args) =>
			{
				if (args.Length == 1) // Display key command only
				{
					if (!boundCommands.TryGetValue(args[0], out BoundAliasValue commandValue))
					{
						HDConsole.WriteLine($"alias '{args[0]}' is not bound.");
						return;
					}
					HDConsole.WriteLine($"alias '{args[0]}' is '{CommandToString(commandValue.Commands)}'");
					return;
				}
				// Setting new bind
				BindNewAlias(args[0], StringToCommandList($"bind {string.Join(' ', args)}"));
			}) { description = "invoke console commands via a shorthand name.\nStack them back by typing bind {key} \"command1\" \"command2\"", },

			new HDCommand("alias", new string[] { "key" }, (args) =>
			{
				InvokeAlias(args[0]);
			}) { description = "Invokes an alias." },
		};

		public static SortedList<string, BoundAliasValue> boundCommands = new();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Constructor()
		{
			const string KEY = "HDConsole Keybinds";
			try
			{
				string output = PlayerPrefs.GetString(KEY, null);
				if (!string.IsNullOrEmpty(output))
					DeserializeFromString(output);

			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				PlayerPrefs.DeleteKey(KEY);
			}

			Application.quitting += () => PlayerPrefs.SetString(KEY, SerializeAsString(boundCommands));
		}


		internal static void InvokeAlias(string key)
		{
			IList<string> commands = boundCommands[key].Commands;
			HDConsole.Instance.StartCoroutine(Enumerator());
			IEnumerator Enumerator()
			{
				for (int i = 0; i < commands.Count; i++)
				{
					if (commands[i].StartsWith(KeybindHandler.Delay.command))
					{
						string[] split = HDCommand.SplitWithQuotations(commands[i]);
						yield return new WaitForSecondsRealtime(float.Parse(split[1]));
						continue;
					}
					HDConsole.Instance.InvokeFromString(commands[i]);
				}
			}
		}
		public static void BindNewAlias(string name, IList<string> commands)
		{
			if (boundCommands.ContainsKey(name))
				RemoveName(name);
			boundCommands[name] = new BoundAliasValue(commands);
		}
		public static void RemoveName(string key)
		{
			boundCommands.Remove(key);
		}
		public static void RemoveAll()
		{
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

		private static string SerializeAsString(IEnumerable<KeyValuePair<string, BoundAliasValue>> values)
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
				BindNewAlias(key, commands);
			}
		}
	}
}
