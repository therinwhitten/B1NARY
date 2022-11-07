namespace B1NARY.UI
{
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.Events;
	using System;
	using B1NARY.Scripting;
	using System.Collections.Generic;
	using TMPro;
	using B1NARY.DesignPatterns;
	using System.Collections;

	/// <summary>
	/// An interface in unity to allow the player to choose between options that
	/// is entirely written in the scripts, allowing to save on prefab loading cost
	/// and hard drive space.
	/// </summary>
	/// <remarks>
	/// This uses a singleton, as it is meant to be created within a canvas,
	/// such as the UI canvas. DO NOT CREATE A NEW CHOICE PANEL. Instead, use
	/// <see cref="Initialize(IEnumerable{string})"/> to use it, as it will have
	/// their own prefabs already defined.
	/// </remarks>
	[RequireComponent(typeof(VerticalLayoutGroup))]
	public sealed class ChoicePanel : Singleton<ChoicePanel>, IDisposable
	{
		/// <summary>
		/// Uses an existing instance to create a panel of choices.
		/// </summary>
		/// <param name="choices"> The choices to iterate through. </param>
		/// <returns> The choice panel that has been selected to do the bidding. </returns>
		public static ChoicePanel StartNew(IEnumerable<string> choices)
		{
			ChoicePanel panel = Instance;
			panel.Initialize(choices);
			return panel;
		}


		[SerializeField, Tooltip("The prefab will need: 'Button' and 'Text'.")]
		private GameObject choiceButtonPrefab;

		/// <summary>
		/// If the <see cref="MonoBehaviour"/> has been initialized, important
		/// for using <see cref="IDisposable"/>.
		/// </summary>
		private bool hasBeenInitialized = false;
		/// <summary>
		/// All the buttons attached to the choice panel. Usually they can also
		/// be achieved via <see cref="Transform.GetChild(int)"/> with count
		/// enumeration.
		/// </summary>
		private List<(GameObject obj, TMP_Text text, Button button)> choiceButtons;
		/// <summary>
		/// What to do when the player picks up a choice when they press the button.
		/// Modifies <see cref="CurrentlyPickedChoice"/> to reflect firsthand.
		/// </summary>
		public event Action<string> PickedChoice;
		/// <summary>
		/// A nullable string that depicts what choice the player chose. Should
		/// be used with a <see cref="Dictionary{string, object}"/>
		/// </summary>
		public string CurrentlyPickedChoice { get; private set; } = null;
		/// <summary>
		/// If the player has even picked a choice yet.
		/// </summary>
		public bool HasPickedChoice => !string.IsNullOrEmpty(CurrentlyPickedChoice);

		private int additiveSlots = -1;

		private void Awake()
		{
			choiceButtons = new List<(GameObject obj, TMP_Text, Button button)>(2);
		}

		/// <summary>
		/// Starts up a new choice box for the player to pick up.
		/// </summary>
		/// <param name="choices"> All the choices to enumerate through. </param>
		/// <exception cref="InvalidOperationException">
		/// When the current panel is not yet disposed or is already initialized.
		/// </exception>
		public void Initialize(IEnumerable<string> choices)
		{
			if (hasBeenInitialized)
				throw new InvalidOperationException($"{nameof(ChoicePanel)} is already" +
					" been used to make a choice panel. Did you forget to dispose?");
			hasBeenInitialized = true;
			enabled = true;
			ScriptHandler.Instance.ShouldPause = true;
			gameObject.SetActive(true);
			PickedChoice = key =>
			{
				for (int i = 0; i < choiceButtons.Count; i++)
					choiceButtons[i].obj.SetActive(false);
				Debug.Log($"Picked Choice: {key}", this);
			};
			StartCoroutine(WaitForAdditive());
			var enumerator = choices.GetEnumerator();
			for (int i = 0; enumerator.MoveNext(); i++)
			{
				if (i <= choiceButtons.Count)
				{
					GameObject obj = Instantiate(choiceButtonPrefab, transform);
					choiceButtons.Add((obj, obj.GetComponentInChildren<TMP_Text>(), obj.GetComponentInChildren<Button>()));
				}
				choiceButtons[i].obj.SetActive(true);
				choiceButtons[i].text.text = enumerator.Current;
				for (int ii = 0; ii <= i; ii++)
					choiceButtons[i].button.onClick.AddListener((UnityAction)(() => additiveSlots++));
			}

			IEnumerator WaitForAdditive()
			{
				while (additiveSlots < 0)
				{
					yield return new WaitForFixedUpdate();
				}
				ScriptHandler.Instance.ShouldPause = false;
				HandlePress(choiceButtons[additiveSlots].text.text);
			}
		}
		/// <summary>
		/// What the button sends out when pressed. Make sure you to not use 
		/// referencing your values if you use method capturing!
		/// </summary>
		/// <param name="value"> The key to send out when pressed. </param>
		public void HandlePress(string value)
		{
			if (!hasBeenInitialized)
				return;
			CurrentlyPickedChoice = value;
			PickedChoice.Invoke(value);
		}

		public void Dispose()
		{
			if (!hasBeenInitialized)
				return;
			hasBeenInitialized = false;
			CurrentlyPickedChoice = null;
			ScriptHandler.Instance.ShouldPause = false;
			gameObject.SetActive(false);
			enabled = false;
			for (int i = 0; i < choiceButtons.Count; i++)
				choiceButtons[i].obj.SetActive(false);
			choiceButtons.Clear();
			PickedChoice = null;
		}

		private void OnDestroy()
		{
			Dispose();
		}
	}
}