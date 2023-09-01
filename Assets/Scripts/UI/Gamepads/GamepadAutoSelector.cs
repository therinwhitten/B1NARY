namespace B1NARY.UI.Gamepads
{
	using B1NARY.DesignPatterns;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.InputSystem;
	using UnityEngine.InputSystem.UI;

	public class GamepadAutoSelector : Singleton<GamepadAutoSelector>
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Constructor() => ThrowErrorIfEmpty = false;

		public EventSystem eventSystem;
		public InputSystemUIInputModule module;
		public PlayerInput playerInput; 
		public bool IsGamepadEnabled => playerInput.currentControlScheme == gamepadScheme;
		public string gamepadScheme = "Gamepad";

		private void Reset()
		{
			eventSystem = GetComponentInChildren<EventSystem>(true);
			playerInput = GetComponentInChildren<PlayerInput>(true);
			module = GetComponentInChildren<InputSystemUIInputModule>(true);
		}

		private bool gamepadEnabledOnLastFrame = false;
		private GameObject lastObject = null;
		private GameObject GetNewAssigned() => lastObject == null ? AutoGamepadButtonSelector.Youngest.gameObject : lastObject;
		private void Update()
		{
			bool alreadySelecting = eventSystem.alreadySelecting;
			if (alreadySelecting && eventSystem.currentSelectedGameObject != null)
			{
				GameObject assignable = eventSystem.currentSelectedGameObject;
				if (assignable != null)
					lastObject = eventSystem.currentSelectedGameObject;
			}

			bool currentFrame = IsGamepadEnabled;
			if (currentFrame == gamepadEnabledOnLastFrame)
				return;
			gamepadEnabledOnLastFrame = currentFrame;
			if (currentFrame & !alreadySelecting)
			{
				lastObject = GetNewAssigned();
				Debug.Log(lastObject, lastObject);
				eventSystem.SetSelectedGameObject(lastObject);

			}
		}
	}
}
