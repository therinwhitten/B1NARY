namespace HDConsole
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Text;
	using TMPro;
	using UnityEngine;

	/// <summary>
	/// A command to invoke or change various aspects for the game. Perhaps acting
	/// as a way to change config files from the console, or actually testing the 
	/// game quickly without creating custom scenes. Maybe you or your friends on
	/// your own game server wants to dick around with 50,000 bosses sitting in the
	/// spawn room.
	/// <para>
	/// There are multiple types of commands; which usually has the snake case
	/// of the main command and 0-1 for enum/boolean implementation. Here are a few
	/// starting tags to make finding tags generally easier:
	/// </para>
	/// <list type="bullet">
	/// <item>sv_ = Server, usually gated by cheats or mod-only commands, as these impact the server themselves. </item>
	/// <item>cons_ = Console, typically impacting the console or the console's memory itself. </item>
	/// <item>cl_ = Impacts the client, usually has to do with graphics or visual effects. </item>
	/// <item>obj_ = Impacts existing gameobjects. Barebones typically. </item>
	/// </list>
	/// <para>
	/// Note that some commands that can be most frequently used dont have to follow
	/// the 'starting abbreviation' rule.
	/// </para>
	/// </summary>
	public struct HDCommand
	{
		public const string CONSOLE_PREFIX = "cons";
		public const string SERVER_PREFIX = "sv";
		public const string CLIENT_PREFIX = "cl";
		public const string GAMEOBJECT_PREFIX = "obj";

		public enum MainTags : byte
		{
			None = 0,
			ServerModOnly = 1,
			Cheat = 2,
		}
		public static string[] SplitWithQuotations(string input)
		{
			List<string> modified = new(input.Length);
			StringBuilder currentLine = new();
			bool inQuotations = false;

			for (int i = 0; i < input.Length; i++)
			{
				if (input[i] == ' ' && !inQuotations)
				{
					if (currentLine.Length <= 0)
						continue;
					modified.Add(currentLine.ToString());
					currentLine = new StringBuilder();
					continue;
				}
				if (input[i] == '"')
				{
					inQuotations = !inQuotations;
					continue;
				}
				currentLine.Append(input[i]);
			}
			if (currentLine.Length > 0)
				modified.Add(currentLine.ToString());
			return modified.ToArray();
		}

		#region Auto-Complete
		internal static object lastObjectGet = null;
		public static HDCommand AutoCompleteBool(string command, Func<bool> getBool, Action<bool> setBool, MainTags tags = MainTags.None, string description = "") => new()
		{
			command = command,
			requiredArguments = Array.Empty<string>(),
			optionalArguments = new string[] { "0-1" },
			description = description,
			mainTags = tags,
			invoke = (args) =>
			{
				if (args.Length <= 0)
				{
					bool value = getBool.Invoke();
					lastObjectGet = value;
					if (HDConsole.Instance.enabled)
						HDConsole.WriteLine($"{command} {(value ? "1" : "0")}");
					return;
				}
				bool setTo = ParseFrom01(args[0]);
				setBool.Invoke(setTo);
			},
		};
		public static bool ParseFrom01(string line) => line switch { "0" => false, "1" => true, _ => throw new InvalidCastException(line) };
		public static HDCommand AutoCompleteFloat(string command, Func<float> getFloat, Action<float> setFloat, float min, float max, MainTags tags = MainTags.None, string description = "") => new()
		{
			command = command,
			requiredArguments = Array.Empty<string>(),
			optionalArguments = new string[] { $"{min}-{max}" },
			description = description,
			mainTags = tags,
			invoke = (args) =>
			{
				if (args.Length <= 0)
				{
					float value = getFloat.Invoke();
					lastObjectGet = value;
					if (HDConsole.Instance.enabled)
						HDConsole.WriteLine($"{command} {value}");
					return;
				}
				float setTo = float.Parse(args[0]);
				setTo = Math.Clamp(setTo, min, max);
				setFloat.Invoke(setTo);
			},
		};
		public static HDCommand AutoCompleteFloat(string command, Func<float> getFloat, Action<float> setFloat, MainTags tags = MainTags.None, string description = "") => new()
		{
			command = command,
			requiredArguments = Array.Empty<string>(),
			optionalArguments = new string[] { $"value" },
			description = description,
			mainTags = tags,
			invoke = (args) =>
			{
				if (args.Length <= 0)
				{
					float value = getFloat.Invoke();
					lastObjectGet = value;
					if (HDConsole.Instance.enabled)
						HDConsole.WriteLine($"{command} {value}");
					return;
				}
				float setTo = float.Parse(args[0]);
				setFloat.Invoke(setTo);
			},
		};
		public static HDCommand AutoCompleteInt(string command, Func<int> getInt, Action<int> setInt, int min, int max, MainTags tags = MainTags.None, string description = "") => new()
		{
			command = command,
			requiredArguments = Array.Empty<string>(),
			optionalArguments = new string[] { $"{min}-{max}" },
			description = description,
			mainTags = tags,
			invoke = (args) =>
			{
				if (args.Length <= 0)
				{
					int value = getInt.Invoke();
					lastObjectGet = value;
					if (HDConsole.Instance.enabled)
						HDConsole.WriteLine($"{command} {value}");
					return;
				}
				int setTo = int.Parse(args[0]);
				setTo = Math.Clamp(setTo, min, max);
				setInt.Invoke(setTo);
			},
		};
		public static HDCommand AutoCompleteInt(string command, Func<int> getInt, Action<int> setInt,  MainTags tags = MainTags.None, string description = "") => new()
		{
			command = command,
			requiredArguments = Array.Empty<string>(),
			optionalArguments = new string[] { $"value" },
			description = description,
			mainTags = tags,
			invoke = (args) =>
			{
				if (args.Length <= 0)
				{
					int value = getInt.Invoke();
					lastObjectGet = value;
					if (HDConsole.Instance.enabled)
						HDConsole.WriteLine($"{command} {value}");
					return;
				}
				int setTo = int.Parse(args[0]);
				setInt.Invoke(setTo);
			},
		};
		public static HDCommand AutoCompleteEnum<TEnum>(string command, Func<TEnum> getEnum, Action<TEnum> setEnum, MainTags tags = MainTags.None, string description = "") where TEnum : Enum => new()
		{
			command = command,
			requiredArguments = Array.Empty<string>(),
			optionalArguments = new string[] { string.Join('/', ((TEnum[])Enum.GetValues(typeof(TEnum))).Select(@enum => @enum.GetHashCode())) },
			description = description,
			mainTags = tags,
			invoke = (args) =>
			{
				if (args.Length <= 0)
				{
					TEnum value = getEnum.Invoke();
					lastObjectGet = value;
					if (HDConsole.Instance.enabled)
						HDConsole.WriteLine($"{command} {value}");
					return;
				}
				TEnum outEnum = (TEnum)Enum.Parse(typeof(TEnum), args[0]);
				setEnum.Invoke(outEnum);
			},
		};
		public static HDCommand AutoCompleteString(string command, Func<string> getString, Action<string> setString, string argumentName, MainTags tags = MainTags.None, string description = "") => new()
		{
			command = command,
			requiredArguments = Array.Empty<string>(),
			optionalArguments = new string[] { argumentName },
			description = description,
			mainTags = tags,
			invoke = (args) =>
			{
				if (args.Length <= 0)
				{
					string value = getString.Invoke();
					lastObjectGet = value;
					if (HDConsole.Instance.enabled)
						HDConsole.WriteLine($"{command} {value}");
					return;
				}
				string outString = args[0];
				setString.Invoke(outString);
			},
		};
		#endregion

		public string command;
		public string description;
		public IList<string> requiredArguments;
		public IList<string> optionalArguments;
		public MainTags mainTags;
		private Action<string[]> invoke;

		public HDCommand(string command, Action<string[]> invoke)
		{
			mainTags = MainTags.None;
			this.command = command;
			description = string.Empty;
			requiredArguments = Array.Empty<string>();
			optionalArguments = Array.Empty<string>();
			this.invoke = invoke;
		}
		public HDCommand(string command, IList<string> requiredArguments, Action<string[]> invoke)
		{
			mainTags = MainTags.None;
			this.command = command;
			description = string.Empty;
			this.requiredArguments = requiredArguments;
			optionalArguments = Array.Empty<string>();
			this.invoke = invoke;
		}
		public HDCommand(string command, IList<string> requiredArguments, IList<string> optionalArguments, Action<string[]> invoke)
		{
			mainTags = MainTags.None;
			this.command = command;
			description = string.Empty;
			this.requiredArguments = requiredArguments;
			this.optionalArguments = optionalArguments;
			this.invoke = invoke;
		}

		public void Invoke(string[] arguments)
		{
			invoke.Invoke(arguments);
		}

		public override string ToString()
		{
			StringBuilder builder = new(command);
			string required = RequiredArgumentsToString();
			if (!string.IsNullOrEmpty(required))
				builder.Append($" {required}");
			string optional = OptionalArgumentsToString();
			if (!string.IsNullOrEmpty(optional))
				builder.Append($" {optional}");
			return builder.ToString();
		}
		public string RequiredArgumentsToString()
		{
			List<string> builder = new();
			for (int i = 0; i < requiredArguments.Count; i++)
				builder.Add($"<{requiredArguments[i]}>");
			return string.Join(' ', builder);
		}
		public string OptionalArgumentsToString()
		{
			List<string> builder = new();
			for (int i = 0; i < optionalArguments.Count; i++)
				builder.Add($"[{optionalArguments[i]}]");
			return string.Join(' ', builder);
		}
	}
}
