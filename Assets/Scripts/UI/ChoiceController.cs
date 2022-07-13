using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FadeController))]
public class ChoiceController : MonoBehaviour
{
	GameObject panel;
	private FadeController fadeController;
	void Start()
	{
		panel = GameObject.Find("Choices");
		fadeController = GetComponent<FadeController>();
	}

	public void newChoice()
	{
		foreach (Transform child in panel.transform)
		{
			Destroy(child.gameObject);
		}
		// generate a button for each choice from the parser
		foreach (string choice in ScriptParser.Instance.currentNode.choices.Keys)
		{
			GameObject choiceButton = Instantiate(Resources.Load<GameObject>("UI/Choice/Choice Button"), panel.transform);
			ChoiceButton choiceScript = choiceButton.GetComponent<ChoiceButton>();
			choiceScript.assignName(choice);
			choiceScript.node = ScriptParser.Instance.currentNode.choices[choice];
		}
		// show choice panel
		fadeController.FadeIn(0.1f);
	}
}
