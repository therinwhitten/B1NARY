namespace B1NARY.UI
{
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Events;
	using System;
	using System.Text;
	using System.Threading.Tasks;
	using B1NARY.Scripting;
	using System.Collections.Generic;
	using System.Linq;

	[RequireComponent(typeof(FadeController))]
	public sealed class ChoicePanel : MonoBehaviour, IDisposable
	{
		public static ChoicePanel StartNew(IEnumerable<string> choices)
		{
			ChoicePanel panel = GameObject.FindObjectOfType<ChoicePanel>();
			panel.Initialize(choices);
			return panel;
		}


		private FadeController fadeController;
		[SerializeField, Tooltip("The prefab will need: 'Button' and 'Text'.")]
		private GameObject choiceButtonPrefab;

		private bool hasBeenInitialized = false;
		private GameObject[] choiceButtons;
		public event Action<string> PickedChoice;

		private void Awake()
		{
			fadeController = GetComponent<FadeController>();
		}

		public void Initialize(IEnumerable<string> choices)
		{
			if (hasBeenInitialized)
				throw new InvalidOperationException($"{nameof(ChoicePanel)} is already" +
					" been used to make a choice panel. Did you forget to dispose?");
			hasBeenInitialized = true;
			choiceButtons = new GameObject[choices.Count()];
			choiceButtons = choices
				.Select(choice =>
				{
					GameObject button = Instantiate(choiceButtonPrefab, transform);
					button.GetComponentInChildren<Text>().text = choice;
					Action action = () => HandlePress(choice); // Capturing value.
					button.GetComponentInChildren<Button>().onClick
						.AddListener(new UnityAction(action));
					return button;
				}).ToArray();
		}
		/*
		private Task ConsoleBuilder(string[] choices)
		{
			var consoleBuilder = new StringBuilder("Starting new choice panel with options:");
			for (int i = 0; i < choices.Length; i++)
				consoleBuilder.Append($"\n{choices[i]}");
			Debug.Log(name + ": " + consoleBuilder.ToString());
			return Task.CompletedTask;
		}
		*/

		public void HandlePress(string value)
		{
			if (!hasBeenInitialized)
				return;
			PickedChoice?.Invoke(value);
		}

		public void Dispose()
		{
			if (!hasBeenInitialized)
				return;
			hasBeenInitialized = false;
			fadeController.FadeOut(0.1f);
			choiceButtons = null;
			PickedChoice = null;
			foreach (GameObject obj in choiceButtons)
				Destroy(obj);
		}

		private void OnDestroy()
		{
			Dispose();
		}
	}
}