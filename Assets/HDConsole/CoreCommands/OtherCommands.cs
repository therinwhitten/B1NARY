namespace HDConsole
{
	using System;
	using System.Text;
	using UnityEngine.Diagnostics;
	using UnityEngine;
	using System.IO;
	using UnityEngine.SceneManagement;

	public static partial class CoreCommands
	{
		[return: CommandsFromGetter]
		public static HDCommand[] OtherCommands() => new HDCommand[]
		{
			new HDCommand($"scene", new string[] { "scene name" }, (args) =>
			{
				SceneManager.LoadScene(args[0]);
			}) { description = "Loads in the scene." },

			new HDCommand($"increment_var_int", new string[] { "command", "min", "max", "delta" }, (args) =>
			{
				// Get value, hopefully through auto.
				HDConsole.InvokeThoughConsole(args[0]);
				int value = (int)HDCommand.lastObjectGet;
				value = Math.Clamp(value + int.Parse(args[3]), int.Parse(args[1]), int.Parse(args[2]));
				HDConsole.InvokeThoughConsole(args[0], value.ToString());
			}) { description = "Everytime the command is invoked, it will try to change the existing value by basing the auto-complete\n" +
				"commands. Uses integer values for changing values, which include solid 0-1 values." },
			
			new HDCommand($"increment_var_float", new string[] { "command", "min", "max", "delta" }, (args) =>
			{
				// Get value, hopefully through auto.
				HDConsole.InvokeThoughConsole(args[0]);
				float value = (float)HDCommand.lastObjectGet;
				value = Math.Clamp(value + float.Parse(args[3]), float.Parse(args[1]), float.Parse(args[2]));
				HDConsole.InvokeThoughConsole(args[0], value.ToString());
			}) { description = "Everytime the command is invoked, it will try to change the existing value by basing the auto-complete\n" +
				"commands. Uses floating point values for changing values." },
		};
	}
}