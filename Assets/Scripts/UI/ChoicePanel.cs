namespace B1NARY.UI
{
	using UnityEngine;
	using UnityEngine.UI;
	using System;
	using B1NARY.Scripting;
	using B1NARY.DesignPatterns;
	using CharacterManager = B1NARY.CharacterManagement.CharacterManager;
	using System.Collections.Generic;
	using B1NARY.DataPersistence;

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
		public static ChoicePanel StartNew(IEnumerable<ScriptLine> choices)
		{
			ChoicePanel panel = Instance;
			panel.Initialize(choices);
			return panel;
		}


		[SerializeField, Tooltip("The prefab will need: 'Button' and 'Text' and 'ChoiceButton'.")]
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
		private List<ChoiceButton> choiceButtons;
		/// <summary>
		/// What to do when the player picks up a choice when they press the button.
		/// Modifies <see cref="CurrentlyPickedChoice"/> to reflect firsthand.
		/// </summary>
		public event Action<ScriptLine> PickedChoice;
		/// <summary>
		/// A nullable string that depicts what choice the player chose. Should
		/// be used with a <see cref="Dictionary{string, object}"/>
		/// </summary>
		public ScriptLine? CurrentlyPickedChoice { get; private set; } = null;
		/// <summary>
		/// If the player has even picked a choice yet.
		/// </summary>
		public bool HasPickedChoice => CurrentlyPickedChoice.HasValue;

		private void Awake()
		{
			choiceButtons = new List<ChoiceButton>(2);
		}

		/// <summary>
		/// Starts up a new choice box for the player to pick up.
		/// </summary>
		/// <param name="choices"> All the choices to enumerate through. </param>
		/// <exception cref="InvalidOperationException">
		/// When the current panel is not yet disposed or is already initialized.
		/// </exception>
		public void Initialize(IEnumerable<ScriptLine> choices)
		{
			if (hasBeenInitialized)
				throw new InvalidOperationException($"{nameof(ChoicePanel)} is already" +
					" been used to make a choice panel. Did you forget to dispose?");
			hasBeenInitialized = true;
			enabled = true;
			ScriptHandler.Instance.pauser.Pause();
			gameObject.SetActive(true);
			PickedChoice = key =>
			{
				for (int i = 0; i < choiceButtons.Count; i++)
					choiceButtons[i].gameObject.SetActive(false);
#if DEBUG
				Debug.Log($"Picked Choice: {key}", this);
#endif
			};
			using (IEnumerator<ScriptLine> enumerator = choices.GetEnumerator())
				for (int i = 0; enumerator.MoveNext(); i++)
				{
					GameObject obj = Instantiate(choiceButtonPrefab, transform);
					choiceButtons.Add(obj.GetComponent<ChoiceButton>());
					obj.SetActive(true);
					choiceButtons[i].Text = enumerator.Current.RawLine;
					choiceButtons[i].tiedPanel = this;
					choiceButtons[i].VoiceActor = CharacterManager.Instance.ActiveCharacter.controller.VoiceData;
					choiceButtons[i].currentLine = enumerator.Current;
				}
		}
		/// <summary>
		/// What the button sends out when pressed. Make sure you to not use 
		/// referencing your values if you use method capturing!
		/// </summary>
		/// <param name="value"> The key to send out when pressed. </param>
		public void HandlePress(ScriptLine value)
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
			ScriptHandler.Instance.pauser.Play();
			gameObject.SetActive(false);
			enabled = false;
			for (int i = 0; i < choiceButtons.Count; i++)
				Destroy(choiceButtons[i].gameObject);
			choiceButtons.Clear();
			PickedChoice = null;
		}

		private void OnDestroy()
		{
			Dispose();
		}
	}
}