using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
public class MainMenuController : MonoBehaviour
{
    // public int x, y;
    // [SerializeField] int maxX, maxY;
    // bool keyDown;
    [SerializeField]
    GameObject optionsMenu;
    [SerializeField]
    GameObject dialogueBox;

    // Start is called before the first frame update
    void Start()
    {
        // x = 0;
        // y = 0;


    }

    // BUTTON BEHAVIOURS
    void NewGame()
    {
        AudioManager.Instance.Play("Game-Start");
        gameObject.SendMessage("fadeOut");
        DialogueSystem.Instance.initialize();
        ScriptParser.Instance.initialize();
        dialogueBox.SendMessage("fadeIn");
    }
    void Options()
    {
        gameObject.SendMessage("fadeOut");
        optionsMenu.SendMessage("fadeIn");
        optionsMenu.SendMessage("openSettings");
    }
    void Exit()
    {
        Debug.Log("Quitting");
        // EditorApplication.isPlaying = false;
        Application.Quit();
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
