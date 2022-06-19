using System.IO;
using System.Linq;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class DoomWindow : EditorWindow
{
	[MenuItem("B1NARY/Doom Porter"/*,priority = 0*/)]
	public static void ShowWindow()
	{
		// Get existing open window or if none, make a new one:
		var window = (DoomWindow)GetWindow(typeof(DoomWindow));
		window.titleContent = new GUIContent("Doom Porter");
	}
	private void OnGUI()
	{
		string[] mapNames = Directory.GetFiles(DoomMapDirectory);
		mapNames = mapNames.Where(dir => dir.EndsWith(".wad")).ToArray();
		for (int i = 0; i < mapNames.Length; i++)
		{
			string name = mapNames[i].Remove(0, mapNames[i].LastIndexOfAny(new char[] { '/', '\\' }) + 1);
			name = name.Remove(name.LastIndexOf('.'));
			bool playDoomGame = GUILayout.Button(name);
			if (playDoomGame)
				RunDoomLevel(name);
		}
	}


	#region Logic & Launcher
	private static readonly string doomSourceFile = "/GZDoom";
	private static string DoomWindowsSourceFile => DoomSourceDirectory + "/Windows";
	private static string DoomSourceDirectory
		=> Application.streamingAssetsPath + doomSourceFile;
	public static string DoomMapDirectory => DoomSourceDirectory + "/Maps";
	private static readonly ProcessStartInfo defaultStartInfo =
		new ProcessStartInfo($"{DoomWindowsSourceFile}/gzdoom.exe")
		{ CreateNoWindow = true };




	public static void RunDoomLevel(string wadName, bool runAsStandard = false)
	{
		var doomProcess = defaultStartInfo;
		doomProcess.Arguments = DetermineArguments(
			$"{DoomMapDirectory}/{wadName}.wad", runAsStandard ? 0 : 1, 3);
		Process.Start(doomProcess);
	}

	private static string DetermineArguments(string mapFileLocation, int mapIndex, int skillLevel)
	{
		skillLevel = Mathf.Clamp(skillLevel, 0, 8);
		string output = string.Join(" ", new string[]
		{
			$"-config \"{DoomSourceDirectory}/gzdoom-def.ini\"",
			$"-iwad \"{DoomSourceDirectory}/Maps/DOOM2.WAD\"",
			$"-file \"{mapFileLocation}\"",
			"-savedir \"Save\"",
			"+screenshot_dir \"Screenshots\"",
		});
		if (mapIndex != 0)
			output += $" +map MAP{(mapIndex < 10 ? "0" : "")}{mapIndex}";
		output += $" -skill {skillLevel}";
		return output;
	}
	#endregion
}