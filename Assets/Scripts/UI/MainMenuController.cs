using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FadeController))]
public class MainMenuController : MonoBehaviour
{
	// public int x, y;
	// [SerializeField] int maxX, maxY;
	// bool keyDown;
	[SerializeField]
	GameObject optionsMenu;
	[SerializeField]
	GameObject dialogueBox;
	FadeController fadeController;

	private void Start()
	{
		fadeController = GetComponent<FadeController>();
	}


	// BUTTON BEHAVIOURS
	public void NewGame()
	{
		DialogueSystem.Instance.initialize();
		ScriptParser.Instance.initialize();
		dialogueBox.GetComponent<FadeController>().FadeIn(0.5f);
	}
	public void Options()
	{
		optionsMenu.SendMessage("FadeIn");
		optionsMenu.SendMessage("openSettings");
	}
	public void Exit()
	{
		Debug.Log("Quitting");
		// EditorApplication.isPlaying = false;
		GameCommands.QuitGame();
	}

	// keeping this commented since I don't need it for now. May revisit in the future if we need a keyboard UI
	// // Update is called once per frame
	// void Update()
	// {
	//     if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Vertical") != 0)
	//     {
	//         if (!keyDown)
	//         {
	//             if (Input.GetAxis("Vertical") < 0)
	//             {
	//                 if (y < maxY)
	//                 {
	//                     y++;
	//                 }
	//                 else
	//                 {
	//                     y = 0;
	//                 }
	//             }
	//             else if (Input.GetAxis("Vertical") > 0)
	//             {
	//                 if (y > 0)
	//                 {
	//                     y--;
	//                 }
	//                 else
	//                 {
	//                     y = maxY;
	//                 }
	//             }
	//             keyDown = true;
	//         }
	//     }
	//     else
	//     {
	//         keyDown = false;
	//     }
	// }

}
