using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorButton : MonoBehaviour
{
    public string value;
    public void selectScriptFile()
    {
        MenuCommands.Instance.startingScript = value;
    }
    public void selectSceneFile()
    {
        MenuCommands.Instance.startingScene = value;
    }
}
