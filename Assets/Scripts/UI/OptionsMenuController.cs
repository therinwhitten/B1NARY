using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
public class OptionsMenuController : MonoBehaviour
{
    // public int x, y;
    // [SerializeField] int maxX, maxY;
    // bool keyDown;
    [SerializeField]
    GameObject mainMenu;
    // Start is called before the first frame update
    void Start()
    {
        // x = 0;
        // y = 0;


    }

    // BUTTON BEHAVIOURS

    void Back()
    {
        gameObject.SendMessage("fadeOut");
        mainMenu.SendMessage("fadeIn");
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
