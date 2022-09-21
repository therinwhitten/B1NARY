namespace B1NARY
{
	using UnityEngine;
	using UI;

	[RequireComponent(typeof(FadeController))]
	public sealed class MainMenuCommands : MonoBehaviour
	{
		FadeController mainMenuFadeController;

		private void Awake()
		{
			mainMenuFadeController = GetComponent<FadeController>();
		}

		// BUTTON BEHAVIOURS
		public void NewGame()
		{
			mainMenuFadeController.FadeOut(0.5f);
			SceneManager.Initialize();
		}
		public void Exit() => Application.Quit();
	}
}