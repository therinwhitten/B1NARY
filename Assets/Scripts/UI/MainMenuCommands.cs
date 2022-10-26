namespace B1NARY
{
	using UnityEngine;
	using UI;
	using System.Threading.Tasks;
	using System.Collections;

	public  class MainMenuCommands : MonoBehaviour 
	{
		// Buttons
		public void NewGame()
		{
			SceneManager.Instance.InitializeGame();
		}
		public void  QuitGame ()
		{
			Debug.Log("Quitting!");
			Application.Quit();
		}
		
	}
}