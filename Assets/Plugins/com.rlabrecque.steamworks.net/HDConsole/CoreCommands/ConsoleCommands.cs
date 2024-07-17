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
		public static HDCommand[] ConsoleCommands() => new HDCommand[]
		{
			new HDCommand($"{HDCommand.CONSOLE_PREFIX}_display_all_commands", (args) =>
			{
				StringBuilder builder = new("All Commands:\n");
				for (int i = 0; i < HDConsole.Instance.commands.Count; i++)
					builder.AppendLine($"\t{HDConsole.Instance.commands[i]}");
				HDConsole.WriteLine(builder.ToString());
			}) { description = "Gets all the commands and prints them in the console." },

			new HDCommand($"{HDCommand.CONSOLE_PREFIX}_clear_console", (args) =>
			{
				HDConsole.Instance.consoleTextMemory.Clear();
				HDConsole.Instance.consoleText.text = "";
			}) { description = "In case the console goofs up from certain commands or logs, this clears it." },

			new HDCommand($"quit", (args) =>
			{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.ExitPlaymode();
#else
			Application.Quit();
#endif
			}),

			new HDCommand($"{HDCommand.CONSOLE_PREFIX}_credits", (args) =>
			{
				HDConsole.WriteLine($"This console is written by AnOddDoorKnight\n - https://steamcommunity.com/id/AnOddDoorKnight/\n - https://twitter.com/AnOddDoorKnight");
			}) { description = "Prints the console creator's name." },

			new HDCommand($"{HDCommand.CONSOLE_PREFIX}_fatal_crash", (args) =>
			{
				Utils.ForceCrash(ForcedCrashCategory.Abort);
			}) { description = "Tells the unity engine that its worse than Gorbino's Quest, which destroys itself from depression." },

			new HDCommand($"{HDCommand.CONSOLE_PREFIX}_log", new string[] { "text" }, new string[] { "type" }, (args) =>
			{
				LogType type = LogType.Log;
				if (args.Length > 1)
					type = Enum.Parse<LogType>(args[1]);
				HDConsole.WriteLine(type, args[0]);
			}),

			new HDCommand($"{HDCommand.CONSOLE_PREFIX}_open_logs", (args) =>
			{
#if UNITY_EDITOR
				string path = @"%LOCALAPPDATA%\Unity\Editor\Editor.log";
				System.Diagnostics.Process.Start(path);
#elif UNITY_STANDALONE_WIN
				string path = @$"%USERPROFILE%\AppData\LocalLow\{Application.companyName}\{Application.productName}\Player.log";
				path = Path.GetFullPath(path);
				System.Diagnostics.Process.Start(path);
#else
				HDConsole.WriteLine(LogType.Log, $"sorry but opening {Environment.OSVersion.Platform} from game at this time isn't possible, since it isn't coded in.");
#endif
			}) { description = "Tries to open your log file, may or may not work." },

			new HDCommand($"{HDCommand.CONSOLE_PREFIX}_clear_commands", (args) =>
			{
				HDConsole.Instance.consoleCommandMemory.Clear();
				PlayerPrefs.SetString(HDConsole.MEM_CMD, null);
				HDConsole.Instance.UpdateOtherCommandList();
			}) { description = "Clears all commands written by the player that is saved in memory." },

			HDCommand.AutoCompleteBool($"{HDCommand.SERVER_PREFIX}_cheats", HDConsole.CheatsEnabled, (@bool) => HDConsole.CheatsEnabled = () => @bool,
				HDCommand.MainTags.ServerModOnly, "Enables or disables cheats on a server, or during ingame."),

			HDCommand.AutoCompleteInt($"{HDCommand.SERVER_PREFIX}_line_capacity", () => HDConsole.LineCapacity, set => HDConsole.LineCapacity = set, description: "how many lines the console can store/display."),
		};
	}
}
