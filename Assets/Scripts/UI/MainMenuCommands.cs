namespace B1NARY
{
	using UnityEngine;
	using UI;

	public sealed class MainMenuCommands : MonoBehaviour
	{

		// Buttons
		public void NewGame()
		{
			SceneManager.Initialize();
		}
		public void Exit() => Application.Quit();
	}
}