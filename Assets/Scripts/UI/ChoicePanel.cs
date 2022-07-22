using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

[RequireComponent(typeof(FadeController))]
public sealed class ChoicePanel : MonoBehaviour, IDisposable
{
	private FadeController fadeController;
	[SerializeField, Tooltip("The prefab will need: 'Button' and 'Text'.")]
	private GameObject choiceButtonPrefab;

	private bool hasBeenInitialized = false;
	private (GameObject @object, DialogueNode node)[] choiceButtons;
	public event Action<string> PickedChoice;

	private void Awake()
	{
		fadeController = GetComponent<FadeController>();
	}

	public ChoicePanel Initialize(string[] choices)
	{
		if (hasBeenInitialized)
			throw new InvalidOperationException($"{nameof(ChoicePanel)} is already" +
				" been used to make a choice panel. Did you forget to dispose?");
		hasBeenInitialized = true;
		choiceButtons = new (GameObject, DialogueNode)[choices.Length];
		for (byte i = 0; i < choices.Length; i++)
		{
			choiceButtons[i] = (Instantiate(choiceButtonPrefab, transform),
				ScriptParser.Instance.currentNode.choices[choices[i]]);
			choiceButtons[i].@object.GetComponent<Text>().text = choices[i];
			Action action = () => HandlePress(i); // Capturing value.
			choiceButtons[i].@object.GetComponent<Button>().onClick
				.AddListener(new UnityAction(action));
		}
		return this;
	}

	public void HandlePress(byte index)
	{
		if (!hasBeenInitialized)
			return;
		// Dunno why we could just use the dialogueNodes from the dictionary
		// - values, but im afraid it will just break instantly due to how 
		// - volatile it may be.
		ScriptParser.Instance.currentNode.selectChoice(choiceButtons[index].node);
		PickedChoice?.Invoke(choiceButtons[index].node.GetCurrentLine().line);
	}

	public void Dispose()
	{
		if (!hasBeenInitialized)
			return;
		hasBeenInitialized = false;
		fadeController.FadeOut(0.1f);
		choiceButtons = null;
		PickedChoice = null;
		foreach (var pair in choiceButtons)
			Destroy(pair.@object);
	}
}
