using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

[RequireComponent(typeof(FadeController))]
public class MainMenuCommands : MonoBehaviour
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
		FadeController.FadeInAndActivate(dialogueBox.GetComponent<FadeController>(), 0.5f);
		fadeController.FadeOutAndDeActivate(0.5f);
	}
	public void LoadGame()
	{
		throw new NotImplementedException();
	}
	public void Options()
	{
		throw new NotImplementedException();
	}
	public void Exit() => GameCommands.QuitGame();

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
