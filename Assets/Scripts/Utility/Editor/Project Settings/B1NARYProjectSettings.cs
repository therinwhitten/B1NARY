namespace B1NARY.Editor.Project
{
	using UnityEditor;
	using UnityEngine;

	[CreateAssetMenu(fileName = "B1NARY Project Settings", menuName = "B1NARY/Project Settings")]
	public class B1NARYProjectSettings : ScriptableObject
	{
		public static void Load()
		{
			const string name = "B1NARY Project Settings";
			var settings = Resources.Load<B1NARYProjectSettings>(name);
			if (settings == null)
			{
				settings = CreateInstance<B1NARYProjectSettings>();
			}
		}
	}
}