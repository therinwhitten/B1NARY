namespace B1NARY.Editor
{
	using UnityEditor;

	[CustomEditor(typeof(UIThemeHandler))]
	public class UIThemeHandlerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var themeHandler = (UIThemeHandler)target;
			themeHandler.option = (UIThemeHandler.Option)EditorGUILayout.EnumPopup("Color Option", themeHandler.option);
			if (themeHandler.option == UIThemeHandler.Option.Custom)
				themeHandler.themeName = EditorGUILayout.DelayedTextField("Theme Name", themeHandler.themeName);
		}
	}
}