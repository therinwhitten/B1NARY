using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceController : MonoBehaviour
{
    GameObject panel;
    // Start is called before the first frame update
    void Start()
    {
        panel = GameObject.Find("Choices");
    }

    public void newChoice()
    {
        // clear out previous choices if there are any
        foreach (Transform child in panel.transform)
        {
            Destroy(child.gameObject);
        }
        // generate a button for each choice from the parser
        foreach (string choice in ScriptParser.Instance.currentChoiceOptions.Keys)
        {
            GameObject choiceButton = Instantiate(Resources.Load<GameObject>("UI/Choice/Choice Button"), panel.transform);
            ChoiceButton choiceScript = choiceButton.GetComponent<ChoiceButton>();
            choiceScript.assignName(choice);
        }
        // show choice panel
        gameObject.SendMessage("fadeIn");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
