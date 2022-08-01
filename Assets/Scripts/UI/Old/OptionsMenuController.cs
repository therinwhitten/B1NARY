using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuController : MonoBehaviour
{
    // public int x, y;
    // [SerializeField] int maxX, maxY;
    // bool keyDown;
    [SerializeField]
    GameObject mainMenu;

    [SerializeField]
    GameObject[] audioOptions;
    // Awake is called before the first frame update
    void Start()
    {
        // x = 0;
        // y = 0;
        openSettings();

    }

    // BUTTON BEHAVIOURS

    void Back()
    {
        gameObject.SendMessage("FadeOut");
        mainMenu.SendMessage("FadeIn");
        openSettings();
    }

    void openSettings()
    {
        foreach (GameObject audioOption in audioOptions)
        {
            Slider slider = audioOption.GetComponentInChildren<Slider>();

            slider.value = PlayerPrefs.GetFloat(audioOption.name, 1f);
        }
    }

    void Save()
    {
        foreach (GameObject audioOption in audioOptions)
        {
            Slider slider = audioOption.GetComponentInChildren<Slider>();
            PlayerPrefs.SetFloat(audioOption.name, slider.value);
        }
        PlayerPrefs.Save();
        Back();
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
