namespace B1NARY
{
	using UnityEngine;
	using UI;
	using System.Threading.Tasks;
	using System.Collections;

	public sealed class MainMenuCommands : MonoBehaviour
	{

		// Buttons
		public void NewGame()
		{
			StartCoroutine(MainThreadCoroutine());
			// DOTween's debugger stuffs is terrible, easy work-around while allowing
			// - it to be on the main thread for unity to be happy about it.
			IEnumerator MainThreadCoroutine()
			{
				SceneManager.Initialize();
				yield break;
			}
		}
		public void Exit() => Application.Quit();
	}
}