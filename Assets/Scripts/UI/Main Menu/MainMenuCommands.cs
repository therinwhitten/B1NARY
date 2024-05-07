namespace B1NARY
{
	using UnityEngine;
	using UI;
	using System.Threading.Tasks;
	using System.Collections;
	using B1NARY.Scripting;

	public class MainMenuCommands : MonoBehaviour
	{
		// Buttons
		public void NewGame()
		{
			ScriptHandler.Instance.pauser?.Dispose();
			ScriptHandler.Instance.pauser = new Pauser();
			SceneManager.Instance.InitializeGame();
		}
		public void NewGame(string streamingAssetsFilePath)
		{
			ScriptHandler.Instance.pauser?.Dispose();
			ScriptHandler.Instance.pauser = new Pauser();
			SceneManager.Instance.InitializeScript(streamingAssetsFilePath);
		}
		public void QuitToMainMenu()
		{
			CoroutineWrapper wrapper = new(SceneManager.InstanceOrDefault, SceneManager.InstanceOrDefault.ReturnToMainMenu());
			wrapper.AfterActions += (mono) =>
			{
				if (!ScriptHandler.TryGetInstance(out var handler))
					return;
				handler.document = null;
				handler.documentWatcher = null;
			};
			wrapper.Start();
		}
		public void QuitGame()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.ExitPlaymode();
#else
			Application.Quit();
#endif
		}

	}
}
#if UNITY_EDITOR
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
			if (AddButton("Quit To Main Menu") && Application.isPlaying)
				commands.QuitToMainMenu();
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
#endif