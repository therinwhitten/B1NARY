namespace HDConsole
{
	using System;
	using System.Text;
	using UnityEngine.Diagnostics;
	using UnityEngine;
	using System.IO;
	using UnityEngine.SceneManagement;

	public  static partial class CoreCommands
	{
		[return: CommandsFromGetter]
		public static HDCommand[] GameObjectCommands() => new HDCommand[]
		{
			new HDCommand($"{HDCommand.GAMEOBJECT_PREFIX}_fire", new string[] { "Gameobject Name" }, (args) =>
			{
				GameObject obj = GameObject.Find(args[0]);
				if (obj == null)
					throw new MissingReferenceException($"Object '{args[0]}' is not found");
				UnityEngine.Object.Destroy(obj);
			})
			{
				description = "Snaps an object out of existence.",
				mainTags = HDCommand.MainTags.ServerModOnly,
			},

			new HDCommand($"{HDCommand.GAMEOBJECT_PREFIX}_load", new string[] { "Gameobject Name", "x", "y", "z" }, (args) =>
			{
				GameObject obj = Resources.Load<GameObject>(args[0]);
				if (obj == null)
					throw new MissingReferenceException($"Object '{args[0]}' is not found in resources");
				obj = UnityEngine.Object.Instantiate(obj);
				obj.transform.position = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
			})
			{
				description = "Loads (and Instantiates) a resource object into the current scene played back.",
				mainTags = HDCommand.MainTags.ServerModOnly,
			},

			new HDCommand($"{HDCommand.GAMEOBJECT_PREFIX}_move", new string[] { "Gameobject Name", "x", "y", "z" }, (args) =>
			{
				GameObject obj = GameObject.Find(args[0]);
				if (obj == null)
					throw new MissingReferenceException($"Object '{args[0]}' is not found");
				obj.transform.position = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
			})
			{
				description = "Moves a specific gameobject's position in the world.",
				mainTags = HDCommand.MainTags.ServerModOnly,
			},

			new HDCommand($"{HDCommand.GAMEOBJECT_PREFIX}_display_all_root_objects", (args) =>
			{
				GameObject[] allObj = SceneManager.GetActiveScene().GetRootGameObjects();
				StringBuilder builder = new("All Scene Objects: \n");
				for (int i = 0; i < allObj.Length; i++)
					builder.AppendLine($"\t{allObj[i].name}");
				HDConsole.WriteLine(builder.ToString());
			}) { description = "Displays all root objects in the current scene being played back." },


		};
	}
}