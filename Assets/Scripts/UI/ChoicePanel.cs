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
	using TMPro;
	using B1NARY.DesignPatterns;

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

		private bool hasBeenInitialized = false;
		private List<GameObject> choiceButtons;
		public event Action<string> PickedChoice;
		/// <summary>
		/// A nullable string that depicts what choice the player chose. Should
		/// be used with a <see cref="Dictionary{string, object}"/>
		/// </summary>
		public string CurrentlyPickedChoice { get; private set; } = null;

		private void OnEnable()
		{
			Debug.Log($"{name} with {nameof(ChoicePanel)} is enabled!");
		}
		private void OnDisable()
		{
			Debug.Log($"{name} with {nameof(ChoicePanel)} is disabled!");
		}

		public void Initialize(IEnumerable<string> choices)
		{
			if (hasBeenInitialized)
				throw new InvalidOperationException($"{nameof(ChoicePanel)} is already" +
					" been used to make a choice panel. Did you forget to dispose?");
			hasBeenInitialized = true;
			enabled = true;
			ScriptHandler.Instance.ShouldPause = true;
			gameObject.SetActive(true);
			var enumerator = choices.GetEnumerator();
			for (int i = 0; enumerator.MoveNext(); i++)
			{
				GameObject button = Instantiate(choiceButtonPrefab, transform);
				button.GetComponentInChildren<TMP_Text>().text = enumerator.Current;
				void HandlePress() => this.HandlePress(enumerator.Current); // Capturing value.
				button.GetComponentInChildren<Button>().onClick
					.AddListener(new UnityAction((Action)HandlePress));
				choiceButtons.Add(button);
			}
		}

		public void HandlePress(string value)
		{
			if (!hasBeenInitialized)
				return;
			CurrentlyPickedChoice = value;
			PickedChoice?.Invoke(value);
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
				Destroy(choiceButtons[i]);
			choiceButtons.Clear();
			PickedChoice = null;
		}

		private void OnDestroy()
		{
			Dispose();
		}
	}
}