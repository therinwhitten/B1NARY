namespace B1NARY
{
	using UnityEngine;
	using UI;

	public  class MainMenuCommands : MonoBehaviour 
	{
		// Buttons
		public void NewGame()
		{
			SceneManager.Initialize();
		}
		public void  QuitGame ()
		{
			Debug.Log("Quitting!");
			Application.Quit();
		}
		
	}
}