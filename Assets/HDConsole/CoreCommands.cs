namespace HDConsole
{
	using System;
	using System.Text;
	using UnityEngine.Diagnostics;
	using UnityEngine;
	using System.IO;
	using UnityEngine.SceneManagement;

	internal static class CoreCommands
	{
		public static string CONSOLE_PREFIX = "cons";
		[return: CommandToConsole]
		public static HDCommand[] ConsoleCommands() => new HDCommand[]
		{
			new HDCommand($"{CONSOLE_PREFIX}_display_all_commands", (args) =>
			{
				StringBuilder builder = new("All Commands:\n");
				for (int i = 0; i < HDConsole.Instance.commands.Count; i++)
					builder.AppendLine($"\t{HDConsole.Instance.commands[i]}");
				HDConsole.WriteLine(builder.ToString());
			}) { description = "Gets all the commands and prints them in the console." },

			new HDCommand($"{CONSOLE_PREFIX}_clear_console", (args) =>
			{
				HDConsole.Instance.consoleTextMemory.Clear();
				HDConsole.Instance.consoleText.text = "";
			}) { description = "In case the console goofs up from certain commands or logs, this clears it." },

			new HDCommand($"quit", (args) =>
			{
				Application.Quit();
			}),

			new HDCommand($"{CONSOLE_PREFIX}_credits", (args) =>
			{
				HDConsole.WriteLine($"This console is written by AnOddDoorKnight\n - https://steamcommunity.com/id/AnOddDoorKnight/\n - https://twitter.com/AnOddDoorKnight");
			}) { description = "Prints the console creator's name." },

			new HDCommand($"{CONSOLE_PREFIX}_fatal_crash", (args) =>
			{
				Utils.ForceCrash(ForcedCrashCategory.Abort);
			}) { description = "Tells the unity engine that its worse than Gorbino's Quest, which destroys itself from depression." },

			new HDCommand($"{CONSOLE_PREFIX}_log", new string[] { "text" }, new string[] { "type" }, (args) =>
			{
				LogType type = LogType.Log;
				if (args.Length > 1)
					type = Enum.Parse<LogType>(args[1]);
				HDConsole.WriteLine(type, args[0]);
			}),

			new HDCommand($"{CONSOLE_PREFIX}_open_logs", (args) =>
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

			new HDCommand($"{CONSOLE_PREFIX}_clear_commands", (args) =>
			{
				HDConsole.Instance.consoleCommandMemory.Clear();
				PlayerPrefs.SetString(HDConsole.MEM_CMD, null);
				HDConsole.Instance.UpdateOtherCommandList();
			}) { description = "Clears all commands written by the player that is saved in memory." },

		};

		public static string GAMEOBJECT_PREFIX = "obj";
		[return: CommandToConsole]
		public static HDCommand[] GameObjectCommands() => new HDCommand[]
		{
			new HDCommand($"{GAMEOBJECT_PREFIX}_fire", new string[] { "Gameobject Name" }, (args) =>
			{
				GameObject obj = GameObject.Find(args[0]);
				if (obj == null)
					throw new MissingReferenceException($"Object '{args[0]}' is not found");
				UnityEngine.Object.Destroy(obj);
			}) { description = "Snaps an object out of existence." },

			new HDCommand($"{GAMEOBJECT_PREFIX}_load", new string[] { "Gameobject Name", "x", "y", "z" }, (args) =>
			{
				GameObject obj = Resources.Load<GameObject>(args[0]);
				if (obj == null)
					throw new MissingReferenceException($"Object '{args[0]}' is not found in resources");
				obj = UnityEngine.Object.Instantiate(obj);
				obj.transform.position = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
			}) { description = "Loads (and Instantiates) a resource object into the current scene played back." },

			new HDCommand($"{GAMEOBJECT_PREFIX}_move", new string[] { "Gameobject Name", "x", "y", "z" }, (args) =>
			{
				GameObject obj = GameObject.Find(args[0]);
				if (obj == null)
					throw new MissingReferenceException($"Object '{args[0]}' is not found");
				obj.transform.position = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
			}) { description = "Moves a specific gameobject's position in the world." },

			new HDCommand($"{GAMEOBJECT_PREFIX}_display_all_root_objects", (args) =>
			{
				GameObject[] allObj = SceneManager.GetActiveScene().GetRootGameObjects();
				StringBuilder builder = new("All Scene Objects: \n");
				for (int i = 0; i < allObj.Length; i++)
					builder.AppendLine($"\t{allObj[i].name}");
				HDConsole.WriteLine(builder.ToString());
			}) { description = "Displays all root objects in the current scene being played back." },


		};

		[return: CommandToConsole]
		public static HDCommand[] OtherCommands() => new HDCommand[]
		{
			new HDCommand($"scene", new string[] { "scene name" }, (args) =>
			{
				SceneManager.LoadScene(args[0]);
			}) { description = "Loads in the scene." },
		};
	}
}