namespace B1NARY.Editor.Project
{
	using System.Collections.Generic;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;

	[CreateAssetMenu(fileName = "B1NARY Project Settings", menuName = "B1NARY/Project Settings")]
	public class B1NARYProjectSettings : ScriptableObject
	{
		public static B1NARYProjectSettings Instance
		{
			get
			{
				const string name = "B1NARY Project Settings";
				var settings = Resources.Load<B1NARYProjectSettings>(name);
				if (settings == null)
				{
					settings = CreateInstance<B1NARYProjectSettings>();
				}
				return settings;
			}
		}
		public string[] languages = { "English" };
		public IReadOnlyDictionary<string, string> StartupScriptPaths =>
			languages.Zip(startupScriptPaths, (language, scriptPath) => new KeyValuePair<string, string>(language, scriptPath))
			.ToDictionary(pair => pair.Key, pair => pair.Value);
		public string[] startupScriptPaths;
	}
}