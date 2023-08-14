namespace HDConsole
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using TMPro;
	using UnityEngine;

	public struct HDCommand
	{
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

		public string command;
		public string description;
		public string[] requiredArguments;
		public string[] optionalArguments;

		private readonly Action<string[]> invoke;

		public HDCommand(string command, Action<string[]> invoke)
		{
			this.command = command;
			description = string.Empty;
			requiredArguments = Array.Empty<string>();
			optionalArguments = Array.Empty<string>();
			this.invoke = invoke;
		}
		public HDCommand(string command, string[] requiredArguments, Action<string[]> invoke)
		{
			this.command = command;
			description = string.Empty;
			this.requiredArguments = requiredArguments;
			optionalArguments = Array.Empty<string>();
			this.invoke = invoke;
		}
		public HDCommand(string command, string[] requiredArguments, string[] optionalArguments, Action<string[]> invoke)
		{
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
			for (int i = 0; i < requiredArguments.Length; i++)
				builder.Add($"<{requiredArguments[i]}>");
			return string.Join(' ', builder);
		}
		public string OptionalArgumentsToString()
		{
			List<string> builder = new();
			for (int i = 0; i < optionalArguments.Length; i++)
				builder.Add($"[{optionalArguments[i]}]");
			return string.Join(' ', builder);
		}
	}
}
