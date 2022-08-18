namespace B1NARY
{
	using UnityEngine;
	using System;
	using System.Threading.Tasks;
	using System.Diagnostics;
	using UI;
	using System.Text;
	using System.Linq;
	using System.Collections;
	using System.Threading;
	using B1NARY.Logging;

	[RequireComponent(typeof(FadeController))]
	public class MainMenuCommands : MonoBehaviour
	{
		/// <summary>
		/// Determines if <paramref name="conditions"/> has managed to get all their
		/// <see cref="TaskCompletionSource{TResult}.Task"/>s completed.
		/// </summary>
		public static bool IsAllCompleted<T>(TaskCompletionSource<T>[] conditions) =>
			!conditions.Any(taskCompletion => !taskCompletion.Task.IsCompleted);

		// public int x, y;
		// [SerializeField] int maxX, maxY;
		// bool keyDown;
		[SerializeField]
		GameObject optionsMenu;
		[SerializeField]
		GameObject dialogueBox;
		FadeController mainMenuFadeController;

		private void Start()
		{
			mainMenuFadeController = GetComponent<FadeController>();
		}

		// BUTTON BEHAVIOURS
		public void NewGame()
		{
			B1NARYConsole.Log(name, "Initialized new game sequence.");
			mainMenuFadeController.FadeOut(0.5f);

			SceneManager.Initialize(1f);
			
		}
		public void LoadGame()
		{
			throw new NotImplementedException();
		}
		public void Options()
		{
			throw new NotImplementedException();
		}
		public void Exit() => Application.Quit();
	}
}