#if UNITY_EDITOR
namespace B1NARY.Editor.Project
{
	using UnityEditor;
	using UnityEngine;

	public static class ProjectSettingsProvider
	{
		[SettingsProvider]
		public static SettingsProvider ProvideSettings()
		{
			return new SettingsProvider("Project/B1NARY", SettingsScope.Project)
			{
				guiHandler = searchContext =>
				{
					EditorGUILayout.LabelField("Bruh");
				}
			};
		}
	}
}
#endif