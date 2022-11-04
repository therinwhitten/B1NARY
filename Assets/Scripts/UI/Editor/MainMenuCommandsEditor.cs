namespace B1NARY.UI.Editor
{
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(MainMenuCommands), true)]
	public class MainMenuCommandsEditor : Editor
	{
		protected MainMenuCommands commands;
		protected virtual void Awake() => commands = (MainMenuCommands)target;
		public override void OnInspectorGUI()
		{
			if (AddButton("New Game") && Application.isPlaying)
				commands.NewGame();
			if (AddButton("Quit Game") && Application.isPlaying)
				commands.QuitGame();
		}

		protected bool AddButton(string label)
		{
			Rect rect = GUILayoutUtility.GetRect(Screen.width, 20f);
			rect = EditorGUI.IndentedRect(rect);
			rect.xMax -= 2;
			rect.xMin += 2;
			return GUI.Button(rect, label);
		}
	}
}
